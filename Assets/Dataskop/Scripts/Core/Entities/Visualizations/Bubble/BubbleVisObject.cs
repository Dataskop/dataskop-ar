using System;
using System.Globalization;
using Dataskop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class BubbleVisObject : MonoBehaviour, IVisObject {

		[Header("References")]
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] private Image visRenderer;
		[SerializeField] private Image boolIconRenderer;
		[SerializeField] private Image authorIconImageRenderer;
		[SerializeField] private Sprite[] boolIcons;
		[SerializeField] private SphereCollider visCollider;

		[Header("Values")]
		[SerializeField] private Color32 boolTrueColor;
		[SerializeField] private Color32 boolFalseColor;
		[SerializeField] private AnimationCurve scaleCurve;
		[SerializeField] public float minScale;
		[SerializeField] public float maxScale;

		private Coroutine scaleRoutine;
		private Coroutine animationCoroutine;
		private Coroutine moveLineCoroutine;
		private Vector3 animationTarget;
		private bool isSelected;

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public int Index { get; set; }

		public bool IsFocused { get; set; }

		public CanvasGroup DataDisplay => dataDisplay;

		public TextMeshProUGUI IDTextMesh => idTextMesh;

		public TextMeshProUGUI ValueTextMesh => valueTextMesh;

		public TextMeshProUGUI DateTextMesh => dateTextMesh;

		public Image VisRenderer => visRenderer;

		public Image BoolIconRenderer => boolIconRenderer;

		public Image AuthorIconRenderer => authorIconImageRenderer;

		public void SetDisplayData(VisualizationResultDisplayData displayData) {

			idTextMesh.text = displayData.Result.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();

			switch (displayData.Type) {

				case MeasurementType.Float: {
					float receivedValue = displayData.Result.ReadAsFloat();
					boolIconRenderer.enabled = false;
					valueTextMesh.alpha = 1;
					valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {displayData.Attribute.Unit}";
					dateTextMesh.text = displayData.Result.GetTime();
					OnMeasurementResultUpdated(receivedValue, displayData.Attribute.Minimum, displayData.Attribute.Maximum);
					break;
				}
				case MeasurementType.Bool: {
					float receivedValue = displayData.Result.ReadAsBool() ? 1 : 0;
					valueTextMesh.alpha = 1;
					boolIconRenderer.enabled = false;
					valueTextMesh.text = displayData.Result.ReadAsBool().ToString();
					boolIconRenderer.color = displayData.Result.ReadAsBool() ? boolTrueColor : boolFalseColor;
					boolIconRenderer.sprite = receivedValue == 0 ? boolIcons[0] : boolIcons[1];
					dateTextMesh.text = displayData.Result.GetTime();
					OnMeasurementResultUpdated(receivedValue, displayData.Attribute.Minimum, displayData.Attribute.Maximum);
					break;
				}

			}

			if (displayData.Result.Author != string.Empty) {
				authorIconImageRenderer.sprite = displayData.AuthorSprite;
				authorIconImageRenderer.enabled = true;
			}
			else {
				authorIconImageRenderer.enabled = false;
			}

		}

		public void OnHover() {
			HasHovered?.Invoke(Index);
		}

		public void OnSelect() {
			isSelected = true;
			HasSelected?.Invoke(Index);
		}

		public void OnDeselect() {

			if (isSelected) {
				isSelected = false;
			}

			HasDeselected?.Invoke(Index);
		}

		public void ShowDisplay() {
			dataDisplay.alpha = 1;
		}

		public void HideDisplay() {
			dataDisplay.alpha = 0;
		}

		public void SetMaterial(Material newMaterial) {
			visRenderer.material = newMaterial;
			valueTextMesh.color = newMaterial.color;
		}

		public void Delete() {
			Destroy(gameObject);
		}

		private void OnMeasurementResultUpdated(float value, float minAttributeValue, float maxAttributeValue) {

			float newSize = BubbleUtils.CalculateRadius(value, minAttributeValue, maxAttributeValue, minScale, maxScale);
			Vector3 newBubbleScale = new(newSize, newSize, newSize);

			Transform visTransform = visRenderer.transform;

			if (scaleRoutine != null) {
				StopCoroutine(scaleRoutine);
			}

			scaleRoutine = StartCoroutine(Lerper.TransformLerpOnCurve(visTransform, TransformValue.Scale, visTransform.localScale,
				newBubbleScale, 0.12f, scaleCurve, null));
			visCollider.radius = newSize / 2f;

		}

	}

}