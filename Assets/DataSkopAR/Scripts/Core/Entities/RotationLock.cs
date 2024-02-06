using UnityEngine;

namespace DataskopAR.Entities {

	public class RotationLock : MonoBehaviour {

#region Fields

		[SerializeField] private Transform targetTransform;

		[SerializeField] private bool freezeX;
		[SerializeField] private bool freezeY;
		[SerializeField] private bool freezeZ;

#endregion

#region Methods

		private void FixedUpdate() {

			var xEuler = freezeX ? transform.rotation.eulerAngles.x : targetTransform.rotation.eulerAngles.x;
			var yEuler = freezeY ? transform.rotation.eulerAngles.y : targetTransform.rotation.eulerAngles.y;
			var zEuler = freezeZ ? transform.rotation.eulerAngles.z : targetTransform.rotation.eulerAngles.z;

			transform.rotation = Quaternion.Euler(xEuler, yEuler, zEuler);

		}

#endregion

	}

}