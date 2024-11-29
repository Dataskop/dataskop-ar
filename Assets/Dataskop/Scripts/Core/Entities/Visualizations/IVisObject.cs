using System;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public interface IVisObject {

		public int Index { get; set; }

		public bool IsFocused { get; }

		public Collider VisCollider { get; }

		public Transform VisObjectTransform { get; }

		public VisObjectData CurrentData { get; }

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public void OnHover();

		public void OnSelect();

		public void OnDeselect();

		public void OnHistoryToggle(bool active);

		public void ChangeState(VisObjectState newState);

		/// <summary>
		/// Apply changes to the visualizations based on the data.
		/// </summary>
		/// <param name="data">The data used for the vis object.</param>
		public void ApplyData(params VisObjectData[] data);

		public void SetFocus(bool isFocused);

		public void Delete();

		public void SetLatestState(bool state);

		public void SetNewState(bool state);

	}

}
