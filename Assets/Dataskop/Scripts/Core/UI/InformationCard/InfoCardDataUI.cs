using System.Globalization;
using Dataskop.Data;
using Dataskop.Entities;
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

		private Label MeasurementDefinitionLabel { get; set; }

		private Label DeviceLabel { get; set; }

		private Label TotalMeasurementsLabel { get; set; }

		private Label FirstMeasurementLabel { get; set; }

		private Label MeasurementIntervalLabel { get; set; }

		private VisualElement AuthorIcon { get; set; }

		public override void Init(VisualElement infoCard) {
			InfoCard = infoCard;
			ComponentRoot = InfoCard.Q<VisualElement>("DataContainer");

			IdLabel = ComponentRoot.Q<Label>("IdValue");
			MeasurementLabel = ComponentRoot.Q<Label>("MeasurementResultValue");
			LocationLabel = ComponentRoot.Q<Label>("LatLonValue");
			TimeStampLabel = ComponentRoot.Q<Label>("TimeStampValue");
			MeasurementDefinitionLabel = ComponentRoot.Q<Label>("MeasurementDefinitionValue");
			DeviceLabel = ComponentRoot.Q<Label>("DeviceValue");
			TotalMeasurementsLabel = ComponentRoot.Q<Label>("TotalMeasurementsValue");
			FirstMeasurementLabel = ComponentRoot.Q<Label>("FirstMeasurementValue");
			MeasurementIntervalLabel = ComponentRoot.Q<Label>("MeasurementIntervalValue");
			AuthorIcon = ComponentRoot.Q<VisualElement>("AuthorIcon");

		}

		public void UpdateDataPointData(DataPoint dp) {

			if (SelectedDataPoint != null) {
				SelectedDataPoint.FocusedIndexChanged -= UpdateIndexTextElements;
			}

			SelectedDataPoint = dp;

			if (SelectedDataPoint == null) {
				IdLabel.text = "-";
				MeasurementLabel.text = "-";
				LocationLabel.text = "-";
				TimeStampLabel.text = "-";
				MeasurementDefinitionLabel.text = "-";
				DeviceLabel.text = "-";
				TotalMeasurementsLabel.text = "-";
				FirstMeasurementLabel.text = "-";
				MeasurementIntervalLabel.text = "-";
				AuthorIcon.style.backgroundImage = new StyleBackground();
			}
			else {
				SelectedDataPoint.FocusedIndexChanged += UpdateIndexTextElements;
				UpdateIndexTextElements(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.FocusedIndex);
			}

		}

		private void UpdateIndexTextElements(MeasurementDefinition def, int index) {

			MeasurementResult focusedResult = def.GetMeasurementResult(index);

			IdLabel.text = focusedResult.MeasurementDefinition.ID.ToString();
			MeasurementLabel.text =
				$"{focusedResult.ReadAsFloat().ToString("00.00", CultureInfo.InvariantCulture)} {SelectedDataPoint!.Attribute.Unit}";
			LocationLabel.text = focusedResult.Position.GetLatLong();
			TimeStampLabel.text = focusedResult.GetDate();
			MeasurementDefinitionLabel.text = focusedResult.MeasurementDefinition.MeasurementDefinitionInformation.Name;
			DeviceLabel.text = focusedResult.MeasurementDefinition.DeviceId;
			TotalMeasurementsLabel.text = focusedResult.MeasurementDefinition.TotalMeasurements.ToString();
			FirstMeasurementLabel.text = focusedResult.MeasurementDefinition.FirstMeasurementResult.GetDate();
			MeasurementIntervalLabel.text = $"{focusedResult.MeasurementDefinition.MeasuringInterval}s";

			AuthorIcon.style.backgroundImage = !string.IsNullOrEmpty(focusedResult.Author)
				? new StyleBackground(authorRepository.AuthorSprites[focusedResult.Author])
				: new StyleBackground();

		}

	}

}