using System;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class RadialBarVisObject : MonoBehaviour, IVisObject {

		[Header("References")]
		[SerializeField] private SpriteRenderer visRenderer;
		[SerializeField] private Collider visCollider;

		private bool isSelected;

		public int Index { get; set; }

		public bool IsFocused { get; private set; }

		public Collider VisCollider => visCollider;

		public Transform VisObjectTransform => transform;

		public VisObjectData CurrentData { get; private set; }

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public void OnHover() => HasHovered?.Invoke(Index);

		public void OnSelect() => HasSelected?.Invoke(Index);

		public void OnDeselect() => HasDeselected?.Invoke(Index);

		public void OnHistoryToggle(bool active) {
			// Intentionally empty body
		}

		public void ChangeState(VisObjectState newState) {
			switch (newState) {
				case VisObjectState.Deselected:
					if (isSelected) {
						isSelected = false;
					}
					break;
				case VisObjectState.Hovered:
					if (isSelected && IsFocused) {
						return;
					}
					break;
				case VisObjectState.Selected:
					isSelected = true;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
			}
		}

		public void ApplyData(params VisObjectData[] data) {
			Debug.Log(data);
		}

		public void SetFocus(bool isFocused) {
			IsFocused = isFocused;
		}

		public void Delete() {
			Destroy(gameObject);
		}

	}

}