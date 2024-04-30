using System.Collections.Generic;
using System.Linq;
using DataskopAR.Interaction;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace DataskopAR.Data {

	public class DataPointsManager : MonoBehaviour {

#region Events

		[Header("Events")]
		public UnityEvent<VisualizationOption> onVisualizationChanged;
		public UnityEvent dataPointsResultsUpdated;
		public UnityEvent<int> dataPointHistorySwiped;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private DataManager dataManager;
		[SerializeField] private InputHandler inputHandler;
		[SerializeField] private AbstractMap map;
		[SerializeField] private GameObject dataPointPrefab;
		[SerializeField] private Transform dataPointsContainer;
		[SerializeField] private VisualizationRepository visRepository;
		[SerializeField] private DataAttributeManager dataAttrRepo;
		[SerializeField] private AuthorRepository authorRepository;

		private GameObject dummyVisObject;

#endregion

#region Properties

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

#endregion

#region Methods

		private void Awake() {
			DataManager.HasUpdatedMeasurementResults += OnMeasurementResultsUpdated;
		}

		private void Start() {

			inputHandler.WorldPointerUpped += OnSwiped;

			dummyVisObject = new GameObject {
				tag = "Vis"
			};

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

		public void SetDataPointVisualization(VisualizationOption visOpt) {

			if (VisualizationRepository.IsAvailable(visOpt.Type.FirstCharToUpper())) {

				GameObject vis = VisualizationRepository.GetVisualization(visOpt.Type.FirstCharToUpper());

				foreach (DataPoint dp in DataPoints) {
					ToggleTimeSeries(dp, false);
					dp.SetVis(vis);
					dp.Vis.VisOption = visOpt;
					dp.Vis.ApplyStyle(dp.Vis.VisOption.Style);
				}

				onVisualizationChanged?.Invoke(visOpt);

			}
			else {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Info,
					Text = $"Could not find {visOpt.Type} Vis.",
					DisplayDuration = NotificationDuration.Medium
				});
			}

		}

		public void OnHistoryViewChanged(bool isActive) {

			foreach (DataPoint dp in DataPoints) {
				ToggleTimeSeries(dp, isActive);
			}

		}

		private static void ToggleTimeSeries(DataPoint dp, bool isActive) {

			if (dp.Vis == null)
				return;

			if (dp.Vis.VisOption.Style.IsTimeSeries) {
				dp.Vis.OnTimeSeriesToggled(isActive);
			}

		}

		private void OnSwiped(PointerInteraction pointerInteraction) {

			if (!pointerInteraction.isSwipe) return;

			if (!HasLoadedDataPoints)
				return;

			foreach (DataPoint dp in DataPoints) {
				dp.Vis.Swiped(pointerInteraction);
			}

			dataPointHistorySwiped?.Invoke(DataPoints[0].CurrentMeasurementResultIndex);

		}

		public void OnHistorySliderMoved(int newCount, int prevCount) {

			if (!HasLoadedDataPoints)
				return;

			PointerInteraction historyPointerInteraction = new();
			historyPointerInteraction.startingGameObject = dummyVisObject;
			historyPointerInteraction.endingGameObject = dummyVisObject;
			historyPointerInteraction.isSwipe = true;

			if (newCount > prevCount) {
				historyPointerInteraction.startPosition = Vector2.zero;
				historyPointerInteraction.endPosition = Vector2.down * 100f;
			}
			else {
				historyPointerInteraction.startPosition = Vector2.zero;
				historyPointerInteraction.endPosition = Vector2.up * 100f;
			}

			foreach (DataPoint dp in DataPoints) {
				dp.Vis.Swiped(historyPointerInteraction);
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

		}

		public void UpdateDataPoints(DataAttribute attribute) {

			if (HasLoadedDataPoints) {
				ClearDataPoints();
			}

			SpawnDataPoints();
			HasLoadedDataPoints = true;

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
					dataPointInstance.SetMeasurementResult(dataPointInstance.MeasurementDefinition.GetLatestMeasurementResult());

					//Move the DataPoint to its location
					if (AppOptions.DemoMode) {

						Vector3 GetLastKnownDevicePosition(Device device) {

							if (LastKnownDevicePositions.TryGetValue(device, out Vector3 position)) {
								return position;
							}

							return new Vector3(-1000, -1000, -1000);

						}

						PlaceDataPoint(GetLastKnownDevicePosition(projectDevices[i]), dataPointInstance.transform);

					}
					else {
						DataPointsLocations[i] = Conversions.StringToLatLon(projectDevices[i].Position.GetLatLong());
						PlaceDataPoint(DataPointsLocations[i], dataPointInstance.transform);
					}

					DataPoints.Add(dataPointInstance);
				}

			}

			SetDataPointVisualization(DataAttributeManager.SelectedAttribute.VisOptions.First());

		}

		public void PlaceDataPoint(Vector2d newPosition, Transform dataPointTransform) {
			dataPointTransform.localPosition = map.GeoToWorldPosition(newPosition);
		}

		public void PlaceDataPoint(Vector3 newPosition, Transform dataPointTransform) {
			dataPointTransform.localPosition = newPosition;
		}

		private void OnMeasurementResultsUpdated() {

			if (!HasLoadedDataPoints) {
				return;
			}

			foreach (DataPoint dp in DataPoints) {
				dp.Vis.OnMeasurementResultsUpdated();
				dp.CurrentMeasurementResult = dp.MeasurementDefinition.GetLatestMeasurementResult();
			}

			dataPointsResultsUpdated?.Invoke();

		}

		private void ClearDataPoints() {

			HasLoadedDataPoints = false;

			foreach (DataPoint dp in DataPoints) {
				dp.RemoveVis();
				Destroy(dp.gameObject);
			}

			DataPoints.Clear();
		}

#endregion

	}

}