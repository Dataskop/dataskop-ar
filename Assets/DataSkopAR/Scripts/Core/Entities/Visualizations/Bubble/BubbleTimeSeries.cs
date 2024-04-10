using DataskopAR.Utils;
using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	public class BubbleTimeSeries : TimeSeries {

#region Constants

		private static readonly int Alpha = Shader.PropertyToID("_Alpha");

#endregion

#region Fields

		[SerializeField] private float minScale;
		[SerializeField] private float maxScale;

#endregion

#region Methods

		private void Awake() {
			TimeSeriesSpawned += SetSize;
			TimeSeriesStartMoved += SetSize;
		}

		private void SetSize() {

			foreach (TimeElement e in TimeElements) {

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

			//TODO: Make it work with the new shader
			if (!Configuration.isFading) return;

			foreach (TimeElement e in TimeElements) {

				if (ShouldDrawTimeElement(Configuration.visibleHistoryCount, e)) {
					e.GetComponentInChildren<SpriteRenderer>().material
						.SetFloat(Alpha, 1f - MathExtensions.Map01(Mathf.Abs(e.DistanceToDataPoint), 0, Configuration.visibleHistoryCount));
				}

			}

		}

		private void OnDisable() {
			TimeSeriesSpawned -= SetSize;
			TimeSeriesStartMoved -= SetSize;
		}

#endregion

	}

}