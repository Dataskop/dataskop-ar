using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class WorldInteractionDetector : MonoBehaviour {

#region Events

		[Header("Events")]
		public UnityEvent<Vector2> hasPointerDownOutsideOfUi;
		public UnityEvent<Vector2> hasPointerUpOutsideOfUi;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument blockerDoc;

		private VisualElement blockerRoot;

#endregion

#region Methods

		private void Awake() {
			blockerRoot = blockerDoc.rootVisualElement;
			blockerRoot.RegisterCallback<PointerDownEvent>(OnPointerDown);
			blockerRoot.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}

		private void OnPointerDown(PointerDownEvent e) {
			Vector2 processedPos = new(e.position.x, Screen.height - e.position.y);
			hasPointerDownOutsideOfUi?.Invoke(processedPos);
		}

		private void OnPointerUp(PointerUpEvent e) {
			Vector2 processedPos = new(e.position.x, Screen.height - e.position.y);
			hasPointerUpOutsideOfUi?.Invoke(processedPos);
		}

#endregion

	}

}