using DataskopAR.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DataskopAR.Entities.Visualizations {

	public class TimeElement : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private Image authorImageRenderer;
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

		public Image AuthorSprite => authorImageRenderer;

#endregion

#region Methods

		private void SetAuthorSprite() {

			if (MeasurementResult.Author != string.Empty) {
				authorImageRenderer.sprite = Series.DataPoint.AuthorRepository.AuthorSprites[MeasurementResult.Author];
				authorImageRenderer.enabled = true;
			}
			else {
				authorImageRenderer.enabled = false;
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