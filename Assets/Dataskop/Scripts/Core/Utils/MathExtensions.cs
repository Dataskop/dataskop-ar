using System.Linq;
using UnityEngine;
using static System.Math;

namespace DataskopAR.Utils {

	public static class MathExtensions {

		// http://answers.unity.com/answers/1158105/view.html
		public static float GetSignedAngle(Quaternion a, Quaternion b, Vector3 axis) {

			(b * Quaternion.Inverse(a)).ToAngleAxis(out float angle, out Vector3 angleAxis);

			if (Vector3.Angle(axis, angleAxis) > 90f) angle = -angle;

			return Mathf.DeltaAngle(0f, angle);

		}

		/// <summary>
		///     Returns a projected Quaternion to a specific axis ignoring the other axes.
		/// </summary>
		/// <param name="eulerRotation">The objects current euler rotation.</param>
		/// <param name="axis">The axis the quaternion should be on.</param>
		/// <returns>A quaternion that is aligned only to the given axis.</returns>
		public static Quaternion QuaternionOnAxis(Vector3 eulerRotation, Vector3 axis) {
			return Quaternion.Euler(eulerRotation.x * axis.x, eulerRotation.y * axis.y, eulerRotation.z * axis.z);
		}

		/// <summary>
		///     Gets the signed angle between two transforms on an axis.
		/// </summary>
		/// <param name="a">Transform of the first object.</param>
		/// <param name="b">Transform of the second object.</param>
		/// <param name="axis">The axis that gets compared.</param>
		/// <returns>A right-handed signed angle calculated from object a.</returns>
		public static float GetSignedAngleOnAxis(Transform a, Transform b, Vector3 axis) {
			Quaternion q1 = QuaternionOnAxis(a.rotation.eulerAngles, axis);
			Quaternion q2 = QuaternionOnAxis(b.rotation.eulerAngles, axis);
			return GetSignedAngle(q1, q2, axis);
		}

		// https://rosettacode.org/wiki/Averages/Mean_angle#C.23
		/// <summary>
		///     Gets the mean of a set of angles.
		/// </summary>
		/// <param name="angles">Value set of angles.</param>
		/// <returns>Mean angle</returns>
		public static double MeanAngle(double[] angles) {
			double x = angles.Sum(a => Cos(a * PI / 180)) / angles.Length;
			double y = angles.Sum(a => Sin(a * PI / 180)) / angles.Length;
			return Atan2(y, x) * 180 / PI;
		}

		/// <summary>
		///     Maps a value from some arbitrary range to the 0 to 1 range
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static float Map01(float value, float min, float max) {
			return (value - min) * 1f / (max - min);
		}

		/// <summary>
		///     Maps a value from ome arbitrary range (x1, x2) to another arbitrary range (y1, y2)
		/// </summary>
		/// <param name="value">The value in the from range that will be mapped to range.</param>
		/// <param name="x1">Min value from range </param>
		/// <param name="x2">Max value from range</param>
		/// <param name="y1">Min value to range</param>
		/// <param name="y2">Max value to range</param>
		/// <returns></returns>
		public static float Map(float value, float x1, float x2, float y1, float y2) {
			return y1 + (value - x1) * (y2 - y1) / (x2 - x1);
		}

		/// <summary>
		///     Finds the closest point on a ray to a given point. If the point is
		///     behind the ray's origin, the closest point will be the origin.
		/// </summary>
		/// <param name="ray">The ray on which the closes point will be found.</param>
		/// <param name="point">The given point.</param>
		/// <param name="distance">Distance from the found point on the ray to the given point.</param>
		/// <returns></returns>
		public static Vector3 ClosestPointAlongRay(Ray ray, Vector3 point, out float distance) {

			distance = Vector3.Dot(point - ray.origin, ray.direction);
			distance = Mathf.Max(distance, 0f);
			return ray.origin + ray.direction * distance;

		}

	}

}