using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class InfoCardRefetchProgress : InfoCardComponent {

		protected override VisualElement ComponentRoot { get; set; }

		private ProgressBar progressBar;

		public override void Init(VisualElement infoCard) {
			ComponentRoot = infoCard;
			progressBar = ComponentRoot.Q<ProgressBar>("RefetchTimerProgressBar");
			progressBar.highValue = 1;
			progressBar.lowValue = 0;
		}

		public void OnNewValueReceived(float newValue) {
			progressBar.value = newValue;
		}

	}

}
