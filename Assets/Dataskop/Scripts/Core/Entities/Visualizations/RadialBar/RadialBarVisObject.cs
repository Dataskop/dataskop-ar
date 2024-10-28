using System;
using System.Collections;
using System.Collections.Generic;
using Dataskop.Entities.Visualizations;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class RadialBarVisObject : MonoBehaviour, IVisObject {

		public int Index { get; set; }

		public bool IsFocused { get; }

		public Collider VisCollider { get; }

		public Transform VisObjectTransform { get; }

		public VisObjectData CurrentData { get; }

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public void OnHover() {
			throw new NotImplementedException();
		}

		public void OnSelect() {
			throw new NotImplementedException();
		}

		public void OnDeselect() {
			throw new NotImplementedException();
		}

		public void OnHistoryToggle(bool active) {
			throw new NotImplementedException();
		}

		public void ChangeState(VisObjectState newState) {
			throw new NotImplementedException();
		}

		public void ApplyData(params VisObjectData[] data) {
			throw new NotImplementedException();
		}

		public void SetFocus(bool isFocused) {
			throw new NotImplementedException();
		}

		public void Delete() {
			throw new NotImplementedException();
		}

	}

}