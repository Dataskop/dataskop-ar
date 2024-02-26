using DataskopAR.Utils;
using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	public class BubbleTimeSeries : TimeSeries {

#region Fields

		[SerializeField] private float minScale;
		[SerializeField] private float maxScale;

#endregion

#region Constants

		private static readonly int Alpha = Shader.PropertyToID("_Alpha");

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
				float bubbleSize = MathExtensions.Map(elementValue, DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum, minScale,
					maxScale);
				e.transform.localScale = new Vector3(bubbleSize, bubbleSize, bubbleSize);
			}

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