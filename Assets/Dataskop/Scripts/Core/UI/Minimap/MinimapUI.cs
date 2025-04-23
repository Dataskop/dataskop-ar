using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Dataskop {

	public class MinimapUI : MonoBehaviour {

		[Header("Events")]
		public UnityEvent minimapTapped;

		[Header("References")]
		[SerializeField] private UIDocument minimapDocument;

		private VisualElement MinimapRoot { get; set; }

		private void Awake() {
			MinimapRoot = minimapDocument.rootVisualElement;
			MinimapRoot.RegisterCallback<ClickEvent>(_ => minimapTapped?.Invoke());
		}

		private void OnDisable() {
			MinimapRoot.UnregisterCallback<ClickEvent>(_ => minimapTapped?.Invoke());
		}

		public void OnCalibrationFinished() {
			MinimapRoot.visible = true;
		}

		public void OnCalibrationInitialized() {
			MinimapRoot.visible = false;
		}

		public void ToggleMinimap() {
			MinimapRoot.visible = !MinimapRoot.visible;
		}

	}

}