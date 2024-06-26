using Mapbox.Unity.Map;
using UnityEngine;

namespace Dataskop.Entities {

	public class BirdViewCamera : MonoBehaviour {

		[Header("References")]
		[SerializeField] private AbstractMap map;
		[SerializeField] private Camera birdViewCamera;
		[SerializeField] private Transform userCameraTransform;

		[Header("Values")]
		[SerializeField] private float defaultCameraSize = 20;

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

		public float GetCurrentCameraSize() {
			return birdViewCamera.orthographicSize;
		}

		public void ChangeCameraSizeBy(float value) {
			birdViewCamera.orthographicSize = Mathf.Clamp(GetCurrentCameraSize() + value, 3, 150);
			map.UpdateMap();
		}

		public void ResetCameraSize() {
			birdViewCamera.orthographicSize = defaultCameraSize;
		}

	}

}