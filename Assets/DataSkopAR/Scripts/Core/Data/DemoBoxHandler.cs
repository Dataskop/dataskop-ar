using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
namespace DataskopAR.Data {

	public class DemoBoxHandler : MonoBehaviour {

#region Fields

		[SerializeField] private ARTrackedImageManager imageManager;
		[SerializeField] private DataPointsManager dataPointManager;
		[SerializeField] private DataManager dataManager;
		[SerializeField] private DataAttributeManager dataAttributeManager;

#endregion

#region Properties

		private Dictionary<ARTrackedImage, Device> ARImageObjects { get; set; }

		private bool ShouldTrackImages { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			dataManager.HasLoadedProjectData += ActivateTracking;
			imageManager.trackedImagesChanged += OnTrackedImagesChanged;
			ARImageObjects = new Dictionary<ARTrackedImage, Device>();
		}

		private void ActivateTracking(Project loadedProject) {
			ShouldTrackImages = true;
		}

		private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs e) {

			if (!ShouldTrackImages) {
				return;
			}

			if (dataPointManager.DataPoints.Count == 0) {
				return;
			}

			foreach (ARTrackedImage i in e.added) {

				string encodedDeviceName = i.referenceImage.name;

				Device scannedDevice = dataManager.SelectedProject.Devices.FirstOrDefault(device => device.ID == encodedDeviceName);

				if (scannedDevice == null) {
					continue;
				}

				ARImageObjects.Add(i, scannedDevice);

				string scannedAttribute = scannedDevice!.MeasurementDefinitions?.FirstOrDefault()!.AttributeId;

				if (string.IsNullOrEmpty(scannedAttribute)) {
					continue;
				}

				dataAttributeManager.SetSelectedAttribute(scannedAttribute);

				DataPoint dataPoint = dataPointManager.DataPoints.FirstOrDefault(dp => dp.Device == scannedDevice);

				if (dataPoint == null) {
					continue;
				}

				float visOffset = dataPoint.Vis.Offset.y;
				Vector3 position = i.transform.position;
				Vector3 imagePosition = new(position.x, position.y - visOffset, position.z);
				dataPointManager.PlaceDataPoint(imagePosition, dataPoint.transform);

			}

			foreach (ARTrackedImage i in e.updated) {

				if (i.trackingState == TrackingState.Tracking) {

					if (ARImageObjects.TryGetValue(i, out Device device)) {

						string scannedAttribute = device!.MeasurementDefinitions?.FirstOrDefault()!.AttributeId;

						if (string.IsNullOrEmpty(scannedAttribute)) {
							continue;
						}

						dataAttributeManager.SetSelectedAttribute(scannedAttribute);

						DataPoint dataPoint = dataPointManager.DataPoints.FirstOrDefault(dp => dp.Device == device);

						if (dataPoint == null) {
							continue;
						}

						if (dataPoint.gameObject.TryGetComponent(out ARAnchor anchor)) {
							Destroy(anchor);
						}

						Vector3 position = i.transform.position;
						Vector3 imagePosition = new(position.x, position.y - dataPoint.Vis.Offset.y, position.z);
						dataPointManager.PlaceDataPoint(imagePosition, dataPoint.transform);

					}

				}

				if (i.trackingState == TrackingState.Limited) {

					if (!ARImageObjects.TryGetValue(i, out Device device)) {
						continue;
					}

					DataPoint dataPoint = dataPointManager.DataPoints.FirstOrDefault(dp => dp.Device == device);

					if (dataPoint == null) {
						continue;
					}

					if (dataPoint.gameObject.TryGetComponent(out ARAnchor _)) {
						continue;
					}

					dataPoint.gameObject.AddComponent<ARAnchor>();
					dataPointManager.LastKnownDevicePositions[device] = dataPoint.transform.position;

				}

			}

		}

		private void OnDisable() {
			ShouldTrackImages = false;
			imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
		}

#endregion

	}

}