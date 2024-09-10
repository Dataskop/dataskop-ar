using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class LoadingIndicator : MonoBehaviour {

		[Header("References")]
		[SerializeField] private UIDocument loadingIndicatorUiDocument;

		[Header("Values")]
		[SerializeField] private float rotationSpeed = 350f;

		private VisualElement indicator;
		private float rotation;

		public bool IsLoading { get; set; }

		private void Awake() {
			indicator = loadingIndicatorUiDocument.rootVisualElement.Q<VisualElement>("spinner");
			Hide();
		}

		private void FixedUpdate() {
			rotation += rotationSpeed * Time.fixedDeltaTime;

			if (rotation >= 360) rotation = 0;

			indicator.style.rotate = new StyleRotate(new Rotate(new Angle(rotation)));
		}

		public void Show() {
			if (indicator == null) return;

			IsLoading = true;
			if (loadingIndicatorUiDocument != null) {
				loadingIndicatorUiDocument.rootVisualElement.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
				Debug.Log("LOADING STARTED!");
			}
		}

		public void Hide() {
			if (indicator == null) return;

			IsLoading = false;

			if (loadingIndicatorUiDocument != null) {
				loadingIndicatorUiDocument.rootVisualElement.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
				Debug.Log("LOADING FINISHED!");
			}
		}

	}

}