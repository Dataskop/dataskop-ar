using System.Collections;
using System.Collections.Generic;
using Dataskop.Data;
using Dataskop.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class HistoryUI : MonoBehaviour {

		[Header("Events")]
		public UnityEvent<int, int> sliderChanged;
		public UnityEvent<bool> historyViewToggled;

		[Header("References")]
		[SerializeField] private UIDocument historyMenuDoc;

		private VisualElement Root { get; set; }

		private VisualElement HistoryContainer { get; set; }

		private VisualElement Dragger { get; set; }

		private SliderInt HistorySlider { get; set; }

		private bool IsActive { get; set; }

		private Label CurrentTimeLabel { get; set; }

		private DataPoint SelectedDataPoint { get; set; }

		private void Start() {
			SetVisibility(HistoryContainer, false);
		}

		private void OnEnable() {

			Root = historyMenuDoc.rootVisualElement;
			HistoryContainer = Root.Q<VisualElement>("HistoryContainer");

			HistorySlider = HistoryContainer.Q<SliderInt>("Slider");
			HistorySlider.RegisterCallback<ChangeEvent<int>>(SliderValueChanged);

			CurrentTimeLabel = HistoryContainer.Q<Label>("CurrentTime");
			Dragger = HistorySlider.Q<VisualElement>("unity-dragger");
			Dragger.RegisterCallback<GeometryChangedEvent>(_ => AdjustTimeLabelPosition());

		}

		private void OnDisable() {
			HistorySlider.UnregisterCallback<ChangeEvent<int>>(SliderValueChanged);
		}

		private void SliderValueChanged(ChangeEvent<int> e) {

			if (!IsActive) {
				return;
			}

			sliderChanged?.Invoke(e.newValue, e.previousValue);
			AdjustTimeLabelPosition();
		}

		public void OnDataPointSelectionChanged(DataPoint selectedDataPoint) {

			if (SelectedDataPoint != null) {
				SelectedDataPoint.FocusedIndexChanged -= UpdateTimeLabel;
			}

			SelectedDataPoint = selectedDataPoint;

			if (SelectedDataPoint == null) {
				CurrentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
				SetVisibility(HistoryContainer, false);
				return;
			}

			SelectedDataPoint.FocusedIndexChanged += UpdateTimeLabel;

			if (!IsActive) {
				return;
			}

			SetVisibility(HistoryContainer, true);

			int newResultsCount = GetMeasurementCount();

			HistorySlider.highValue = newResultsCount - 1;
			HistorySlider.SetValueWithoutNotify(SelectedDataPoint.FocusedIndex);

			StartCoroutine(GenerateTicks(newResultsCount));
			CurrentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
			UpdateTimeLabel(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.FocusedIndex);
		}

		private int GetMeasurementCount() {

			if (SelectedDataPoint == null) {
				return 0;
			}

			return SelectedDataPoint.MeasurementDefinition.MeasurementResults.Count <
			       SelectedDataPoint.Vis.VisHistoryConfiguration.visibleHistoryCount
				? SelectedDataPoint.MeasurementDefinition.MeasurementResults.Count
				: SelectedDataPoint.Vis.VisHistoryConfiguration.visibleHistoryCount;
		}

		private void UpdateTimeLabel(MeasurementDefinition def, int index) {
			MeasurementResult focusedResult = def.MeasurementResults[index];
			CurrentTimeLabel.text = $"{focusedResult.GetDate()}";
		}

		public void OnDataPointHistorySwiped(int newCount) {
			HistorySlider.SetValueWithoutNotify(newCount);
			AdjustTimeLabelPosition();
		}

		public void OnVisualizationOptionChanged(VisualizationOption currentVisOption) {

			if (IsActive) {

				if (currentVisOption.Style.IsTimeSeries) {
					StartCoroutine(DelayToggle());
					return;
				}

				SetVisibility(Root, currentVisOption.Style.IsTimeSeries);
				SetVisibility(HistoryContainer, false);
				IsActive = false;

			}
			else {
				SetVisibility(Root, currentVisOption.Style.IsTimeSeries);
				SetVisibility(HistoryContainer, false);
				IsActive = false;
			}

		}

		private void AdjustTimeLabelPosition() {
			CurrentTimeLabel.style.top = Dragger.localBound.yMax - Dragger.resolvedStyle.height;
		}

		private void SetVisibility(VisualElement element, bool isVisible) {
			element.style.display = new StyleEnum<DisplayStyle>(isVisible ? DisplayStyle.Flex : DisplayStyle.None);
		}

		public void ToggleHistoryView() {

			IsActive = !IsActive;

			if (SelectedDataPoint) {
				SetVisibility(HistoryContainer, IsActive);
				SetVisibility(CurrentTimeLabel, IsActive);
				StartCoroutine(GenerateTicks(GetMeasurementCount()));
			}

			historyViewToggled?.Invoke(IsActive);

		}

		private IEnumerator DelayToggle() {
			yield return new WaitForEndOfFrame();
			historyViewToggled?.Invoke(IsActive);
		}

		private IEnumerator GenerateTicks(int dataPointsCount) {
			// Clear existing ticks
			ClearTicks();

			yield return new WaitForEndOfFrame();

			// Get the total height of the slider track where ticks will be placed
			float sliderTrackHeight = HistorySlider.resolvedStyle.height;

			// Determine the interval for displaying ticks to avoid clutter for large data points
			int tickInterval = Mathf.CeilToInt(dataPointsCount / 20f);

			// Calculate the space between ticks
			float tickSpacing = sliderTrackHeight / (dataPointsCount / (float)tickInterval);

			// Generate ticks
			for (int i = 0; i <= dataPointsCount; i += tickInterval) {

				VisualElement tick = new();
				tick.AddToClassList("slider-tick");

				// Set the size of the tick
				tick.style.width = 20; // The width of the tick mark, stretching out from the slider
				tick.style.height = 2; // The height of the tick mark

				// Calculate the vertical position of the tick
				float tickPosition = tickSpacing * ((float)i / tickInterval);

				// The position is calculated from the bottom (sliderTrackHeight - position - half height of tick)
				// to correctly align with the vertical slider's orientation
				tick.style.top = sliderTrackHeight - tickPosition + tick.style.height.value.value / 2 - (Dragger.resolvedStyle.height / 2);
				tick.style.left = 50;

				// Add the tick to the slider container
				HistorySlider.Add(tick);

			}

		}

		private void ClearTicks() {
			// Get all tick elements and remove them
			List<VisualElement> ticks = HistorySlider.Query(className: "slider-tick").ToList();
			foreach (VisualElement tick in ticks) {
				tick.RemoveFromHierarchy();
			}
		}

	}

}