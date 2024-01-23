using UnityEngine;
using UnityEngine.Events;

namespace DataskopAR.Interaction {

	public class RoomCalibrator : MonoBehaviour, ICalibration {

#region Constants

		private const float ProgressDistanceThreshold = 45f;

#endregion

#region Events

		[Header("Events")]
		public UnityEvent<float> roomScanProgressed;

#endregion

#region Properties

		private Camera ArCamera { get; set; }

		private Vector3 PreviousRotationEuler { get; set; }

		public float RoomScanProgress { get; set; }

		public bool IsEnabled { get; set; }

#endregion

#region Methods

		public ICalibration Enable() {
			ArCamera = Camera.main;
			IsEnabled = true;
			return this;
		}

		public void Disable() {
			IsEnabled = false;
		}

		private void FixedUpdate() {

			if (IsEnabled) {
				CheckRotationDelta();
			}

		}

		private void CheckRotationDelta() {

			if (!(Vector3.Distance(ArCamera.transform.eulerAngles, PreviousRotationEuler) > ProgressDistanceThreshold)) {
				return;
			}

			float randomProgressValue = UnityEngine.Random.Range(0.05f, 0.125f);
			RoomScanProgress += randomProgressValue;
			roomScanProgressed?.Invoke(RoomScanProgress);

			PreviousRotationEuler = ArCamera.transform.eulerAngles;

		}

#endregion

	}

}