using Dataskop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public interface IVisObject {

		public int Index { get; set; }

		public bool IsFocused { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; }

		public CanvasGroup DataDisplay { get; }

		public TextMeshProUGUI IDTextMesh { get; }

		public TextMeshProUGUI ValueTextMesh { get; }

		public TextMeshProUGUI DateTextMesh { get; }

		public Image BoolIconRenderer { get; }

		public Image AuthorIconRenderer { get; }

		public void SetDisplayData(VisualizationResultDisplayData displayData);

		public void OnHover();

		public void OnSelect();

		public void OnDeselect();

		public void ShowDisplay();

		public void HideDisplay();

		public void SetMaterial(Material newMaterial);

		public void Delete();

	}

}