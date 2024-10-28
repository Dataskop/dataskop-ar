using System.Globalization;
using Dataskop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class DotDataDisplay : MonoBehaviour {

		[Header("References")]
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] private Image boolIconRenderer;
		[SerializeField] private Sprite[] boolIcons;
		[SerializeField] private Image authorIconImageRenderer;

		[Header("Values")]
		[SerializeField] private Color32 boolTrueColor;
		[SerializeField] private Color32 boolFalseColor;

		public void SetDisplayData(VisObjectData displayData) {

			idTextMesh.text = displayData.Result.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();

			switch (displayData.Type) {
				case MeasurementType.Float: {
					float receivedValue = displayData.Result.ReadAsFloat();
					boolIconRenderer.enabled = false;
					valueTextMesh.alpha = 1;
					valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {displayData.Attribute.Unit}";
					dateTextMesh.text = displayData.Result.GetDateText();
					break;
				}
				case MeasurementType.Bool: {
					valueTextMesh.alpha = 1;
					boolIconRenderer.enabled = false;
					valueTextMesh.text = displayData.Result.ReadAsBool().ToString();
					int boolValue = displayData.Result.ReadAsBool() ? 1 : 0;
					boolIconRenderer.color = displayData.Result.ReadAsBool() ? boolTrueColor : boolFalseColor;
					boolIconRenderer.sprite = boolValue == 0 ? boolIcons[0] : boolIcons[1];
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

		public void Select() {
			valueTextMesh.color = Colors.Selected;
		}

		public void Deselect(bool isFocused) {
			valueTextMesh.color = isFocused ? Colors.Deselected : Colors.Historic;
		}

		public void Hover(bool isFocused) {
			valueTextMesh.color = isFocused ? Colors.Hovered : Colors.Historic;
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

	}

}