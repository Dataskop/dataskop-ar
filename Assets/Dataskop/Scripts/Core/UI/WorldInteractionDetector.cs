using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class WorldInteractionDetector : MonoBehaviour {

#region Events

		[Header("Events")]
		public UnityEvent<WorldPointerEventArgs> hasPointerDownedInWorld;
		public UnityEvent<WorldPointerEventArgs> hasPointerUppedInWorld;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument detectorDocument;

		private VisualElement detectorRoot;

#endregion

#region Methods

		private void Awake() {
			detectorRoot = detectorDocument.rootVisualElement;
			detectorRoot.RegisterCallback<PointerDownEvent>(OnPointerDown);
			detectorRoot.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}

		private void OnPointerDown(PointerDownEvent e) {
			Vector2 processedPos = new(e.position.x, Screen.height - e.position.y);
			hasPointerDownedInWorld?.Invoke(new WorldPointerEventArgs(processedPos, e.pointerId));
		}

		private void OnPointerUp(PointerUpEvent e) {
			Vector2 processedPos = new(e.position.x, Screen.height - e.position.y);
			hasPointerUppedInWorld?.Invoke(new WorldPointerEventArgs(processedPos, e.pointerId));
		}

#endregion

	}

}