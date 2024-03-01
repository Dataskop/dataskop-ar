using DataskopAR.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace DataskopAR.Entities.Visualizations {

	public class DotTimeSeries : TimeSeries {

#region Constants

		private static readonly int Alpha = Shader.PropertyToID("_Alpha");

#endregion

#region Methods

		private void Awake() {
			TimeSeriesSpawned += DrawDotTimeElement;
			TimeSeriesSpawned += OnTimeSeriesSpawn;
			TimeSeriesFinishMoved += DrawDotTimeElement;
		}

		private void OnTimeSeriesSpawn() {

			foreach (TimeElement e in TimeElements) {
				e.transform.localScale *= DataPoint.Vis.Scale;
			}

		}

		private void DrawDotTimeElement() {

			if (!Configuration.isFading) return;

			foreach (TimeElement e in TimeElements) {

				if (ShouldDrawTimeElement(Configuration.visibleHistoryCount, e)) {
					e.GetComponentInChildren<Image>().material
						.SetFloat(Alpha, 1f - MathExtensions.Map01(Mathf.Abs(e.DistanceToDataPoint), 0, Configuration.visibleHistoryCount));
				}

			}

		}

		private void OnDisable() {
			TimeSeriesSpawned -= DrawDotTimeElement;
			TimeSeriesSpawned -= OnTimeSeriesSpawn;
			TimeSeriesFinishMoved -= DrawDotTimeElement;
		}

#endregion

	}

}