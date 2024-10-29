using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class RadialBarDataDisplay : MonoBehaviour {

		[Header("References")]
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private TextMeshProUGUI[] valueTextMesh;
		[SerializeField] private TextMeshProUGUI[] dateTextMesh;
		[SerializeField] private Image[] legendDots;

		public void SetDisplayData(params VisObjectData[] data) {

			for (int i = 0; i < data.Length; i++) {
				float receivedValue = data[i].Result.ReadAsFloat();
				valueTextMesh[i].text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {data[i].Attribute.Unit}";
				dateTextMesh[i].text = data[i].Result.GetDateText();
				legendDots[i].color = data[i].Color;
			}

		}

		public void Select() {
			//valueTextMesh.color = Colors.Selected;
		}

		public void Deselect(bool isFocused) {
			//valueTextMesh.color = isFocused ? Colors.Deselected : Colors.Historic;
		}

		public void Hover(bool isFocused) {
			//valueTextMesh.color = isFocused ? Colors.Hovered : Colors.Historic;
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