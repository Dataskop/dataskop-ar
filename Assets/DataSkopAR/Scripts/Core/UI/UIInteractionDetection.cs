using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR {

	public class UIInteractionDetection : MonoBehaviour {

		public UnityEvent<Vector3> hasNotInteractedWithUI;

		public static bool IsPointerOverUi { get; set; }

		public static bool HasPointerStartedOverSwipeArea { get; set; }

		public static bool HasPointerStartedOverSlider { get; set; }

		[SerializeField] private UIDocument blockerDoc;

		private VisualElement blockerRoot;

		private void Awake() {
			blockerRoot = blockerDoc.rootVisualElement;
			blockerRoot.RegisterCallback<PointerUpEvent>(OnPointerDown);
		}

		private void OnPointerDown(PointerUpEvent e) {
			Vector2 processedPos = new(e.position.x, blockerRoot.resolvedStyle.height - e.position.y);
			hasNotInteractedWithUI?.Invoke(processedPos);
		}

	}

}