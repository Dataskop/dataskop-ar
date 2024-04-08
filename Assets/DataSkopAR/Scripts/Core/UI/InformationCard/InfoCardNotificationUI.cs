using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class InfoCardNotificationUI : InfoCardComponent {

#region Fields

		[Header("Message Colors")]
		[SerializeField] private Color tipColor;
		[SerializeField] private Color warningColor;
		[SerializeField] private Color errorColor;

		[Header("Values")]
		[SerializeField] private float errorDecayTime;

		private WaitForSeconds errorDecay;

#endregion

#region Properties

		protected override VisualElement ComponentRoot { get; set; }

		private VisualElement InfoCard { get; set; }

		private Label ErrorLabel { get; set; }

#endregion

#region Methods

		public override void Init(VisualElement infoCard) {
			InfoCard = infoCard;
			ComponentRoot = InfoCard.Q<VisualElement>("ErrorDisplay");
			ErrorLabel = ComponentRoot.Q<Label>("ErrorText");
			errorDecay = new WaitForSeconds(errorDecayTime);
		}

		internal void OnErrorReceived(ErrorHandler.Error receivedError) {

			switch (receivedError.Type) {
				case ErrorHandler.ErrorType.Tip:
					ErrorLabel.style.color = new StyleColor(tipColor);
					break;
				case ErrorHandler.ErrorType.Warning:
					ErrorLabel.style.color = new StyleColor(warningColor);
					break;
				case ErrorHandler.ErrorType.Error:
					ErrorLabel.style.color = new StyleColor(errorColor);
					break;
				default:
					ErrorLabel.style.color = new StyleColor(tipColor);
					break;
			}

			ErrorLabel.text = receivedError.ToString();
			StartCoroutine(DecayError());

		}

		private IEnumerator DecayError() {
			yield return errorDecay;
			ErrorLabel.text = "";
		}

#endregion

	}

}