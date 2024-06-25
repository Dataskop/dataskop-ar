using System.Globalization;
using Dataskop.Data;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class TimeElement : MonoBehaviour {

 

		[Header("References")]
		[SerializeField] private Image authorImageRenderer;
		[SerializeField] private CanvasGroup dataDisplayGroup;
		[SerializeField] private Transform dataDisplay;
		[SerializeField] private Transform authorDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] [CanBeNull] private TextMeshProUGUI minTextMesh = null!;
		[SerializeField] [CanBeNull] private TextMeshProUGUI maxTextMesh = null!;

		private MeasurementResult measurementResult;

  

 

		public TimeSeries Series { get; set; }

		public Vector3 NextTargetPosition { get; set; }

		public MeasurementResult MeasurementResult {
			get => measurementResult;
			set {
				measurementResult = value;
				SetAuthorSprite();
				SetDisplayData();
			}
		}

		public int DistanceToDataPoint { get; set; }

		private Image AuthorSprite => authorImageRenderer;

  

 

		private void SetAuthorSprite() {

			if (MeasurementResult.Author != string.Empty) {
				AuthorSprite.sprite = Series.DataPoint.AuthorRepository.AuthorSprites[MeasurementResult.Author];
				AuthorSprite.enabled = true;
			}
			else {
				AuthorSprite.enabled = false;
			}

		}

		public void SetDisplayData() {

			valueTextMesh.SetText(MeasurementResult.Value + $" {Series.DataPoint.Attribute?.Unit}");
			dateTextMesh.SetText(MeasurementResult.GetTime());
			idTextMesh.SetText(Series.DataPoint.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper());

			if (minTextMesh != null && maxTextMesh != null) {
				SetMinMaxDisplayValues(Series.DataPoint.Attribute.Minimum, Series.DataPoint.Attribute.Maximum);
			}

		}

		private void SetMinMaxDisplayValues(float min, float max) {
			minTextMesh!.text = min.ToString("00.00", CultureInfo.InvariantCulture) + $" {Series.DataPoint.Attribute?.Unit}";
			maxTextMesh!.text = max.ToString("00.00", CultureInfo.InvariantCulture) + $" {Series.DataPoint.Attribute?.Unit}";
		}

		public void DisplayData() {
			dataDisplayGroup.alpha = 1;
		}

		public void HideData() {
			dataDisplayGroup.alpha = 0;
		}

  

	}

}