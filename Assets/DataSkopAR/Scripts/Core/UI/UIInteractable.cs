using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class UIInteractable : MonoBehaviour {

		[Header("Events")]
		public UnityEvent<Vector2, UISection> hasPointerDowned;
		public UnityEvent<Vector2, UISection> hasPointerUpped;

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
			hasPointerDowned?.Invoke(e.position, section);
		}

		private void OnPointerUp(PointerUpEvent e) {
			hasPointerUpped?.Invoke(e.position, section);
		}

	}

}