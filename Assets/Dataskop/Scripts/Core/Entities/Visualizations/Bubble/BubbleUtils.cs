using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public static class BubbleUtils {

		private const float PI = Mathf.PI;

		/// <summary>
		/// Calculates the area based on a given value and maps it to a new radius based on it.
		/// </summary>
		/// <param name="value">The given value for the mapping</param>
		/// <param name="minValue">The lower limit of the value</param>
		/// <param name="maxValue">The upper limit of the value</param>
		/// <param name="minRadius">The lower radius limit of the </param>
		/// <param name="maxRadius">The upper limit of the calculated radius</param>
		/// <returns>Radius</returns>
		public static float CalculateRadius(float value, float minValue, float maxValue, float minRadius,
			float maxRadius) {
			value = Mathf.Clamp(value, minValue, maxValue);

			float minArea = PI * minRadius * minRadius;
			float maxArea = PI * maxRadius * maxRadius;

			// calc mapped area from the value
			float t = (value - minValue) / (maxValue - minValue);
			float newArea = minArea + t * (maxArea - minArea);

			// calc radius out of the area
			return Mathf.Sqrt(newArea / PI);
		}

	}

}