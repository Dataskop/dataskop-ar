using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace DataSkopAR {

	public class RoomCalibrator : MonoBehaviour {

#region Constants

		private const float ProgressDistanceThreshold = 45f;

#endregion

#region Events

		[Header("Events")]
		public UnityEvent<float> HasMadeCalibrationProgress;

#endregion

#region Properties

		private Camera ArCamera { get; set; }

		private Vector3 PreviousRotationEuler { get; set; }

		private bool IsWaitingForRotation { get; set; }

#endregion

#region Methods

		private void Start() {
			ArCamera = Camera.main;
		}

		private void FixedUpdate() {

			if (IsWaitingForRotation) {
				CheckRotationDelta();
			}

		}

		private void CheckRotationDelta() {

			if (!(Vector3.Distance(ArCamera.transform.eulerAngles, PreviousRotationEuler) > ProgressDistanceThreshold)) {
				return;
			}

			float randomProgressValue = Random.Range(0.05f, 0.15f);
			HasMadeCalibrationProgress?.Invoke(randomProgressValue);
			PreviousRotationEuler = ArCamera.transform.eulerAngles;

		}

		public void OnRoomCalibrationPhaseBegan() {
			IsWaitingForRotation = true;
		}

		public void OnCalibrationPhaseEnded() {
			IsWaitingForRotation = false;
		}

#endregion

	}

}