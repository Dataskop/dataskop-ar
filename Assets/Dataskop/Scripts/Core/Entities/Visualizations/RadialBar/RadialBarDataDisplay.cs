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
		[SerializeField] private TextMeshProUGUI unitSymbol;
		[SerializeField] private Image legendDots;
		[SerializeField] private Image upArrow;
		[SerializeField] private Image downArrow;
		[SerializeField] private Image downArrowShadow;
		[SerializeField] private Image upArrowShadow;

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

				if (currentDataIndex == 0) {
					return;
				}

				currentDataIndex--;

			}
			else {

				if (currentDataIndex == DataSet.Length - 1) {
					return;
				}

				currentDataIndex++;

			}

			ApplyData(currentDataIndex);

		}

		private void ApplyData(int index) {
			VisObjectData data = DataSet[index];
			float receivedValue = data.Result.ReadAsFloat();
			valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture);
			unitSymbol.text = data.Attribute.Unit;
			dateTextMesh.text = data.Result.GetDateText();
			legendDots.color = data.Color;
			minMaxTextMesh.text =
				$"{data.Attribute.Minimum} {data.Attribute.Unit} - {data.Attribute.Maximum} {data.Attribute.Unit}";

			idTextMesh.text = data.Result.MeasurementDefinition.MeasurementDefinitionInformation.Name;
			SetArrowState();
		}

		private void SetArrowState() {

			downArrow.color = currentDataIndex == 0 ?
				new Color32(255, 200, 100, 125) :
				new Color32(255, 200, 100, 255);

			upArrow.color = currentDataIndex == DataSet.Length - 1 ?
				new Color32(255, 200, 100, 125) :
				new Color32(255, 200, 100, 255);

			downArrowShadow.enabled = currentDataIndex != 0;
			upArrowShadow.enabled = currentDataIndex != DataSet.Length - 1;

		}

	}

}