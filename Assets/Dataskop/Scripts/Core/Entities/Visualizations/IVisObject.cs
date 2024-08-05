using System;
using Dataskop.Data;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public interface IVisObject {

		public int Index { get; set; }

		public bool IsFocused { get; set; }

		public Collider VisCollider { get; }

		public Transform VisObjectTransform { get; }

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public void SetDisplayData(VisualizationResultDisplayData displayData);

		public void OnHover();

		public void OnSelect();

		public void OnDeselect();

		public void ShowDisplay();

		public void HideDisplay();

		public void OnHistoryToggle(bool active);

		public void SetMaterials(params Material[] materials);

		public void Delete();

	}

}