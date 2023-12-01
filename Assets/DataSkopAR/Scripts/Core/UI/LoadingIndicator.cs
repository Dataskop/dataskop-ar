using UnityEngine;
using UnityEngine.UIElements;

namespace DataSkopAR.UI {

	public class LoadingIndicator : MonoBehaviour {

#region Properties

		public bool IsLoading { get; set; }

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument loadingIndicatorUiDocument;

		[Header("Values")]
		[SerializeField] private float rotationSpeed = 350f;

		private VisualElement indicator;
		private float rotation;

#endregion

#region Methods

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
			loadingIndicatorUiDocument.rootVisualElement.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
		}

		public void Hide() {
			if (indicator == null) return;

			IsLoading = false;
			loadingIndicatorUiDocument.rootVisualElement.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
		}

#endregion

	}

}