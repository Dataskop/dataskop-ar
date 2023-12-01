using System.Globalization;
using Mapbox.Unity.Map;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

namespace DataSkopAR.Entities {

	/// <summary>
	///     Responsible for aligning the AR Worlds y-Axis to the real worlds ground level.
	/// </summary>
	public class GroundLevelCalibrator : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private ARPlaneManager arPlaneManager;
		[SerializeField] private AbstractMap map;

		[Header("Events")]
		[SerializeField] private UnityEvent<bool> onCalibrationToggle;
		[SerializeField] private UnityEvent onFirstCalibrationChange;

#endregion

#region Properties

		private float GroundLevelYPosition { get; set; }
		private bool IsCalibrating { get; set; }
		private bool HasCalibrated { get; set; }

#endregion

#region Methods

		private void Start() {
			arPlaneManager.planesChanged += GetLowestPlane;
			map.OnUpdated += OnMapUpdated;
		}

		private void GetLowestPlane(ARPlanesChangedEventArgs e) {

			if (!IsCalibrating)
				return;

			foreach (ARPlane plane in e.added) {
				float yPos = plane.center.y;

				if (yPos < -2f) {
					plane.gameObject.SetActive(false);
					continue;
				}

				if (GroundLevelYPosition > yPos) {
					GroundLevelYPosition = yPos;
					SetRootGroundLevel(GroundLevelYPosition);

					if (!HasCalibrated) {
						onFirstCalibrationChange?.Invoke();
						HasCalibrated = true;
					}

				}
			}

		}

		private void SetRootGroundLevel(float newGroundLevel) {
			Vector3 mapPosition = map.Root.position;
			mapPosition = new Vector3(mapPosition.x, newGroundLevel, mapPosition.z);
			map.Root.position = mapPosition;
		}

		private void ResetRootGroundLevel() {

			GroundLevelYPosition = 0;

			Vector3 mapPosition = map.Root.position;
			mapPosition = new Vector3(mapPosition.x, 0, mapPosition.z);
			map.Root.position = mapPosition;
		}

		private void OnMapUpdated() {
			SetRootGroundLevel(GroundLevelYPosition);
		}

		public void ToggleGroundLevelCalibration() {
			IsCalibrating = !IsCalibrating;
			TogglePlanes(IsCalibrating);
			arPlaneManager.enabled = IsCalibrating;
			onCalibrationToggle?.Invoke(IsCalibrating);
		}

		public void SetGroundLevelCalibrationState(bool newState) {
			IsCalibrating = newState;
			TogglePlanes(IsCalibrating);
			arPlaneManager.enabled = IsCalibrating;
			onCalibrationToggle?.Invoke(IsCalibrating);
		}

		public void ResetCalibratedStatus() {
			HasCalibrated = false;
			ResetRootGroundLevel();
		}

		private void TogglePlanes(bool status) {
			foreach (ARPlane plane in arPlaneManager.trackables) plane.gameObject.SetActive(status);
		}

		private void OnDisable() {
			map.OnUpdated -= OnMapUpdated;
			arPlaneManager.planesChanged -= GetLowestPlane;
		}

#endregion

	}

}