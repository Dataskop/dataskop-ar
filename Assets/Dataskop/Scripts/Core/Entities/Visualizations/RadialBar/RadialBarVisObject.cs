using System;
using Dataskop.Utils;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class RadialBarVisObject : MonoBehaviour, IVisObject {

		private static readonly int Arc2 = Shader.PropertyToID("_Arc2");

		[Header("References")]
		[SerializeField] private SpriteRenderer visRenderer;
		[SerializeField] private Collider visCollider;
		[SerializeField] private GameObject radialSegmentPrefab;

		private bool isSelected;

		public int Index { get; set; }

		public bool IsFocused { get; private set; }

		public Collider VisCollider => visCollider;

		public Transform VisObjectTransform => transform;

		public VisObjectData CurrentData { get; private set; }

		private GameObject[] RadialSegments { get; set; }

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public void OnHover() {
			HasHovered?.Invoke(Index);
		}

		public void OnSelect() {
			HasSelected?.Invoke(Index);
		}

		public void OnDeselect() {
			HasDeselected?.Invoke(Index);
		}

		public void OnHistoryToggle(bool active) {
			// Intentionally empty body
			return;
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

			if (RadialSegments?.Length > 0) {
				foreach (GameObject r in RadialSegments) {
					Destroy(r);
				}
			}

			RadialSegments = new GameObject[data.Length];

			for (int i = 0; i < RadialSegments.Length; i++) {
				RadialSegments[i] = Instantiate(radialSegmentPrefab, transform);

				RadialSegments[i].transform.localScale = new Vector2(1 + 0.1f * (i + 1), 1 + 0.1f * (i + 1));
				SpriteRenderer sr = RadialSegments[i].GetComponent<SpriteRenderer>();
				sr.color = data[i].Color;
				sr.sortingOrder = (i + 1) * -1;
				int angle = GetMappedAngle(
					data[i].Result.ReadAsFloat(), data[i].Attribute.Minimum, data[i].Attribute.Maximum
				);

				sr.material.SetInt(Arc2, angle);
			}

		}

		public void SetFocus(bool isFocused) {
			IsFocused = isFocused;
		}

		public void Delete() {
			Destroy(gameObject);
		}

		private int GetMappedAngle(float value, float min, float max) {
			return 360 - (int)MathExtensions.Map(value, min, max, 0, 360);
		}

	}

}
