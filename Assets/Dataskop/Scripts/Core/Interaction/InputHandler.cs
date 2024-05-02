#nullable enable

using System;
using System.Threading.Tasks;
using Dataskop.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dataskop.Interaction {

	public class InputHandler : MonoBehaviour {

#region Events

		public event Action<PointerInteraction>? WorldPointerDowned;

		public event Action<PointerInteraction>? WorldPointerUpped;

		public event Action<PointerInteraction>? InfoCardPointerDowned;

		public event Action<PointerInteraction>? InfoCardPointerUpped;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private Camera mainCamera = null!;
		[SerializeField] private float minimumSwipeDistance = 100f;

		private bool isInteracting;

#endregion

#region Properties

		private Vector2 TapPosition { get; set; }

		private PointerInteraction CurrentPointerInteraction { get; set; }

		private Ray PointerRay { get; set; }

#endregion

#region Methods

		public void TapPositionInput(InputAction.CallbackContext ctx) {
			TapPosition = ctx.ReadValue<Vector2>();
		}

		public async void OnPointerDownInWorld(WorldPointerEventArgs e) {
			await Task.Delay(10);

			if (isInteracting) return;

			isInteracting = true;

			PointerInteraction newPointerInteraction = new() {
				isDownPhase = true,
				startPosition = e.screenPosition,
				startingGameObject = TryGetPointerGameObject(TapPosition),
				isUI = false
			};

			CurrentPointerInteraction = newPointerInteraction;

			WorldPointerDowned?.Invoke(CurrentPointerInteraction);
		}

		public async void OnPointerUpInWorld(WorldPointerEventArgs e) {
			await Task.Delay(10);

			if (!CurrentPointerInteraction.isDownPhase) return;

			PointerInteraction currentPointerInteraction = CurrentPointerInteraction;

			currentPointerInteraction.isUpPhase = true;
			currentPointerInteraction.endPosition = e.screenPosition;
			currentPointerInteraction.endingGameObject = TryGetPointerGameObject(TapPosition);
			currentPointerInteraction.isSwipe = currentPointerInteraction.Distance > minimumSwipeDistance;

			CurrentPointerInteraction = currentPointerInteraction;

			if (CurrentPointerInteraction.isUI) {

				if (CurrentPointerInteraction.uiStartSection == UISection.InfoCard) {
					InfoCardPointerUpped?.Invoke(CurrentPointerInteraction);
				}

				isInteracting = false;
				return;

			}

			WorldPointerUpped?.Invoke(CurrentPointerInteraction);
			isInteracting = false;

		}

		public void OnPointerDownOnUI(UIPointerEventArgs e) {

			if (isInteracting) return;

			isInteracting = true;

			PointerInteraction newPointerInteraction = new() {
				isDownPhase = true,
				startPosition = TapPosition,
				startingGameObject = null,
				isUI = true,
				uiStartSection = e.uiPointerSection,
				pointerId = e.pointerId
			};

			CurrentPointerInteraction = newPointerInteraction;

			if (newPointerInteraction.uiStartSection == UISection.InfoCard) {
				InfoCardPointerDowned?.Invoke(CurrentPointerInteraction);
			}

		}

		public void OnPointerUpOnUI(UIPointerEventArgs e) {

			if (CurrentPointerInteraction.pointerId != e.pointerId) return;

			if (!CurrentPointerInteraction.isDownPhase) return;

			PointerInteraction currentPointerInteraction = CurrentPointerInteraction;

			currentPointerInteraction.isUpPhase = true;
			currentPointerInteraction.endPosition = TapPosition;
			currentPointerInteraction.endingGameObject = null;
			currentPointerInteraction.isSwipe = currentPointerInteraction.Distance > minimumSwipeDistance;
			currentPointerInteraction.uiEndSection = e.uiPointerSection;

			CurrentPointerInteraction = currentPointerInteraction;

			if (!CurrentPointerInteraction.isUI) {
				WorldPointerUpped?.Invoke(CurrentPointerInteraction);
				isInteracting = false;
				return;
			}

			if (currentPointerInteraction.uiStartSection == UISection.InfoCard) {
				InfoCardPointerUpped?.Invoke(CurrentPointerInteraction);
			}

			isInteracting = false;

		}

		private GameObject? TryGetPointerGameObject(Vector2 position) {

			PointerRay = mainCamera.ScreenPointToRay(position);

			return Physics.Raycast(PointerRay, out RaycastHit hit, Mathf.Infinity)
				? hit.collider.gameObject
				: null;

		}

#if UNITY_EDITOR

		private Ray ReticuleToWorldRay => mainCamera.ScreenPointToRay(new Vector3(MousePosition.x, MousePosition.y, -5));

		private static Vector3 MousePosition => Mouse.current.position.ReadValue();

		private GameObject? GetRayHitObject() {

			if (Physics.Raycast(ReticuleToWorldRay, out RaycastHit hit, Mathf.Infinity)) {
				Debug.DrawRay(ReticuleToWorldRay.origin, ReticuleToWorldRay.direction * 50f, Color.red, 20f);
				return hit.collider.gameObject;
			}

			return null;

		}

		public void MouseScrolledInput(InputAction.CallbackContext ctx) {

			if (ctx.started) {
				EditorSwipe(ctx.ReadValue<Vector2>().normalized);
			}

		}

		private void EditorSwipe(Vector2 dir) {
			/*
			PointerInteraction lastPointerInteraction = new() {
				startingGameObject = GetRayHitObject(),
				endingGameObject = GetRayHitObject(),
				endPosition = Vector2.zero,
				startPosition = dir
			};
			*/

			//OnSwipe?.Invoke(lastSwipe);

		}

#endif

#endregion

	}

}