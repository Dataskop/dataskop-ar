using System;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public class DotVisObject : MonoBehaviour, IVisObject {

		[Header("References")]
		[SerializeField] private SpriteRenderer visRenderer;
		[SerializeField] private Collider visCollider;
		[SerializeField] private Sprite defaultSprite;
		[SerializeField] private Sprite hoveredSprite;
		[SerializeField] private Sprite selectedSprite;
		[SerializeField] private Sprite historicSprite;

		[Header("Values")]
		[SerializeField] private AnimationCurve animationCurveSelect;
		[SerializeField] private AnimationCurve animationCurveDeselect;
		[SerializeField] private float animationTimeOnSelect;
		[SerializeField] private float animationTimeOnDeselect;
		[SerializeField] private float selectionScale;

		private Coroutine animationCoroutine;
		private Vector3 animationTarget;
		private bool isSelected;

		public Transform VisObjectTransform => transform;

		public int Index { get; set; }

		public bool IsFocused { get; private set; }

		public VisObjectData CurrentData { get; private set; }

		public Collider VisCollider => visCollider;

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
		}

		public void ChangeState(VisObjectState newState) {
			switch (newState) {

				case VisObjectState.Deselected:
					if (isSelected) {
						PlayDeselectionAnimation();
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
					PlaySelectionAnimation();
					isSelected = true;
					visRenderer.sprite = selectedSprite;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
			}
		}

		public void ApplyData(params VisObjectData[] data) {
			CurrentData = data[0];
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

			if (animationCoroutine != null) {
				StopCoroutine(animationCoroutine);
			}

			Destroy(gameObject);
		}

		public void SetLatestState(bool state) { }

		public void SetNewState(bool state) { }

		private void OnAnimationFinished() {
			animationCoroutine = null;
		}

		private void PlaySelectionAnimation() {

			if (animationCoroutine != null) {
				StopCoroutine(animationCoroutine);
				visRenderer.transform.localScale = animationTarget;
			}

			animationTarget = visRenderer.transform.localScale * selectionScale;

			animationCoroutine = StartCoroutine(
				Lerper.TransformLerpOnCurve(
					visRenderer.transform,
					TransformValue.Scale,
					visRenderer.transform.localScale,
					animationTarget,
					animationTimeOnSelect,
					animationCurveSelect,
					OnAnimationFinished
				)
			);

		}

		private void PlayDeselectionAnimation() {

			if (animationCoroutine != null) {
				StopCoroutine(animationCoroutine);
				visRenderer.transform.localScale = animationTarget;
			}

			if (visRenderer == null) {
				return;
			}

			animationTarget = visRenderer.transform.localScale / selectionScale;

			animationCoroutine = StartCoroutine(
				Lerper.TransformLerpOnCurve(
					visRenderer.transform,
					TransformValue.Scale,
					visRenderer.transform.localScale,
					animationTarget,
					animationTimeOnDeselect,
					animationCurveDeselect,
					OnAnimationFinished
				)
			);

		}

	}

}
