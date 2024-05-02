using Dataskop.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class DotTimeSeries : TimeSeries {

#region Constants

		private static readonly int Alpha = Shader.PropertyToID("_Alpha");

#endregion

#region Methods

		private void Awake() {
			TimeElementSpawned += OnTimeElementSpawned;
			TimeElementMoved += DrawDotTimeElement;
		}

		private void OnTimeElementSpawned(TimeElement e) {
			e.transform.localScale *= DataPoint.Vis.Scale;
			DrawDotTimeElement(e);
		}

		private void DrawDotTimeElement(TimeElement e) {

			if (!Configuration.isFading) return;

			if (ShouldDrawTimeElement(Configuration.visibleHistoryCount, e)) {
				e.GetComponentInChildren<Image>().material
					.SetFloat(Alpha, 1f - MathExtensions.Map01(Mathf.Abs(e.DistanceToDataPoint), 0, Configuration.visibleHistoryCount));
			}

		}

		private void OnDisable() {
			TimeElementSpawned -= OnTimeElementSpawned;
			TimeElementMoved -= DrawDotTimeElement;
		}

#endregion

	}

}