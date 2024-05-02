using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

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

			ErrorLabel.style.color = receivedError.Type switch {
				ErrorHandler.ErrorType.Tip => new StyleColor(tipColor),
				ErrorHandler.ErrorType.Warning => new StyleColor(warningColor),
				ErrorHandler.ErrorType.Error => new StyleColor(errorColor),
				_ => new StyleColor(tipColor)
			};

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