using Dataskop.Utils;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class BarTimeSeries : TimeSeries {

#region Fields

		[SerializeField] private Color timeElementColor;

#endregion

#region Methods

		private void Awake() {
			TimeElementSpawned += OnTimeElementSpawned;
			TimeElementMoved += OnTimeElementMoved;
		}

		private void OnTimeElementSpawned(TimeElement e) {
			e.transform.localScale *= DataPoint.Vis.Scale;
			SetBarHeight(e, e.MeasurementResult.ReadAsFloat(), DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum);
		}

		private void OnTimeElementMoved(TimeElement e) {
			SetBarHeight(e, e.MeasurementResult.ReadAsFloat(), DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum);
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
			TimeElementSpawned -= OnTimeElementSpawned;
			TimeElementMoved += OnTimeElementMoved;
		}

#endregion

	}

}