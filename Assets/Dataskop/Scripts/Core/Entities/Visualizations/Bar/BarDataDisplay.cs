using System.Globalization;
using Dataskop.Data;
using Dataskop.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class BarDataDisplay : MonoBehaviour {

		[Header("References")]
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private CanvasGroup authorDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] private TextMeshProUGUI maxValueTextMesh;
		[SerializeField] private TextMeshProUGUI minValueTextMesh;
		[SerializeField] private Image boolIconRenderer;
		[SerializeField] private Sprite[] boolIcons;
		[SerializeField] private Image authorIconImageRenderer;

		[Header("Values")]
		[SerializeField] private Color32 boolTrueColor;
		[SerializeField] private Color32 boolFalseColor;

		public void SetDisplayData(VisObjectData displayData) {

			idTextMesh.text = displayData.Result.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();

			switch (displayData.Type) {
				case MeasurementType.Bool: {
					float receivedValue = displayData.Result.ReadAsBool() ? 1 : 0;
					valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {displayData.Attribute?.Unit}";
					dateTextMesh.text = displayData.Result.GetDateText();
					break;
				}
				case MeasurementType.Float: {
					float receivedValue = displayData.Result.ReadAsFloat();
					valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {displayData.Attribute?.Unit}";
					minValueTextMesh.text = displayData.Attribute?.Minimum.ToString("00.00", CultureInfo.InvariantCulture) +
					                        $" {displayData.Attribute?.Unit}";
					maxValueTextMesh.text = displayData.Attribute?.Maximum.ToString("00.00", CultureInfo.InvariantCulture) +
					                        $" {displayData.Attribute?.Unit}";
					dateTextMesh.text = displayData.Result.GetDateText();
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

		private void Rotate(bool isRotated) {

			if (isRotated) {
				transform.localRotation = Quaternion.Euler(0, 0, -90);
				dataDisplay.transform.localRotation = Quaternion.Euler(0, 0, 90);
				authorDisplay.transform.localRotation = Quaternion.Euler(0, 0, 90);
			}
			else {
				dataDisplay.transform.localRotation = Quaternion.Euler(0, 0, 0);
				authorDisplay.transform.localRotation = Quaternion.Euler(0, 0, 0);
				transform.localRotation = Quaternion.Euler(0, 0, 0);
			}

			/*dataDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(
				isRotated ? barFrame.localScale.y * 100 : barFrame.localScale.x * 100,
				isRotated ? barFrame.localScale.x * 100 : barFrame.localScale.y * 100
			);
			*/

			/*
			authorDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(
				isRotated ? barFrame.localScale.y * 100 : barFrame.localScale.x * 100,
				isRotated ? barFrame.localScale.x * 100 : barFrame.localScale.y * 100
			);
			*/

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

		public void MoveTo(Vector3 position) {
			transform.position = position;
		}

	}

}