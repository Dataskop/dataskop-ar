using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DataskopAR {

	public class InputHandler : MonoBehaviour {

#region Events

		public event Action<Vector3> WorldPointerDowned;

		public event Action<Vector3> WorldPointerUpped;

#endregion

		public Vector2 TapPosition { get; private set; }

		public void TapPositionInput(InputAction.CallbackContext ctx) {
			TapPosition = ctx.ReadValue<Vector2>();
		}

		public async void OnPointerDownInWorld(Vector3 screenPosition) {

			await Task.Delay(10);
			WorldPointerDowned?.Invoke(screenPosition);

		}

		public async void OnPointerUpInWorld(Vector3 screenPosition) {

			await Task.Delay(10);
			WorldPointerUpped?.Invoke(screenPosition);

		}

	}

}