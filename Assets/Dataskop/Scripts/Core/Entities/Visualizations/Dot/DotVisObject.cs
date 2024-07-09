using System;
using System.Globalization;
using Dataskop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class DotVisObject : MonoBehaviour, IVisObject {

		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] private Image boolIconRenderer;
		[SerializeField] private Image authorIconImageRenderer;
		[SerializeField] private Color32 boolTrueColor;
		[SerializeField] private Color32 boolFalseColor;

		public bool IsFocused { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; } = {
			MeasurementType.Float,
			MeasurementType.Bool
		};

		public CanvasGroup DataDisplay => dataDisplay;

		public TextMeshProUGUI IDTextMesh => idTextMesh;

		public TextMeshProUGUI ValueTextMesh => valueTextMesh;

		public TextMeshProUGUI DateTextMesh => dateTextMesh;

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
					break;
				}
				case MeasurementType.Bool: {
					valueTextMesh.alpha = 1;
					boolIconRenderer.enabled = false;
					valueTextMesh.text = displayData.Result.ReadAsBool().ToString();
					int boolValue = displayData.Result.ReadAsBool() ? 1 : 0;
					boolIconRenderer.color = displayData.Result.ReadAsBool() ? boolTrueColor : boolFalseColor;
					boolIconRenderer.sprite = null;
					dateTextMesh.text = displayData.Result.GetTime();
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

		public void ShowDisplay() {
			dataDisplay.alpha = 1;
		}

		public void HideDisplay() {
			dataDisplay.alpha = 0;
		}

		public void SetInteractionColor() {
			throw new NotImplementedException();
		}

	}

}