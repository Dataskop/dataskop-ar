using System.Globalization;
using Dataskop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class RadialBarDataDisplay : MonoBehaviour {

		[Header("References")]
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;

		public void SetDisplayData(params VisObjectData[] data) {

			var currentData = data[0];

			idTextMesh.text = currentData.Result.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();

			switch (currentData.Type) {
				case MeasurementType.Float: {
					float receivedValue = currentData.Result.ReadAsFloat();
					valueTextMesh.alpha = 1;
					valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {currentData.Attribute.Unit}";
					dateTextMesh.text = currentData.Result.GetDateText();
					break;
				}
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