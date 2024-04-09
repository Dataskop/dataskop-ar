using UnityEngine;
namespace DataskopAR.Entities {

	public class RotationLock : MonoBehaviour {

#region Methods

		private void FixedUpdate() {

			float xEuler = freezeX ? transform.rotation.eulerAngles.x : targetTransform.rotation.eulerAngles.x;
			float yEuler = freezeY ? transform.rotation.eulerAngles.y : targetTransform.rotation.eulerAngles.y;
			float zEuler = freezeZ ? transform.rotation.eulerAngles.z : targetTransform.rotation.eulerAngles.z;

			transform.rotation = Quaternion.Euler(xEuler, yEuler, zEuler);

		}

#endregion

#region Fields

		[SerializeField] private Transform targetTransform;

		[SerializeField] private bool freezeX;
		[SerializeField] private bool freezeY;
		[SerializeField] private bool freezeZ;

#endregion

	}

}