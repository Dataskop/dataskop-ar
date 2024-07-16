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
				SelectedDataPoint.MeasurementResultChanged -= UpdateResultTextElements;
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
				SelectedDataPoint.MeasurementResultChanged += UpdateResultTextElements;
				UpdateResultTextElements(SelectedDataPoint.FocusedMeasurement);
			}

		}

		private void UpdateResultTextElements(MeasurementResult newResult) {
			IdLabel.text = newResult.MeasurementDefinition.ID.ToString();
			MeasurementLabel.text =
				$"{newResult.ReadAsFloat().ToString("00.00", CultureInfo.InvariantCulture)} {SelectedDataPoint!.Attribute.Unit}";
			LocationLabel.text = newResult.Position.GetLatLong();
			TimeStampLabel.text = newResult.GetTime();
			MeasurementDefinitionLabel.text = newResult.MeasurementDefinition.MeasurementDefinitionInformation.Name;
			DeviceLabel.text = newResult.MeasurementDefinition.DeviceId;
			TotalMeasurementsLabel.text = newResult.MeasurementDefinition.TotalMeasurements.ToString();
			FirstMeasurementLabel.text = newResult.MeasurementDefinition.FirstMeasurementResult.GetTime();
			MeasurementIntervalLabel.text = $"{newResult.MeasurementDefinition.MeasuringInterval / 10}s";

			AuthorIcon.style.backgroundImage = !string.IsNullOrEmpty(newResult.Author)
				? new StyleBackground(authorRepository.AuthorSprites[newResult.Author])
				: new StyleBackground();

		}

	}

}