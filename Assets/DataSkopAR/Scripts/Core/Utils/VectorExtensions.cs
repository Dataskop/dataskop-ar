using UnityEngine;

namespace DataskopAR.Utils {

	public static class VectorExtensions {

		public static Vector3 WithY(this Vector3 v, float y) {
			return new Vector3(v.x, y, v.z);
		}

	}

}