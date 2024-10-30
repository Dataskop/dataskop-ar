using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class RadialBarDataDisplay : MonoBehaviour {

		[Header("References")]
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] private TextMeshProUGUI minMaxTextMesh;
		[SerializeField] private Image legendDots;

		private int currentDataIndex = 0;

		private VisObjectData[] DataSet { get; set; }

		public void SetDisplayData(params VisObjectData[] data) {
			DataSet = data;
			ApplyData(currentDataIndex);
		}

		public void Select() { }

		public void Deselect(bool isFocused) { }

		public void Hover(bool isFocused) { }

		public void Show() {
			dataDisplay.alpha = 1;
		}

		public void Hide() {
			dataDisplay.alpha = 0;
		}

		public void OnSwipe(Vector2 direction) {

			if (direction.y > 0) {

				if (currentDataIndex == DataSet.Length - 1) {
					return;
				}

				currentDataIndex++;
			}
			else {

				if (currentDataIndex == 0) {
					return;
				}

				currentDataIndex--;
			}

			ApplyData(currentDataIndex);

		}

		private void ApplyData(int index) {
			VisObjectData data = DataSet[index];
			float receivedValue = data.Result.ReadAsFloat();
			valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {data.Attribute.Unit}";
			dateTextMesh.text = data.Result.GetDateText();
			legendDots.color = data.Color;
			minMaxTextMesh.text = $"{data.Attribute.Minimum} {data.Attribute.Unit} - {data.Attribute.Maximum} {data.Attribute.Unit}";
			idTextMesh.text = data.Result.MeasurementDefinition.MeasurementDefinitionInformation.Name;
		}

	}

}