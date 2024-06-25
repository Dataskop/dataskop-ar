using Dataskop.Utils;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class BubbleTimeSeries : TimeSeries {

		[SerializeField] private float minScale;
		[SerializeField] private float maxScale;

		private void Awake() {
			TimeElementSpawned += SetSize;
			TimeElementMoved += SetSize;
		}

		private void SetSize(TimeElement e) {

			float elementValue = Mathf.Clamp(e.MeasurementResult.ReadAsFloat(), DataPoint.Attribute.Minimum,
				DataPoint.Attribute.Maximum);

			// calc min and max area out of desired min and max scale
			float minArea = Mathf.PI * Mathf.Pow(minScale, 2);
			float maxArea = Mathf.PI * Mathf.Pow(maxScale, 2);

			float newArea = MathExtensions.Map(elementValue, DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum, minArea,
				maxArea);

			float bubbleSize = Mathf.Sqrt(newArea / Mathf.PI);

			e.transform.localScale = new Vector3(bubbleSize, bubbleSize, bubbleSize);

		}

	}

}