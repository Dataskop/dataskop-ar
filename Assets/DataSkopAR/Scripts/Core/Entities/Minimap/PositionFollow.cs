using UnityEngine;

namespace DataskopAR.Entities {

	public class PositionFollow : MonoBehaviour {

		[SerializeField] private Transform targetTransform;

		[SerializeField] private bool freezeX;
		[SerializeField] private bool freezeY;
		[SerializeField] private bool freezeZ;

		private void FixedUpdate() {

			Vector3 targetPos = targetTransform.position;
			var xPos = freezeX ? transform.position.x : targetPos.x;
			var yPos = freezeY ? transform.position.y : targetPos.y;
			var zPos = freezeZ ? transform.position.z : targetPos.z;

			transform.position = new Vector3(xPos, yPos, zPos);

		}

	}

}