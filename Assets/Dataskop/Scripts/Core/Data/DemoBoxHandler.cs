using System.Collections.Generic;
using System.Linq;
using Dataskop.Entities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Dataskop.Data {

	public class DemoBoxHandler : MonoBehaviour {

		[SerializeField] private ARTrackedImageManager imageManager;
		[SerializeField] private DataPointsManager dataPointManager;
		[SerializeField] private DataManager dataManager;
		[SerializeField] private DataAttributeManager dataAttributeManager;

		private Dictionary<ARTrackedImage, Device> ARImageObjects { get; set; }

		private bool ShouldTrackImages { get; set; }

		private void OnEnable() {
			dataManager.HasLoadedProjectData += ActivateTracking;
		}

		private void OnDisable() {
			ShouldTrackImages = false;
			imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
		}

		private void ActivateTracking(Project loadedProject) {
			ShouldTrackImages = true;
			imageManager.enabled = true;
			imageManager.trackedImagesChanged += OnTrackedImagesChanged;
			ARImageObjects = new Dictionary<ARTrackedImage, Device>();
		}

		private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs e) {

			if (!ShouldTrackImages) {
				return;
			}

			if (dataPointManager.DataPoints.Count == 0) {
				return;
			}

			foreach (ARTrackedImage i in e.added) {
				OnARImageAdded(i);
			}

			foreach (ARTrackedImage i in e.updated) {
				OnARImageUpdated(i);
			}

		}

		private void OnARImageAdded(ARTrackedImage trackedImage) {

			string encodedDeviceName = trackedImage.referenceImage.name;

			Device scannedDevice = dataManager.SelectedProject.Devices.FirstOrDefault(device => device.ID == encodedDeviceName);

			if (scannedDevice == null) {
				return;
			}

			ARImageObjects.Add(trackedImage, scannedDevice);

			string scannedAttribute = scannedDevice!.MeasurementDefinitions?.FirstOrDefault()!.AttributeId;

			if (string.IsNullOrEmpty(scannedAttribute)) {
				return;
			}

			dataAttributeManager.SetSelectedAttribute(scannedAttribute);

			DataPoint dataPoint = dataPointManager.DataPoints.FirstOrDefault(dp => dp.Device == scannedDevice);

			if (dataPoint == null) {
				return;
			}

			float visOffset = dataPoint.Vis.Offset.y;
			Vector3 position = trackedImage.transform.position;
			Vector3 imagePosition = new(position.x, position.y - visOffset, position.z);
			dataPointManager.PlaceDataPoint(imagePosition, dataPoint.transform);

		}

		private void OnARImageUpdated(ARTrackedImage trackedImage) {

			// Add the AR Image if it is already added to the System before Projects are loaded
			// but not in the local Dictionary.
			if (!ARImageObjects.ContainsKey(trackedImage)) {
				OnARImageAdded(trackedImage);
			}

			if (trackedImage.trackingState == TrackingState.Tracking) {

				if (ARImageObjects.TryGetValue(trackedImage, out Device device)) {

					string scannedAttribute = device!.MeasurementDefinitions?.FirstOrDefault()!.AttributeId;

					if (string.IsNullOrEmpty(scannedAttribute)) {
						return;
					}

					dataAttributeManager.SetSelectedAttribute(scannedAttribute);

					DataPoint dataPoint = dataPointManager.DataPoints.FirstOrDefault(dp => dp.Device == device);

					if (dataPoint == null) {
						return;
					}

					if (dataPoint.gameObject.TryGetComponent(out ARAnchor anchor)) {
						Destroy(anchor);
					}

					Vector3 position = trackedImage.transform.position;
					dataPointManager.PlaceDataPoint(position - dataPoint.Vis.Offset, dataPoint.transform);

				}

			}

			if (trackedImage.trackingState == TrackingState.Limited) {

				if (!ARImageObjects.TryGetValue(trackedImage, out Device device)) {
					return;
				}

				DataPoint dataPoint = dataPointManager.DataPoints.FirstOrDefault(dp => dp.Device == device);

				if (dataPoint == null) {
					return;
				}

				if (dataPoint.gameObject.TryGetComponent(out ARAnchor _)) {
					return;
				}

				dataPoint.gameObject.AddComponent<ARAnchor>();
				dataPointManager.LastKnownDevicePositions[device] = dataPoint.transform.position;

			}

		}

	}

}