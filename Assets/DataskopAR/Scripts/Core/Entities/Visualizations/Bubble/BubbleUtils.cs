using DataskopAR.Utils;
using UnityEngine;
namespace DataskopAR.Entities.Visualizations {

	public static class BubbleUtils {

		/// <summary>
		/// Calculates the area based on a given value and maps it to a new radius based on it.
		/// </summary>
		/// <param name="value">The given value for the mapping</param>
		/// <param name="minValue">The lower limit of the value</param>
		/// <param name="maxValue">The upper limit of the value</param>
		/// <param name="minRadius">The lower radius limit of the </param>
		/// <param name="maxRadius">The upper limit of the calculated radius</param>
		/// <returns>Radius</returns>
		public static float CalculateRadius(float value, float minValue, float maxValue, float minRadius, float maxRadius) {
			value = Mathf.Clamp(value, minValue, maxValue);

			// calc min and max area out of desired min and max scale
			float minArea = Mathf.PI * Mathf.Pow(minRadius, 2);
			float maxArea = Mathf.PI * Mathf.Pow(maxRadius, 2);

			// calc mapped area from the value
			float newArea = MathExtensions.Map(value, minValue, maxValue, minArea, maxArea);

			// calc radius out of the area
			return Mathf.Sqrt(newArea / Mathf.PI);
		}

	}

}