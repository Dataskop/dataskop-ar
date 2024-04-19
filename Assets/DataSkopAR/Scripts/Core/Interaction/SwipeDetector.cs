using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DataskopAR.Interaction {

	public class SwipeDetector : MonoBehaviour {

#region Fields

		public static event Action<Swipe> OnSwipe;

		[Header("References")]
		[SerializeField] private Camera arCamera;
		[SerializeField] private InputHandler inputHandler;

		[Header("Values")]
		[SerializeField] private float minSwipeDistance;

		private Swipe currentSwipe;

#endregion

#region Methods

		private void Awake() {
			inputHandler.WorldPointerDowned += OnWorldPointerDownReceived;
			inputHandler.WorldPointerUpped += OnWorldPointerUpReceived;
		}

		private void OnWorldPointerDownReceived(Vector2 screenPos) {
			currentSwipe = new Swipe();
			currentSwipe.StartPoint = screenPos;
			currentSwipe.StartingGameObject = GetTappedGameObject(currentSwipe.StartPoint);
		}

		private void OnWorldPointerUpReceived(Vector2 screenPos) {

			currentSwipe.EndPoint = screenPos;
			currentSwipe.EndingGameObject = GetTappedGameObject(currentSwipe.EndPoint);

			currentSwipe.Direction = (currentSwipe.EndPoint - currentSwipe.StartPoint).normalized;

			currentSwipe.XDistance = Mathf.Abs(currentSwipe.EndPoint.x - currentSwipe.StartPoint.x);
			currentSwipe.YDistance = Mathf.Abs(currentSwipe.EndPoint.y - currentSwipe.StartPoint.y);

			if (currentSwipe.YDistance >= minSwipeDistance || currentSwipe.XDistance >= minSwipeDistance) {
				OnSwipe?.Invoke(currentSwipe);
			}

		}

/*
		public void SwipeInput(InputAction.CallbackContext ctx) {

			if (ctx.started) {
				currentSwipe = new Swipe();
			}

			if (ctx.performed) {
				currentSwipe.StartPoint = FingerPosition;
				currentSwipe.StartingGameObject = GetTappedGameObject(currentSwipe.StartPoint);
			}

			if (ctx.canceled) {

				if (UIInteractionDetection.HasPointerStartedOverSwipeArea) {
					UIInteractionDetection.IsPointerOverUi = false;
				}

				UIInteractionDetection.HasPointerStartedOverSwipeArea = false;

			}

		}

		public void SwipePosition(InputAction.CallbackContext ctx) {
			FingerPosition = ctx.ReadValue<Vector2>();
		}
*/
		private GameObject GetTappedGameObject(Vector2 position) {
			return Physics.Raycast(arCamera.ScreenPointToRay(position), out RaycastHit hit, Mathf.Infinity)
				? hit.collider.gameObject
				: null;
		}

#if UNITY_EDITOR

		private Ray ReticuleToWorldRay => arCamera.ScreenPointToRay(new Vector3(MousePosition.x, MousePosition.y, -5));

		private static Vector3 MousePosition => Mouse.current.position.ReadValue();

		private GameObject GetRayHitObject() {

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

			Swipe lastSwipe = new() {
				StartingGameObject = GetRayHitObject(),
				EndingGameObject = GetRayHitObject(),
				//HasStartedOverSwipeAreaInUI = UIInteractionDetection.HasPointerStartedOverSwipeArea,
				EndPoint = Vector2.zero,
				StartPoint = dir,
				Direction = dir
			};

			OnSwipe?.Invoke(lastSwipe);

		}

#endif

#endregion

	}

}