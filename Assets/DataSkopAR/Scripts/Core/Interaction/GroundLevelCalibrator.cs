using System;
using Mapbox.Unity.Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

namespace DataskopAR.Interaction {

	/// <summary>
	///     Responsible for aligning the AR Worlds y-Axis to the real worlds ground level.
	/// </summary>
	public class GroundLevelCalibrator : MonoBehaviour, ICalibration {

#region Constants

		private const int TargetLayerMask = 1 << 10;
		private const string PlaneTag = "ARPlane";

#endregion

#region Events

		public event Action CalibrationCompleted;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private ARPlaneManager arPlaneManager;
		[SerializeField] private AbstractMap map;

		private Camera cam;

#endregion

#region Properties

		public float GroundLevelYPosition { get; set; }

		public bool IsEnabled { get; set; }

		private ARPlane LowestPlane { get; set; }

		private Ray TapScreenToWorldRay { get; set; }

		private Vector2 TapPosition { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			arPlaneManager.planesChanged += OnArPlanesChanged;
			map.OnUpdated += OnMapUpdated;
		}

		public ICalibration Enable() {

			IsEnabled = true;
			cam = Camera.main;
			arPlaneManager.enabled = IsEnabled;
			TogglePlanes(IsEnabled);
			return this;

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
				LowestPlane = foundPlane;
			}

		}

		private void SetMapRootGroundLevel(float newGroundLevel) {

			Vector3 mapPosition = map.Root.position;
			mapPosition = new Vector3(mapPosition.x, newGroundLevel, mapPosition.z);
			map.Root.position = mapPosition;

		}

		public void ResetGroundLevelCalibration() {

			LowestPlane = null;
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

		public void TapPositionInput(InputAction.CallbackContext ctx) {

			if (IsEnabled) {
				TapPosition = ctx.ReadValue<Vector2>();
			}

		}

		public void TapInput(InputAction.CallbackContext ctx) {

			if (ctx.canceled) {

				if (!IsEnabled) {
					return;
				}

				GameObject tappedPlane = GetTappedPArPlane(TapPosition);

				if (tappedPlane == null) {
					return;
				}

				LowestPlane = tappedPlane.GetComponent<ARPlane>();
				GroundLevelYPosition = tappedPlane.transform.position.y;
				SetMapRootGroundLevel(GroundLevelYPosition);
				CalibrationCompleted?.Invoke();

			}

		}

		private GameObject GetTappedPArPlane(Vector3 tapPos) {

			TapScreenToWorldRay = cam.ScreenPointToRay(new Vector3(tapPos.x, tapPos.y, -5));

			if (!Physics.Raycast(TapScreenToWorldRay, out RaycastHit hit, Mathf.Infinity, TargetLayerMask)) {
				return null;
			}

			GameObject tappedObject = hit.collider.gameObject;

			if (!tappedObject.CompareTag(PlaneTag)) {
				return null;
			}

			return hit.collider.gameObject;

		}

		public void Disable() {

			IsEnabled = false;
			TogglePlanes(IsEnabled);
			arPlaneManager.enabled = IsEnabled;

		}

		private void OnDisable() {
			map.OnUpdated -= OnMapUpdated;
			arPlaneManager.planesChanged -= OnArPlanesChanged;
		}

#endregion

	}

}