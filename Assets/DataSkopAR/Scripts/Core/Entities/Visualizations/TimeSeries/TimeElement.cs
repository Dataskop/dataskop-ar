using DataskopAR.Data;
using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	public class TimeElement : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private SpriteRenderer authorSpriteRenderer;
		[SerializeField] private CanvasGroup dataDisplay;

		private int _distanceToDataPoint;
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

		public int DistanceToDataPoint {
			get => _distanceToDataPoint;
			set => _distanceToDataPoint = value;
		}

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

		public void DisplayData() {
			dataDisplay.alpha = 1;
		}

		public void HideDate() {
			dataDisplay.alpha = 0;
		}

#endregion

	}

}