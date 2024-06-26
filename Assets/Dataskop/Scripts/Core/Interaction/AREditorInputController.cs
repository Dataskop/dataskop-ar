using UnityEngine;
using UnityEngine.InputSystem;

namespace Dataskop.Interaction {

	public class AREditorInputController : MonoBehaviour {

#if UNITY_EDITOR

		[SerializeField] private Transform arCamera;
		[SerializeField] private float speedModifier;
		[SerializeField] private float rotationSpeedModifier;
		[SerializeField] private CharacterController controller;

		private float xRotation;
		private float yRotation;

		private Vector3 MoveDirection { get; set; }

		private Vector2 LookDelta { get; set; }

		private void Update() {
			xRotation -= LookDelta.y * rotationSpeedModifier;
			yRotation += LookDelta.x * rotationSpeedModifier;
			xRotation = Mathf.Clamp(xRotation, -90f, 90f);
			arCamera.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
			controller.Move(arCamera.localRotation * MoveDirection * (speedModifier * Time.deltaTime));
		}

		public void MoveInput(InputAction.CallbackContext ctx) {
			Vector2 readValue = ctx.ReadValue<Vector2>();
			MoveDirection = new Vector3(readValue.x, 0, readValue.y);
		}

		public void LookInput(InputAction.CallbackContext ctx) {
			if (ctx.performed) {
				if (Mouse.current.leftButton.isPressed) {
					LookDelta = ctx.ReadValue<Vector2>();
				}
			}

			if (ctx.canceled) {
				LookDelta = Vector2.zero;
			}
		}

#endif

	}

}