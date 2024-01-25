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

		public float GroundLevelYPosition { get; set; }
		public bool IsEnabled { get; set; }
		private bool HasCalibrated { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			arPlaneManager.planesChanged += OnArPlanesChanged;
			map.OnUpdated += OnMapUpdated;
		}

		public ICalibration Enable() {

			IsEnabled = true;
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
			}

		}

		private void SetMapRootGroundLevel(float newGroundLevel) {

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
			SetMapRootGroundLevel(GroundLevelYPosition);
		}

		private void TogglePlanes(bool status) {

			foreach (ARPlane plane in arPlaneManager.trackables) {
				plane.gameObject.SetActive(status);
			}

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