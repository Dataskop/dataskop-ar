using DataskopAR.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DataskopAR.Entities.Visualizations {

	public class TimeElement : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private Image authorImageRenderer;
		[SerializeField] private CanvasGroup dataDisplayGroup;
		[SerializeField] private Transform dataDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;

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

		public Transform DataDisplay => dataDisplay;

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

			valueTextMesh.SetText(MeasurementResult.Value);
			dateTextMesh.SetText(MeasurementResult.GetTime());
			idTextMesh.SetText(Series.DataPoint.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper());

		}

		public void DisplayData() {
			dataDisplayGroup.alpha = 1;
		}

		public void HideData() {
			dataDisplayGroup.alpha = 0;
		}

#endregion

	}

}