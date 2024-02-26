using DataskopAR.Data;
using TMPro;
using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	public class TimeElement : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private SpriteRenderer authorSpriteRenderer;
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private TextMeshProUGUI valueDisplay;
		[SerializeField] private TextMeshProUGUI dateDisplay;

		private MeasurementResult measurementResult;

#endregion

#region Properties

		public TimeSeries Series { get; set; }
		public Vector3 NextTargetPosition { get; set; }

		public MeasurementResult MeasurementResult {
			get => measurementResult;
			set {
				measurementResult = value;
				SetAuthorSprite();
			}
		}

		public int DistanceToDataPoint { get; set; }

		public SpriteRenderer AuthorSprite {
			get => authorSpriteRenderer;
		}

#endregion

#region Methods

		private void SetAuthorSprite() {

			if (MeasurementResult.Author != string.Empty) {
				authorSpriteRenderer.sprite = Series.DataPoint.AuthorRepository.AuthorSprites[MeasurementResult.Author];
				authorSpriteRenderer.enabled = true;
			}
			else {
				authorSpriteRenderer.enabled = false;
			}

		}

		public void SetDisplayData() {

			valueDisplay.SetText(MeasurementResult.Value);
			dateDisplay.SetText(MeasurementResult.GetTime());

		}

		public void DisplayData() {
			dataDisplay.alpha = 1;
		}

		public void HideData() {
			dataDisplay.alpha = 0;
		}

#endregion

	}

}