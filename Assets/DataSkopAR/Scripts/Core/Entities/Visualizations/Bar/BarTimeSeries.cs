using DataskopAR.Utils;
using UnityEngine;
namespace DataskopAR.Entities.Visualizations {

	public class BarTimeSeries : TimeSeries {

#region Fields

		[SerializeField] private Color timeElementColor;

#endregion

#region Methods

		private void Awake() {
			TimeSeriesSpawned += OnTimeSeriesSpawned;
			TimeSeriesStartMoved += OnTimeSeriesMoved;
		}

		private void OnTimeSeriesSpawned() {

			foreach (TimeElement te in TimeElements) {
				te.transform.localScale *= DataPoint.Vis.Scale;
				RotateDisplay(te.DataDisplay);
				SetBarHeight(te, te.MeasurementResult.ReadAsFloat(), DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum);
			}

		}

		private void OnTimeSeriesMoved() {

			foreach (TimeElement te in TimeElements) {
				SetBarHeight(te, te.MeasurementResult.ReadAsFloat(), DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum);
			}

		}

		private void RotateDisplay(Transform displayTransform) {
			displayTransform.localRotation = Quaternion.Euler(0, 0, 90);
		}

		private void SetBarHeight(TimeElement e, float value, float min, float max) {

			Transform pillarFill = e.gameObject.transform.GetChild(0).GetChild(0);
			value = Mathf.Clamp(value, min, max);
			Vector3 localScale = pillarFill.localScale;
			localScale = new Vector3(localScale.x, MathExtensions.Map(value, min, max, 0, 3), localScale.z);
			pillarFill.localScale = localScale;

			Material meshMaterial = e.gameObject.transform.GetChild(0).GetChild(0).GetComponentInChildren<MeshRenderer>().material;
			meshMaterial.color = timeElementColor;

			if (!Configuration.isFading) return;
			if (!ShouldDrawTimeElement(Configuration.visibleHistoryCount, e)) return;

			float alphaValue = MathExtensions.Map(Mathf.Abs(e.DistanceToDataPoint), 0, Configuration.visibleHistoryCount, 1, 0.25f);
			meshMaterial.color = new Color(meshMaterial.color.r, meshMaterial.color.g, meshMaterial.color.b, alphaValue);

		}

		private void OnDisable() {
			TimeSeriesSpawned -= OnTimeSeriesSpawned;
			TimeSeriesStartMoved -= OnTimeSeriesMoved;
		}

#endregion

	}

}