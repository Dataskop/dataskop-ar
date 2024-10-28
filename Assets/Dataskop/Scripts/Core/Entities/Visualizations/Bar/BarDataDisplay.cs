using System.Globalization;
using Dataskop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class BarDataDisplay : MonoBehaviour {

		[Header("References")]
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] private TextMeshProUGUI maxValueTextMesh;
		[SerializeField] private TextMeshProUGUI minValueTextMesh;
		[SerializeField] private Image authorIconImageRenderer;

		public void SetDisplayData(VisObjectData displayData) {

			idTextMesh.text = displayData.Result.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();

			switch (displayData.Type) {
				case MeasurementType.Float: {
					float receivedValue = displayData.Result.ReadAsFloat();
					valueTextMesh.alpha = 1;
					valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {displayData.Attribute.Unit}";
					dateTextMesh.text = displayData.Result.GetDateText();
					minValueTextMesh.text = displayData.Attribute.Minimum.ToString("00.00", CultureInfo.InvariantCulture) +
					                        $" {displayData.Attribute.Unit}";
					maxValueTextMesh.text = displayData.Attribute.Maximum.ToString("00.00", CultureInfo.InvariantCulture) +
					                        $" {displayData.Attribute.Unit}";
					break;
				}
				case MeasurementType.Bool: {
					valueTextMesh.alpha = 1;
					valueTextMesh.text = displayData.Result.ReadAsBool().ToString();
					dateTextMesh.text = displayData.Result.GetDateText();
					minValueTextMesh.text = "";
					maxValueTextMesh.text = "";
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

		public void Select() {
			valueTextMesh.color = Colors.Selected;
		}

		public void Deselect(bool isFocused) {
			valueTextMesh.color = isFocused ? Colors.Deselected : Colors.HistoricLight;
		}

		public void Hover(bool isFocused) {
			valueTextMesh.color = isFocused ? Colors.Hovered : Colors.HistoricLight;
		}

		public void MoveTo(Vector3 position) {
			transform.position = new Vector3(position.x, position.y, position.z);
		}

		public void Show() {
			dataDisplay.alpha = 1;
		}

		public void Hide() {
			dataDisplay.alpha = 0;
		}

		public void Rotate(bool isRotated, float frameYScale, float frameXScale) {

			dataDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(
				isRotated ? frameYScale * 100 : frameXScale * 100,
				isRotated ? frameXScale * 100 : frameYScale * 100
			);

			RectTransform maxValueTransform = maxValueTextMesh.GetComponent<RectTransform>();
			maxValueTransform.anchorMin = isRotated ? new Vector2(1, 0) : new Vector2(0, 1);
			maxValueTransform.pivot = isRotated ? new Vector2(1, 0.5f) : new Vector2(0.5f, 1);

			maxValueTransform.sizeDelta = isRotated
				? new Vector2(80, maxValueTransform.sizeDelta.y)
				: new Vector2(maxValueTransform.sizeDelta.x, 10);

			RectTransform minValueTransform = minValueTextMesh.GetComponent<RectTransform>();
			minValueTransform.anchorMax = isRotated ? new Vector2(0, 1) : new Vector2(1, 0);
			minValueTransform.pivot = isRotated ? new Vector2(0, 0.5f) : new Vector2(0.5f, 0);

			minValueTransform.sizeDelta = isRotated
				? new Vector2(80, minValueTransform.sizeDelta.y)
				: new Vector2(minValueTransform.sizeDelta.x, 10);

			RectTransform authorIconTransform = authorIconImageRenderer.GetComponent<RectTransform>();
			authorIconTransform.anchorMax = isRotated ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);
			authorIconTransform.anchorMin = isRotated ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);
			authorIconTransform.pivot = isRotated ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);
			authorIconTransform.anchoredPosition = isRotated ? new Vector2(40, 0) : new Vector2(0, -40);

			maxValueTextMesh.alignment = isRotated ? TextAlignmentOptions.Right : TextAlignmentOptions.Center;
			minValueTextMesh.alignment = isRotated ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
		}

	}

}