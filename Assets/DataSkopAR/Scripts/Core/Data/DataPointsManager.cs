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
		[SerializeField] private AbstractMap map;
		[SerializeField] private GameObject dataPointPrefab;
		[SerializeField] private Transform dataPointsContainer;
		[SerializeField] private VisualizationRepository visRepository;
		[SerializeField] private DataAttributeManager dataAttrRepo;
		[SerializeField] private AuthorRepository authorRepository;
		[SerializeField] private bool isDemoScene;

		private GameObject dummyVisObject;

		public static bool IsDemoScene;

#endregion

#region Properties

		/// <summary>
		///     List of currently placed markers in the AR world.
		/// </summary>
		public IList<DataPoint> DataPoints { get; set; }

		public Dictionary<Device, Vector3> LastKnownDevicePositions { get; set; }

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
			IsDemoScene = isDemoScene;
		}

		private void OnEnable() {
			DataManager.HasUpdatedMeasurementResults += OnMeasurementResultsUpdated;
		}

		private void Start() {

			SwipeDetector.OnSwipe += OnSwiped;

			dummyVisObject = new GameObject {
				tag = "Vis"
			};

			LastKnownDevicePositions = new Dictionary<Device, Vector3>();

		}

		private void FixedUpdate() {

			if (!HasLoadedDataPoints)
				return;

			if (IsDemoScene) return;

			for (int i = 0; i < DataPoints.Count; i++) {
				DataPoints[i].transform.localPosition = map.GeoToWorldPosition(DataPointsLocations[i]);
			}

		}

		public void SetDataPointVisualization(VisualizationOption visOpt) {

			if (VisualizationRepository.IsAvailable(visOpt.Type)) {
				GameObject vis = VisualizationRepository.GetVisualizationByName(visOpt.Type);

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

		private void OnSwiped(Swipe swipe) {

			if (!HasLoadedDataPoints)
				return;

			foreach (DataPoint dp in DataPoints) {
				dp.Vis.Swiped(swipe);
			}

			dataPointHistorySwiped?.Invoke(DataPoints[0].CurrentMeasurementResultIndex);

		}

		// idk
		public void OnHistorySliderMoved(int newCount, int prevCount) {

			if (!HasLoadedDataPoints)
				return;

			Swipe historySwipe = new Swipe();

			if (newCount > prevCount) {
				historySwipe.Direction = Vector2.down;
				historySwipe.YDistance = 100;
				historySwipe.StartingGameObject = dummyVisObject;
				historySwipe.HasStartedOverSlider = UIInteractionDetection.HasPointerStartedOverSlider;
			}
			else {
				historySwipe.Direction = Vector2.up;
				historySwipe.YDistance = 100;
				historySwipe.StartingGameObject = dummyVisObject;
				historySwipe.HasStartedOverSlider = UIInteractionDetection.HasPointerStartedOverSlider;
			}

			foreach (DataPoint dp in DataPoints) {
				dp.Vis.Swiped(historySwipe);
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
					if (IsDemoScene) {

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

		public void OnDisable() {
			SwipeDetector.OnSwipe -= OnSwiped;
			DataManager.HasUpdatedMeasurementResults -= OnMeasurementResultsUpdated;
		}

#endregion

	}

}