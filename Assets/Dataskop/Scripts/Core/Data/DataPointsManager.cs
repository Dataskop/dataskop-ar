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
		public UnityEvent hasFilteredByDate;

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
		[SerializeField] private float nearbyDevicesScanInterval;

		private bool hasHistoryEnabled;

		public float NearbyDevicesDistance => nearbyDevicesDistance;

		/// <summary>
		/// List of currently placed markers in the AR world.
		/// </summary>
		public IList<DataPoint> DataPoints { get; private set; }

		public Dictionary<Device, Vector3> LastKnownDevicePositions { get; private set; }

		/// <summary>
		/// Array of precise marker locations in the AR world.
		/// </summary>
		private Vector2d[] DataPointsLocations { get; set; }

		private DataAttributeManager DataAttributeManager => dataAttrRepo;

		private VisualizationRepository VisualizationRepository => visRepository;

		private AuthorRepository AuthorRepository => authorRepository;

		private bool HasLoadedDataPoints { get; set; }

		private DataManager DataManager => dataManager;

		private TimeRange? TimeRangeFilter { get; set; }

		private void Awake() {

			DataManager.HasUpdatedMeasurementResults += OnMeasurementResultsUpdated;
			DataManager.HasDateFiltered += OnDataFiltered;

		}

		private void Start() {

			inputHandler.WorldPointerUpped += OnSwiped;
			LastKnownDevicePositions = new Dictionary<Device, Vector3>();

		}

		private void FixedUpdate() {

			if (!HasLoadedDataPoints) {
				return;
			}

			if (AppOptions.DemoMode) {
				return;
			}

			for (int i = 0; i < DataPoints.Count; i++) {
				DataPoints[i].transform.localPosition = map.GeoToWorldPosition(DataPointsLocations[i]);
			}

		}

		public void OnProjectSelected() {

			if (!HasLoadedDataPoints) {
				return;
			}

			ClearDataPoints();
			hasHistoryEnabled = false;
		}

		public void OnHistoryViewChanged(bool enable) {

			hasHistoryEnabled = enable;
			DataManager.ShouldRefetch = !enable;

			foreach (DataPoint dp in DataPoints) {
				dp.ToggleHistory(enable);
			}

		}

		public void OnHistorySliderMoved(int newCount, int prevCount) {

			if (!HasLoadedDataPoints) {
				return;
			}

			foreach (DataPoint dp in DataPoints) {
				dp.SetIndex(newCount);
			}

		}

		public void OnAttributesInitialized(Project projectData) {

			if (projectData.Devices == null) {
				return;
			}

			if (HasLoadedDataPoints) {
				ClearDataPoints();
			}

			DataPointsLocations = new Vector2d[projectData.Devices.Count];
			InitializeDataPoints();

			HasLoadedDataPoints = true;
			StartCoroutine(GetNearbyDevicesTask(nearbyDevicesScanInterval));

		}

		public void OnVisualizationSelected(VisualizationOption visOpt) {

			foreach (DataPoint dp in DataPoints) {
				SetDataPointVisualization(dp, visOpt);
				dp.ToggleHistory(hasHistoryEnabled);
			}

			onVisualizationChanged?.Invoke(visOpt);

		}

		public void OnAttributeChanged(DataAttribute attribute) {

			if (HasLoadedDataPoints) {
				ClearDataPoints();
			}

			InitializeDataPoints();

			if (hasHistoryEnabled) {
				foreach (DataPoint dp in DataPoints) {
					dp.ToggleHistory(true);
				}
			}

			HasLoadedDataPoints = true;
			StartCoroutine(GetNearbyDevicesTask(nearbyDevicesScanInterval));

		}

		private void InitializeDataPoints() {

			DataPoints = new List<DataPoint>();

			Device[] projectDevices = DataManager.SelectedProject.Devices.ToArray();

			for (int i = 0; i < projectDevices.Length; i++) {

				if (projectDevices[i].Position == null) {
					continue;
				}

				if (DataAttributeManager.SelectedAttribute.ID == "all") {

					//If it is the same, create a datapoint instance
					DataPoint dataPointInstance =
						Instantiate(dataPointPrefab, dataPointsContainer).GetComponent<DataPoint>();
					dataPointInstance.Attribute = DataAttributeManager.SelectedAttribute;
					dataPointInstance.MeasurementDefinition = projectDevices[i].MeasurementDefinitions.First();
					dataPointInstance.Device = projectDevices[i];
					dataPointInstance.AuthorRepository = AuthorRepository;
					dataPointInstance.FocusedIndexChangedByTap += OnIndexChangeRequested;
					dataPointInstance.FocusedMeasurement =
						dataPointInstance.MeasurementDefinition.LatestMeasurementResult;

					//Move the DataPoint to its location
					if (AppOptions.DemoMode) {

						Vector3 GetLastKnownDevicePosition(Device device) {
							return LastKnownDevicePositions.TryGetValue(device, out Vector3 position) ? position
								: new Vector3(-1000, -1000, -1000);
						}

						// Subtract Offset to place the Vis on top of the found images
						PlaceDataPoint(
							GetLastKnownDevicePosition(projectDevices[i]) - dataPointInstance.Vis.Offset,
							dataPointInstance.transform
						);

					}
					else {
						DataPointsLocations[i] = Conversions.StringToLatLon(projectDevices[i].Position.GetLatLong());
						PlaceDataPoint(DataPointsLocations[i], dataPointInstance.transform);
					}

					DataPoints.Add(dataPointInstance);
					SetDataPointVisualization(
						dataPointInstance, DataAttributeManager.SelectedAttribute.VisOptions.First()
					);

				}
				else {

					foreach (MeasurementDefinition definition in projectDevices[i].MeasurementDefinitions) {

						//Check if the definition in the device has the same attribute as the currently selected attribute
						if (definition.AttributeId != DataAttributeManager.SelectedAttribute.ID) {
							continue;
						}

						//If it is the same, create a datapoint instance
						DataPoint dataPointInstance =
							Instantiate(dataPointPrefab, dataPointsContainer).GetComponent<DataPoint>();
						dataPointInstance.Attribute = DataAttributeManager.SelectedAttribute;
						dataPointInstance.MeasurementDefinition = definition;
						dataPointInstance.Device = projectDevices[i];
						dataPointInstance.AuthorRepository = AuthorRepository;
						dataPointInstance.FocusedIndexChangedByTap += OnIndexChangeRequested;
						dataPointInstance.FocusedMeasurement = definition.LatestMeasurementResult;

						//Move the DataPoint to its location
						if (AppOptions.DemoMode) {

							Vector3 GetLastKnownDevicePosition(Device device) {
								return LastKnownDevicePositions.TryGetValue(device, out Vector3 position) ? position
									: new Vector3(-1000, -1000, -1000);
							}

							// Subtract Offset to place the Vis on top of the found images
							PlaceDataPoint(
								GetLastKnownDevicePosition(projectDevices[i]) - dataPointInstance.Vis.Offset,
								dataPointInstance.transform
							);

						}
						else {
							DataPointsLocations[i] =
								Conversions.StringToLatLon(projectDevices[i].Position.GetLatLong());
							PlaceDataPoint(DataPointsLocations[i], dataPointInstance.transform);
						}

						DataPoints.Add(dataPointInstance);
						SetDataPointVisualization(
							dataPointInstance, DataAttributeManager.SelectedAttribute.VisOptions.First()
						);
					}

				}

			}

		}

		private void PlaceDataPoint(Vector2d newPosition, Transform dataPointTransform) {

			dataPointTransform.localPosition = map.GeoToWorldPosition(newPosition);

		}

		public void PlaceDataPoint(Vector3 newPosition, Transform dataPointTransform) {

			dataPointTransform.localPosition = newPosition;

		}

		public MeasurementResult GetLatestResult() {

			MeasurementResult latestResult = null;

			DateTime latestDate = new(1900, 1, 1);

			foreach (DataPoint dp in DataPoints) {
				foreach (MeasurementDefinition md in dp.Device.MeasurementDefinitions) {
					if (md.LatestMeasurementResult.Timestamp > latestDate) {
						latestDate = md.LatestMeasurementResult.Timestamp;
						latestResult = md.LatestMeasurementResult;
					}
				}
			}

			return latestResult;
		}

		public MeasurementResult GetEarliestResult() {

			MeasurementResult earliestResult = null;
			DateTime earliestDate = DateTime.Now;

			foreach (DataPoint dp in DataPoints) {
				foreach (MeasurementDefinition md in dp.Device.MeasurementDefinitions) {
					if (md.LatestMeasurementResult.Timestamp < earliestDate) {
						earliestDate = md.FirstMeasurementResult.Timestamp;
						earliestResult = md.FirstMeasurementResult;
					}
				}
			}

			return earliestResult;
		}

		private void ClearDataPoints() {

			HasLoadedDataPoints = false;

			foreach (DataPoint dp in DataPoints) {
				dp.FocusedIndexChangedByTap -= OnIndexChangeRequested;
				dp.RemoveVisualization();
				Destroy(dp.gameObject);
			}

			DataPoints.Clear();

		}

		private void SetDataPointVisualization(DataPoint dp, VisualizationOption visOpt) {

			if (VisualizationRepository.IsAvailable(visOpt.Type.FirstCharToUpper())) {

				GameObject vis = VisualizationRepository.GetVisualization(visOpt.Type.FirstCharToUpper());
				dp.RemoveVisualization();
				dp.Visualize(vis, TimeRangeFilter);
				dp.Vis.VisOption = visOpt;
				dp.Vis.ApplyStyle(dp.Vis.VisOption.Style);

			}
			else {
				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Info,
						Text = $"Could not find {visOpt.Type} Vis.",
						DisplayDuration = NotificationDuration.Medium
					}
				);
			}

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

		private void OnDataFiltered(TimeRange timeRange) {

			if (!HasLoadedDataPoints) {
				return;
			}

			TimeRangeFilter = timeRange;

			foreach (DataPoint dp in DataPoints) {
				dp.UpdateWithTimeRange(TimeRangeFilter.Value);
			}

			hasHistoryEnabled = true;
			hasFilteredByDate?.Invoke();

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

	}

}
