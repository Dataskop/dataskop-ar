using System;
using Mapbox.Unity.Map;
using UnityEngine;

namespace DataskopAR.Entities {

	public class BirdViewCamera : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private AbstractMap map;
		[SerializeField] private Camera birdViewCamera;
		[SerializeField] private Transform userCameraTransform;

		[Header("Values")]
		[SerializeField] private float defaultCameraSize = 20;

#endregion

#region Methods

		private void Awake() {
			SetCameraSize(defaultCameraSize);
		}

		private void FixedUpdate() {
			birdViewCamera.transform.SetPositionAndRotation(
				GetTrackedPosition(userCameraTransform.position),
				GetAlignedRotation(map.transform.rotation)
			);
		}

		private Vector3 GetTrackedPosition(Vector3 trackingPosition) {
			return new Vector3(trackingPosition.x, 20, trackingPosition.z);
		}

		private Quaternion GetAlignedRotation(Quaternion aligningRotation) {
			return Quaternion.Euler(new Vector3(90, aligningRotation.eulerAngles.y, 0));
		}

		public void SetCameraSize(float newSize) {
			birdViewCamera.orthographicSize = newSize;
		}

		public void ResetCameraSize() {
			birdViewCamera.orthographicSize = defaultCameraSize;
		}

#endregion

	}

}