using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dataskop.Entities;
using Dataskop.Interaction;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Dataskop.Data {

	public class DataPointsManager : MonoBehaviour {

		[Header("Events")]
		public UnityEvent<VisualizationOption> onVisualizationChanged;
		public UnityEvent<int> dataPointHistorySwiped;
		public UnityEvent<int> nearbyDevicesUpdated;

		[Header("References")]
		[SerializeField] private DataManager dataManager;
		[SerializeField] private InputHandler inputHandler;
		[SerializeField] private AbstractMap map;
		[SerializeField] private GameObject dataPointPrefab;
		[SerializeField] private Transform dataPointsContainer;
		[SerializeField] private VisualizationRepository visRepository;
		[SerializeField] private DataAttributeManager dataAttrRepo;
		[SerializeField] private AuthorRepository authorRepository;

		[Header("Values")]
		[SerializeField] private float nearbyDevicesDistance;

		public float NearbyDevicesDistance => nearbyDevicesDistance;

		private bool hasHistoryEnabled;

		/// <summary>
		///     List of currently placed markers in the AR world.
		/// </summary>
		public IList<DataPoint> DataPoints { get; private set; }

		public Dictionary<Device, Vector3> LastKnownDevicePositions { get; private set; }

		/// <summary>
		///     Array of precise marker locations in the AR world.
		/// </summary>
		private Vector2d[] DataPointsLocations { get; set; }

		private DataAttributeManager DataAttributeManager => dataAttrRepo;

		private VisualizationRepository VisualizationRepository => visRepository;

		private AuthorRepository AuthorRepository => authorRepository;

		private bool HasLoadedDataPoints { get; set; }

		private DataManager DataManager => dataManager;

		private void Awake() {
			DataManager.HasUpdatedMeasurementResults += OnMeasurementResultsUpdated;
			DataManager.HasDateFiltered += OnDataFiltered;
		}

		private void Start() {

			inputHandler.WorldPointerUpped += OnSwiped;
			LastKnownDevicePositions = new Dictionary<Device, Vector3>();

		}

		private void FixedUpdate() {

			if (!HasLoadedDataPoints)
				return;

			if (AppOptions.DemoMode) return;

			for (int i = 0; i < DataPoints.Count; i++) {
				DataPoints[i].transform.localPosition = map.GeoToWorldPosition(DataPointsLocations[i]);
			}

		}

		private void SetDataPointVisualization(DataPoint dp, VisualizationOption visOpt) {

			if (VisualizationRepository.IsAvailable(visOpt.Type.FirstCharToUpper())) {

				GameObject vis = VisualizationRepository.GetVisualization(visOpt.Type.FirstCharToUpper());
				dp.RemoveVis();
				dp.SetVis(vis);
				dp.Vis.VisOption = visOpt;
				dp.Vis.ApplyStyle(dp.Vis.VisOption.Style);

			}
			else {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Info,
					Text = $"Could not find {visOpt.Type} Vis.",
					DisplayDuration = NotificationDuration.Medium
				});
			}

		}

		public void OnHistoryViewChanged(bool enable) {

			hasHistoryEnabled = enable;

			foreach (DataPoint dp in DataPoints) {
				ToggleTimeSeries(dp, enable);
			}

		}

		private static void ToggleTimeSeries(DataPoint dp, bool enable) {

			if (dp.Vis == null)
				return;

			if (dp.Vis.VisOption.Style.IsTimeSeries) {
				dp.Vis.OnTimeSeriesToggled(enable);
			}

		}

		private void OnSwiped(PointerInteraction pointerInteraction) {

			if (!pointerInteraction.isSwipe) {
				return;
			}

			if (pointerInteraction.startingGameObject == null) {
				return;
			}

			if (!HasLoadedDataPoints) {
				return;
			}

			if (DataPoints.Count < 1) {
				return;
			}

			if (!pointerInteraction.startingGameObject.CompareTag("VisObject")) {
				return;
			}

			foreach (DataPoint dp in DataPoints) {
				dp.Vis.OnSwipeInteraction(pointerInteraction);
			}

			dataPointHistorySwiped?.Invoke(DataPoints[0].FocusedIndex);

		}

		public void OnHistorySliderMoved(int newCount, int prevCount) {

			if (!HasLoadedDataPoints)
				return;

			foreach (DataPoint dp in DataPoints) {
				dp.SetIndex(newCount);
			}

		}

		public void OnAttributesUpdated(Project projectData) {

			if (projectData.Devices == null) {
				return;
			}

			if (HasLoadedDataPoints) {
				ClearDataPoints();
			}

			DataPointsLocations = new Vector2d[projectData.Devices.Count];
			SpawnDataPoints();

			HasLoadedDataPoints = true;
			StartCoroutine(GetNearbyDevicesTask(5));

		}

		public void OnVisualizationSelected(VisualizationOption visOpt) {

			foreach (DataPoint dp in DataPoints) {
				SetDataPointVisualization(dp, visOpt);
			}

			onVisualizationChanged?.Invoke(visOpt);

		}

		public void OnAttributeChanged(DataAttribute attribute) {

			if (HasLoadedDataPoints) {
				ClearDataPoints();
			}

			SpawnDataPoints();

			if (hasHistoryEnabled) {
				foreach (DataPoint dp in DataPoints) {
					ToggleTimeSeries(dp, true);
				}
			}

			HasLoadedDataPoints = true;
			StartCoroutine(GetNearbyDevicesTask(5));

		}

		private void SpawnDataPoints() {

			DataPoints = new List<DataPoint>();

			Device[] projectDevices = DataManager.SelectedProject.Devices.ToArray();

			for (int i = 0; i < projectDevices.Length; i++) {

				if (projectDevices[i].Position == null)
					continue;

				foreach (MeasurementDefinition definition in projectDevices[i].MeasurementDefinitions) {

					//Check if the definition in the device has the same attribute as the currently selected attribute
					if (definition.AttributeId != DataAttributeManager.SelectedAttribute.ID)
						continue;

					//If it is the same, create a datapoint instance
					DataPoint dataPointInstance = Instantiate(dataPointPrefab, dataPointsContainer).GetComponent<DataPoint>();
					dataPointInstance.Attribute = DataAttributeManager.SelectedAttribute;
					dataPointInstance.MeasurementDefinition = definition;
					dataPointInstance.Device = projectDevices[i];
					dataPointInstance.AuthorRepository = AuthorRepository;
					dataPointInstance.FocusedIndexChangedByTap += OnIndexChangeRequested;
					dataPointInstance.FocusedMeasurement = definition.GetMeasurementResult(0);

					//Move the DataPoint to its location
					if (AppOptions.DemoMode) {

						Vector3 GetLastKnownDevicePosition(Device device) {
							return LastKnownDevicePositions.TryGetValue(device, out Vector3 position) ? position
								: new Vector3(-1000, -1000, -1000);
						}

						// Subtract Offset to place the Vis on top of the found images
						PlaceDataPoint(GetLastKnownDevicePosition(projectDevices[i]) - dataPointInstance.Vis.Offset,
							dataPointInstance.transform);

					}
					else {
						DataPointsLocations[i] = Conversions.StringToLatLon(projectDevices[i].Position.GetLatLong());
						PlaceDataPoint(DataPointsLocations[i], dataPointInstance.transform);
					}

					DataPoints.Add(dataPointInstance);
					SetDataPointVisualization(dataPointInstance, DataAttributeManager.SelectedAttribute.VisOptions.First());
				}

			}

		}

		public void PlaceDataPoint(Vector2d newPosition, Transform dataPointTransform) {
			dataPointTransform.localPosition = map.GeoToWorldPosition(newPosition);
		}

		public void PlaceDataPoint(Vector3 newPosition, Transform dataPointTransform) {
			dataPointTransform.localPosition = newPosition;
		}

		private IEnumerator GetNearbyDevicesTask(float seconds) {
			while (HasLoadedDataPoints) {
				int count = GetDevicesNearPosition(inputHandler.MainCamera.transform.position);
				nearbyDevicesUpdated?.Invoke(count);
				yield return new WaitForSeconds(seconds);
			}
		}

		private int GetDevicesNearPosition(Vector3 position) {
			return DataPoints.Count(dp => Vector3.Distance(dp.transform.position, position) <= nearbyDevicesDistance);
		}

		private void OnIndexChangeRequested(int index) {

			if (!HasLoadedDataPoints) {
				return;
			}

			foreach (DataPoint dp in DataPoints) {
				dp.SetIndex(index);
			}

			dataPointHistorySwiped?.Invoke(index);

		}

		private void OnMeasurementResultsUpdated() {

			if (!HasLoadedDataPoints) {
				return;
			}

			foreach (DataPoint dp in DataPoints) {
				dp.OnMeasurementResultsUpdated();
			}

		}

		private void OnDataFiltered(DateTime from, DateTime to) {

			if (!HasLoadedDataPoints) {
				return;
			}

			foreach (DataPoint dp in DataPoints) {
				dp.OnDateFiltered(from, to);
			}

		}

		private void ClearDataPoints() {

			HasLoadedDataPoints = false;

			foreach (DataPoint dp in DataPoints) {
				dp.FocusedIndexChangedByTap -= OnIndexChangeRequested;
				dp.RemoveVis();
				Destroy(dp.gameObject);
			}

			DataPoints.Clear();
		}

	}

}