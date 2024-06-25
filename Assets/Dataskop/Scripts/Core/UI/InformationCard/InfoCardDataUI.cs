using Dataskop.Data;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class InfoCardDataUI : InfoCardComponent {

		[SerializeField] private AuthorRepository authorRepository;

		protected override VisualElement ComponentRoot { get; set; }

		private VisualElement InfoCard { get; set; }

		[CanBeNull]
		private DataPoint SelectedDataPoint { get; set; }

		private Label IdLabel { get; set; }

		private Label MeasurementLabel { get; set; }

		private Label LocationLabel { get; set; }

		private Label TimeStampLabel { get; set; }

		private VisualElement AuthorIcon { get; set; }

		public override void Init(VisualElement infoCard) {
			InfoCard = infoCard;
			ComponentRoot = InfoCard.Q<VisualElement>("DataContainer");

			IdLabel = ComponentRoot.Q<Label>("IdValue");
			MeasurementLabel = ComponentRoot.Q<Label>("MeasurementResultValue");
			LocationLabel = ComponentRoot.Q<Label>("LatLonValue");
			TimeStampLabel = ComponentRoot.Q<Label>("TimeStampValue");
			AuthorIcon = ComponentRoot.Q<VisualElement>("AuthorIcon");

		}

		public void UpdateDataPointData(DataPoint dp) {

			if (SelectedDataPoint != null) {
				SelectedDataPoint.MeasurementResultChanged -= UpdateResultTextElements;
			}

			SelectedDataPoint = dp;

			if (SelectedDataPoint == null) {
				IdLabel.text = "-";
				MeasurementLabel.text = "-";
				LocationLabel.text = "-";
				TimeStampLabel.text = "-";
				AuthorIcon.style.backgroundImage = new StyleBackground();
			}
			else {
				SelectedDataPoint.MeasurementResultChanged += UpdateResultTextElements;
				UpdateDefinitionTextElements(SelectedDataPoint.MeasurementDefinition);
				UpdateResultTextElements(SelectedDataPoint.CurrentMeasurementResult);
			}

		}

		private void UpdateResultTextElements(MeasurementResult newResult) {
			MeasurementLabel.text = newResult.Value;
			LocationLabel.text = newResult.Position.GetLatLong();
			TimeStampLabel.text = newResult.GetTime();

			AuthorIcon.style.backgroundImage = !string.IsNullOrEmpty(newResult.Author)
				? new StyleBackground(authorRepository.AuthorSprites[newResult.Author])
				: new StyleBackground();

		}

		private void UpdateDefinitionTextElements(MeasurementDefinition newDefinition) {
			IdLabel.text = newDefinition.ID.ToString();
		}

	}

}