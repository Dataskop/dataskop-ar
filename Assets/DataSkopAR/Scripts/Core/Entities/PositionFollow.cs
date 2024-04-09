using UnityEngine;
namespace DataskopAR.Entities {

	public class PositionFollow : MonoBehaviour {

#region Methods

		private void FixedUpdate() {

			Vector3 targetPos = targetTransform.position;
			float xPos = freezeX ? transform.position.x : targetPos.x;
			float yPos = freezeY ? transform.position.y : targetPos.y;
			float zPos = freezeZ ? transform.position.z : targetPos.z;

			transform.position = new Vector3(xPos, yPos, zPos);

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