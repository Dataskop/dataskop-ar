using System;
using Mapbox.Unity.Map;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Dataskop.Interaction {

	/// <summary>
	/// Responsible for aligning the AR Worlds y-Axis to the real worlds ground level.
	/// </summary>
	public class GroundLevelCalibrator : MonoBehaviour, ICalibration {

		private const string PlaneTag = "ARPlane";

		[Header("References")]
		[SerializeField] private ARPlaneManager arPlaneManager;
		[SerializeField] private AbstractMap map;
		[SerializeField] private InputHandler inputHandler;

		public float GroundLevelYPosition { get; set; }

		private void OnEnable() {
			arPlaneManager.planesChanged += OnArPlanesChanged;
			map.OnUpdated += OnMapUpdated;
			inputHandler.WorldPointerUpped += OnPointerInteractionReceived;
		}

		private void OnDisable() {
			map.OnUpdated -= OnMapUpdated;
			arPlaneManager.planesChanged -= OnArPlanesChanged;
			inputHandler.WorldPointerUpped -= OnPointerInteractionReceived;
		}

		public event Action CalibrationCompleted;

		public bool IsEnabled { get; set; }

		public ICalibration Enable() {

			IsEnabled = true;
			arPlaneManager.enabled = IsEnabled;
			TogglePlanes(IsEnabled);
			return this;

		}

		public void Disable() {

			IsEnabled = false;
			TogglePlanes(IsEnabled);
			arPlaneManager.enabled = IsEnabled;

		}

		private void OnArPlanesChanged(ARPlanesChangedEventArgs e) {

			if (!IsEnabled) {
				return;
			}

			foreach (ARPlane plane in e.added) {
				SetLowestPlaneFound(plane);
			}

		}

		public void SetLowestPlaneFound(ARPlane foundPlane) {

			float yPos = foundPlane.center.y;

			if (yPos < -3f) {
				foundPlane.gameObject.SetActive(false);
				return;
			}

			if (GroundLevelYPosition > yPos) {
				GroundLevelYPosition = yPos;
			}

		}

		private void SetMapRootGroundLevel(float newGroundLevel) {

			Vector3 mapPosition = map.Root.position;
			mapPosition = new Vector3(mapPosition.x, newGroundLevel, mapPosition.z);
			map.Root.position = mapPosition;

		}

		public void ResetGroundLevelCalibration() {

			GroundLevelYPosition = 0;
			Vector3 mapPosition = map.Root.position;
			mapPosition = new Vector3(mapPosition.x, GroundLevelYPosition, mapPosition.z);
			map.Root.position = mapPosition;

		}

		private void OnMapUpdated() {
			SetMapRootGroundLevel(GroundLevelYPosition);
		}

		private void TogglePlanes(bool status) {

			foreach (ARPlane plane in arPlaneManager.trackables) {
				plane.gameObject.SetActive(status);
			}

		}

		private void OnPointerInteractionReceived(PointerInteraction i) {

			if (!IsEnabled) {
				return;
			}

			GameObject tappedPlane = GetTappedPArPlane(i.startingGameObject);

			if (tappedPlane == null) {
				return;
			}

			GroundLevelYPosition = tappedPlane.transform.position.y;
			SetMapRootGroundLevel(GroundLevelYPosition);
			CalibrationCompleted?.Invoke();

		}

		private GameObject GetTappedPArPlane(GameObject pointedObject) {

			if (pointedObject == null) return null;

			return pointedObject.CompareTag(PlaneTag) ? pointedObject : null;

		}

	}

}