using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class UIInteractable : MonoBehaviour {

		[Header("Events")]
		public UnityEvent<UIPointerEventArgs> hasPointerDowned;
		public UnityEvent<UIPointerEventArgs> hasPointerUpped;

		[Header("Values")]
		public UISection section;

		private UIDocument doc;
		private VisualElement root;

		private void Awake() {
			doc = GetComponent<UIDocument>();
			root = doc.rootVisualElement;
			root.RegisterCallback<PointerDownEvent>(OnPointerDown);
			root.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}

		private void OnPointerDown(PointerDownEvent e) {
			hasPointerDowned?.Invoke(new UIPointerEventArgs(e.position, section, e.pointerId));
		}

		private void OnPointerUp(PointerUpEvent e) {
			hasPointerUpped?.Invoke(new UIPointerEventArgs(e.position, section, e.pointerId));
		}

	}

}