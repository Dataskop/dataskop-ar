using Mapbox.Utils;
using TMPro;
using UnityEngine;

namespace Dataskop {

	/// <summary>
	///     Used for printing out simple debugging messages and values in a log window on the screen.
	/// </summary>
	public class LogBook : MonoBehaviour {

		[SerializeField] private TMP_Text debugMesh;

		public static LogBook Instance { get; private set; }

		private void Awake() {

			if (Instance != null && Instance != this)
				Destroy(this);
			else
				Instance = this;

		}

		public void Log(string text) {
			debugMesh.text += $"{text}\n";
		}

		public void Log(string text, float value) {
			debugMesh.text += $"{text} {value}\n";
		}

		public void Log(string text, Vector3 vector) {
			debugMesh.text += $"{text} {vector}\n";
		}

		public void Log(Vector3 vector) {
			debugMesh.text += $"{vector}\n";
		}

		public void Log(float value) {
			debugMesh.text += $"{value}\n";
		}

		public void Log(int value) {
			debugMesh.text += $"{value}\n";
		}

		public void Log(Vector2d value) {
			debugMesh.text += $"{value.x}, {value.y}\n";
		}

		public void Clear() {
			debugMesh.SetText("");
		}

	}

}