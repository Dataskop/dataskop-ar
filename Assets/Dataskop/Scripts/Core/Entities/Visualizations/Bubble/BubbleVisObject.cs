using System;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class BubbleVisObject : MonoBehaviour, IVisObject {

		[Header("References")]
		[SerializeField] private SpriteRenderer visRenderer;
		[SerializeField] private SphereCollider visCollider;
		[SerializeField] private Sprite defaultSprite;
		[SerializeField] private Sprite hoveredSprite;
		[SerializeField] private Sprite selectedSprite;
		[SerializeField] private Sprite historicSprite;

		[Header("Values")]
		[SerializeField] private AnimationCurve scaleCurve;
		[SerializeField] public float minScale; // 1
		[SerializeField] public float maxScale; // 2.2

		private Coroutine animationCoroutine;
		private Vector3 animationTarget;
		private bool isSelected;
		private Coroutine moveLineCoroutine;

		private Coroutine scaleRoutine;

		public Transform VisObjectTransform => transform;

		public VisObjectData CurrentData { get; private set; }

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public int Index { get; set; }

		public bool IsFocused { get; set; }

		public Collider VisCollider => visCollider;

		public void OnHover() {
			HasHovered?.Invoke(Index);
		}

		public void OnSelect() {
			isSelected = true;
			HasSelected?.Invoke(Index);
		}

		public void OnDeselect() {

			if (isSelected) {
				isSelected = false;
			}

			HasDeselected?.Invoke(Index);
		}

		public void OnHistoryToggle(bool active) {
			// Intentionally empty body
		}

		public void ChangeState(VisObjectState newState) {
			switch (newState) {

				case VisObjectState.Deselected:
					if (isSelected) {
						isSelected = false;
					}

					visRenderer.sprite = IsFocused ? defaultSprite : historicSprite;
					break;
				case VisObjectState.Hovered:

					if (isSelected && IsFocused) {
						return;
					}

					visRenderer.sprite = IsFocused ? hoveredSprite : historicSprite;
					break;
				case VisObjectState.Selected:
					isSelected = true;
					visRenderer.sprite = selectedSprite;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
			}
		}

		public void ApplyData(params VisObjectData[] data) {
			CurrentData = data[0];
			SetBubbleSize(
				CurrentData.Result.ReadAsFloat(), CurrentData.Attribute.Minimum, CurrentData.Attribute.Maximum
			);
		}

		public void SetFocus(bool isFocused) {
			IsFocused = isFocused;

			if (isSelected) {
				visRenderer.sprite = IsFocused ? selectedSprite : historicSprite;
			}
			else {
				visRenderer.sprite = IsFocused ? defaultSprite : historicSprite;
			}
		}

		public void Delete() {
			Destroy(gameObject);
		}

		private void SetBubbleSize(float value, float minAttributeValue, float maxAttributeValue) {

			float newSize = BubbleUtils.CalculateRadius(
				value, minAttributeValue, maxAttributeValue, minScale, maxScale
			);

			Vector3 newBubbleScale = new(newSize, newSize, newSize);

			Transform visTransform = visRenderer.transform;

			if (scaleRoutine != null) {
				StopCoroutine(scaleRoutine);
			}

			scaleRoutine = StartCoroutine(
				Lerper.TransformLerpOnCurve(
					visTransform, TransformValue.Scale, visTransform.localScale,
					newBubbleScale, 0.12f, scaleCurve, null
				)
			);

		}

	}

}