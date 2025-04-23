using Dataskop.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Dataskop {

	public class InfoCardMap : InfoCardComponent {

		[Header("Events")]
		public UnityEvent<float> zoomButtonPressed;

		[Header("Values")]
		[Range(-20, 0)] [SerializeField]
		private float zoomInValue;

		[Range(0, 20)] [SerializeField]
		private float zoomOutValue;

		protected override VisualElement ComponentRoot { get; set; }

		private VisualElement InfoCard { get; set; }

		private Button ZoomInButton { get; set; }

		private Button ZoomOutButton { get; set; }

		public override void Init(VisualElement infoCard) {

			InfoCard = infoCard;
			ComponentRoot = InfoCard.Q<VisualElement>("MapContainer");
			ZoomInButton = ComponentRoot.Q<Button>("ZoomInButton");
			ZoomInButton.RegisterCallback<ClickEvent>(_ => OnZoomButtonPressed(zoomInValue));
			ZoomOutButton = ComponentRoot.Q<Button>("ZoomOutButton");
			ZoomOutButton.RegisterCallback<ClickEvent>(_ => OnZoomButtonPressed(zoomOutValue));

		}

		private void OnZoomButtonPressed(float zoomChange) {
			zoomButtonPressed?.Invoke(zoomChange);
		}

	}

}