using DataskopAR.Utils;
using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	public class BarTimeSeries : TimeSeries {

#region Fields

		[SerializeField] private Color timeElementColor;

#endregion

#region Methods

		private void Awake() {
			TimeSeriesSpawned += ScaleBars;
			TimeSeriesStartMoved += SetBarValue;
		}

		private void ScaleBars() {

			foreach (TimeElement te in TimeElements) {
				te.transform.localScale *= DataPoint.Vis.Scale;
				te.AuthorSprite.gameObject.transform.Rotate(new Vector3(0, 0, -90));
			}

			SetBarValue();

		}

		private void SetBarValue() {

			foreach (TimeElement te in TimeElements) {
				SetBarHeight(te, te.MeasurementResult.ReadAsFloat(), DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum);
			}

		}

		private void SetBarHeight(TimeElement e, float value, float min, float max) {

			Transform pillarFill = e.gameObject.transform.GetChild(0).GetChild(0);
			value = Mathf.Clamp(value, min, max);
			Vector3 localScale = pillarFill.localScale;
			localScale = new Vector3(localScale.x, MathExtensions.Map01(value, min, max), localScale.z);
			pillarFill.localScale = localScale;

			Material meshMaterial = e.gameObject.transform.GetChild(0).GetChild(0).GetComponentInChildren<MeshRenderer>().material;
			meshMaterial.color = timeElementColor;

			if (!Configuration.isFading) return;
			if (!ShouldDrawTimeElement(Configuration.visibleHistoryCount, e)) return;

			float alphaValue = MathExtensions.Map(Mathf.Abs(e.DistanceToDataPoint), 0, Configuration.visibleHistoryCount, 1, 0.25f);
			meshMaterial.color = new Color(meshMaterial.color.r, meshMaterial.color.g, meshMaterial.color.b, alphaValue);

		}

		private void OnDisable() {
			TimeSeriesSpawned -= ScaleBars;
			TimeSeriesStartMoved -= SetBarValue;
		}

#endregion

	}

}