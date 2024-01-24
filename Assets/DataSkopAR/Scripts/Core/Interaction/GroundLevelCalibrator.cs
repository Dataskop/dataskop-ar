using System;
using Mapbox.Unity.Map;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

namespace DataskopAR.Interaction {

	/// <summary>
	///     Responsible for aligning the AR Worlds y-Axis to the real worlds ground level.
	/// </summary>
	public class GroundLevelCalibrator : MonoBehaviour, ICalibration {

#region Events

		public event Action CalibrationCompleted;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private ARPlaneManager arPlaneManager;
		[SerializeField] private AbstractMap map;

#endregion

#region Properties

		private float GroundLevelYPosition { get; set; }
		public bool IsEnabled { get; set; }
		private bool HasCalibrated { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			arPlaneManager.planesChanged += GetLowestPlane;
			map.OnUpdated += OnMapUpdated;
		}

		private void GetLowestPlane(ARPlanesChangedEventArgs e) {

			if (!IsEnabled)
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
						CalibrationCompleted?.Invoke();
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

		public void ResetCalibratedStatus() {
			HasCalibrated = false;
			ResetRootGroundLevel();
		}

		private void TogglePlanes(bool status) {

			foreach (ARPlane plane in arPlaneManager.trackables) {
				plane.gameObject.SetActive(status);
			}

		}

		public ICalibration Enable() {

			IsEnabled = true;
			TogglePlanes(IsEnabled);
			arPlaneManager.enabled = IsEnabled;
			return this;

		}

		public void Disable() {

			IsEnabled = false;
			TogglePlanes(IsEnabled);
			arPlaneManager.enabled = IsEnabled;

		}

		private void OnDisable() {
			map.OnUpdated -= OnMapUpdated;
			arPlaneManager.planesChanged -= GetLowestPlane;
		}

#endregion

	}

}