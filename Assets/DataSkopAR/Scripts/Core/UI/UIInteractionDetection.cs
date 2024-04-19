using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR {

	public class UIInteractionDetection : MonoBehaviour {

#region Events

		[Header("Events")]
		public UnityEvent<Vector3> hasPointerDownOutsideOfUi;
		public UnityEvent<Vector3> hasPointerUpOutsideOfUi;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument blockerDoc;

		private VisualElement blockerRoot;

#endregion

#region Properties

		public static bool IsPointerOverUi { get; set; }

		public static bool HasPointerStartedOverSwipeArea { get; set; }

		public static bool HasPointerStartedOverSlider { get; set; }

#endregion

#region Methods

		private void Awake() {
			blockerRoot = blockerDoc.rootVisualElement;
			blockerRoot.RegisterCallback<PointerDownEvent>(OnPointerDown);
			blockerRoot.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}

		private void OnPointerDown(PointerDownEvent e) {
			Vector2 processedPos = new(e.position.x, blockerRoot.resolvedStyle.height - e.position.y);
			hasPointerDownOutsideOfUi?.Invoke(processedPos);
		}

		private void OnPointerUp(PointerUpEvent e) {
			Vector2 processedPos = new(e.position.x, blockerRoot.resolvedStyle.height - e.position.y);
			hasPointerUpOutsideOfUi?.Invoke(processedPos);
		}

#endregion

	}

}