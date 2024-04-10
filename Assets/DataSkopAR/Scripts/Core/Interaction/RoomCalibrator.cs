using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace DataskopAR.Interaction {

	public class RoomCalibrator : MonoBehaviour, ICalibration {

#region Constants

		private const float ProgressDistanceThreshold = 45f;

#endregion

#region Events

		[Header("Events")]
		public UnityEvent<float> roomScanProgressed;

		public event Action CalibrationCompleted;

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
			StartCoroutine(CheckRotationDelta());
			IsEnabled = true;
			return this;
		}

		private IEnumerator CheckRotationDelta() {

			while (RoomScanProgress < 1) {

				if (!(Vector3.Distance(ArCamera.transform.eulerAngles, PreviousRotationEuler) > ProgressDistanceThreshold)) {
					yield return new WaitForEndOfFrame();
				}
				else {
					float randomProgressValue = Random.Range(0.05f, 0.125f);
					RoomScanProgress += randomProgressValue;
					roomScanProgressed?.Invoke(RoomScanProgress);
					PreviousRotationEuler = ArCamera.transform.eulerAngles;
				}

			}

			yield return new WaitForEndOfFrame();
			CalibrationCompleted?.Invoke();

		}

		public void ResetRoomCalibration() {
			RoomScanProgress = 0;
			roomScanProgressed?.Invoke(RoomScanProgress);
		}

		public void Disable() {
			IsEnabled = false;
		}

#endregion

	}

}