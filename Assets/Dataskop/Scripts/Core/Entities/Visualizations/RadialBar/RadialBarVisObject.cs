using System;
using Dataskop.Utils;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class RadialBarVisObject : MonoBehaviour, IVisObject {

		[Header("References")]
		[SerializeField] private SpriteRenderer visRenderer;
		[SerializeField] private Collider visCollider;
		[SerializeField] private GameObject radialSegmentPrefab;
		[SerializeField] private GameObject liveIndicator;

		private bool isSelected;

		public int Index { get; set; }

		public bool IsFocused { get; private set; }

		public bool IsNew { get; private set; }

		public Collider VisCollider => visCollider;

		public Transform VisObjectTransform => transform;

		public VisObjectData CurrentData { get; private set; }

		private RadialBarAttributeSegment[] RadialSegments { get; set; }

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
				foreach (RadialBarAttributeSegment r in RadialSegments) {
					Destroy(r.gameObject);
				}
			}

			RadialSegments = new RadialBarAttributeSegment[data.Length];

			for (int i = 0; i < RadialSegments.Length; i++) {
				RadialSegments[i] = Instantiate(radialSegmentPrefab, transform)
					.GetComponent<RadialBarAttributeSegment>();

				RadialSegments[i].transform.localScale = new Vector2(1 + 0.1f * (i + 1), 1 + 0.1f * (i + 1));

				RadialSegments[i].SetColor(data[i].Color);
				RadialSegments[i].SetSortingOrder((i + 1) * -1);

				float min = Mathf.Abs(data[i].Attribute.Minimum);
				float max = Mathf.Abs(data[i].Attribute.Maximum);
				float mappedMax = min > max ? min : max;

				int angle = GetMappedAngle(Mathf.Abs(data[i].Result.ReadAsFloat()), 0, mappedMax);
				Debug.Log($"{data[i].Attribute.ID}: {angle}");
				RadialSegments[i].SetAngle(360 - angle, data[i].Result.ReadAsFloat() < 0);

			}

		}

		public void SetFocus(bool isFocused) {
			IsFocused = isFocused;
		}

		public void Delete() {
			Destroy(gameObject);
		}

		public void SetLatestState(bool state) { }

		public void SetNewState(bool state) { }

		private int GetMappedAngle(float value, float min, float max) {
			return (int)MathExtensions.Map(value, min, max, 0, 180);
		}

	}

}
