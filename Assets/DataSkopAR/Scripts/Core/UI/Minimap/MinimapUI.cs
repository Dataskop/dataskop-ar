using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR {

	public class MinimapUI : MonoBehaviour {

#region Events

		[Header("Events")]
		public UnityEvent minimapTapped;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument minimapDocument;

#endregion

#region Properties

		private VisualElement MinimapRoot { get; set; }

#endregion

#region Methods

		private void Awake() {
			MinimapRoot = minimapDocument.rootVisualElement;
			MinimapRoot.RegisterCallback<ClickEvent>(_ => minimapTapped?.Invoke());
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

		private void OnDisable() {
			MinimapRoot.UnregisterCallback<ClickEvent>(_ => minimapTapped?.Invoke());
		}

#endregion

	}

}