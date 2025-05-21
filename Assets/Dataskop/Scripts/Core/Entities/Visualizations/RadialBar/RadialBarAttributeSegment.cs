using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class RadialBarAttributeSegment : MonoBehaviour {

		// Positive: ArcPoint 1: 0 ArcPoint 2: 180 (max) - 360 (min);
		// Negative: ArcPoint 1: 180 (max) - 360 (min); ArcPoint 2: 0

		private static readonly int Arc1 = Shader.PropertyToID("_Arc1");
		private static readonly int Arc2 = Shader.PropertyToID("_Arc2");
		private static readonly int Tint = Shader.PropertyToID("_Color");

		[SerializeField] private SpriteRenderer positiveDisplay;
		[SerializeField] private SpriteRenderer negativeDisplay;
		[SerializeField] private float unfocusedOpacity;

		public void SetColor(Color color) {

			positiveDisplay.color = color;
			negativeDisplay.color = color;

		}

		public void SetAngle(float angle, bool isNegative) {

			if (isNegative) {
				negativeDisplay.material.SetInt(Arc1, (int)angle);
				negativeDisplay.material.SetInt(Arc2, 0);
				return;
			}

			positiveDisplay.material.SetInt(Arc2, (int)angle);
			positiveDisplay.material.SetInt(Arc1, 0);

		}

		public void SetSortingOrder(int index) {
			positiveDisplay.sortingOrder = index;
			negativeDisplay.sortingOrder = index;
		}

		public void Focus() {
			positiveDisplay.material.SetColor(
				Tint, new Color(positiveDisplay.color.r, positiveDisplay.color.g, positiveDisplay.color.b, 1)
			);

			negativeDisplay.material.SetColor(
				Tint, new Color(positiveDisplay.color.r, positiveDisplay.color.g, positiveDisplay.color.b, 1)
			);
		}

		public void Unfocus() {
			positiveDisplay.material.SetColor(
				Tint, new Color(positiveDisplay.color.r, positiveDisplay.color.g, positiveDisplay.color.b, unfocusedOpacity)
			);

			negativeDisplay.material.SetColor(
				Tint, new Color(positiveDisplay.color.r, positiveDisplay.color.g, positiveDisplay.color.b, unfocusedOpacity)
			);
		}

	}

}
