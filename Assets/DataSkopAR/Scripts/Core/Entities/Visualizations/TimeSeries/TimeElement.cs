using DataskopAR.Data;
using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	public class TimeElement : MonoBehaviour {

		[SerializeField] private int _distanceToDataPoint;
		[SerializeField] private SpriteRenderer authorSpriteRenderer;

		private MeasurementResult measurementResult;

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

#endregion

	}

}