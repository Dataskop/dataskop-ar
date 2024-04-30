#nullable enable

using DataskopAR.Utils;
using UnityEngine;

namespace DataskopAR.Entities {

	public class FaceCamera : MonoBehaviour {

#region Properties

		private Transform? TargetTransform => targetCamera?.transform;

#endregion

#region Fields

		[SerializeField] [Tooltip("How close the camera has to be to the object for it to face it.")]
		private float faceThreshold;

		[SerializeField] private Camera? targetCamera;
		[SerializeField] private bool isBackwards;

#endregion

#region Methods

		private void Awake() {
			if (targetCamera == null) {
				targetCamera = Camera.main;
			}
		}

		private void FixedUpdate() {

			Transform? targetTransform = TargetTransform;

			if (targetTransform == null) {
				return;
			}

			AlignWith(targetTransform.position);
		}

		private void AlignWith(Vector3 target) {

			Vector3 diff = (target - transform.position).WithY(0);
			float distance = diff.magnitude;
			bool shouldFace = distance <= faceThreshold;

			if (!shouldFace) {
				return;
			}

			transform.forward = isBackwards ? -diff.normalized : diff.normalized;
		}

#endregion

	}

}