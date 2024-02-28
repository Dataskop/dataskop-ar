namespace DataskopAR.Utils {
    using UnityEngine;
    public static class VectorExtensions {

        public static Vector3 WithY(this Vector3 v, float y) =>
            new Vector3(v.x, y, v.z);

    }
}