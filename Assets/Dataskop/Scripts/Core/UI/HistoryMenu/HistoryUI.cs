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
		[SerializeField] private DataManager dataManager;

		private VisualElement Root { get; set; }

		private VisualElement HistoryContainer { get; set; }

		private VisualElement HistorySliderContainer { get; set; }

		private VisualElement Dragger { get; set; }

		private SliderInt HistorySlider { get; set; }

		private bool IsActive { get; set; }

		private Label CurrentTimeLabel { get; set; }

		private DataPoint SelectedDataPoint { get; set; }

		private void Start() {
			SetVisibility(Root, false);
			HistorySlider.highValue = dataManager.FetchAmount - 1;
		}

		private void OnEnable() {

			Root = historyMenuDoc.rootVisualElement;
			HistoryContainer = Root.Q<VisualElement>("HistoryContainer");
			HistorySliderContainer = HistoryContainer.Q<VisualElement>("HistorySliderContainer");

			HistorySlider = HistorySliderContainer.Q<SliderInt>("Slider");
			HistorySlider.RegisterCallback<ChangeEvent<int>>(SliderValueChanged);

			CurrentTimeLabel = HistorySliderContainer.Q<Label>("CurrentTime");
			Dragger = HistorySlider.Q<VisualElement>("unity-dragger");

		}

		private void OnDisable() {
			HistorySliderContainer.UnregisterCallback<ChangeEvent<int>>(SliderValueChanged);
		}

		private void SliderValueChanged(ChangeEvent<int> e) {
			sliderChanged?.Invoke(e.newValue, e.previousValue);
			AdjustTimeLabelPosition();
		}

		private void AdjustTimeLabelPosition() {
			CurrentTimeLabel.style.top = Dragger.localBound.yMax;
		}

		public void OnDataPointSelectionChanged(DataPoint selectedDataPoint) {

			if (SelectedDataPoint != null) {
				SelectedDataPoint.MeasurementResultChanged -= UpdateTimeLabel;
			}

			SelectedDataPoint = selectedDataPoint;

			if (SelectedDataPoint == null) {
				CurrentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
				SetVisibility(HistorySliderContainer, false);
				return;
			}

			SelectedDataPoint.MeasurementResultChanged += UpdateTimeLabel;
			UpdateTimeLabel(SelectedDataPoint.FocusedMeasurement);

			if (IsActive) {
				int resultsCount = selectedDataPoint.MeasurementDefinition.MeasurementResults.Count;
				HistorySlider.highValue = resultsCount - 1;
				GenerateTicks(resultsCount);
				SetVisibility(HistorySliderContainer, true);
				CurrentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
			}

		}

		private void UpdateTimeLabel(MeasurementResult currentDataPointMeasurementResult) {
			CurrentTimeLabel.text =
				$"{currentDataPointMeasurementResult.GetDate()}<br>{currentDataPointMeasurementResult.GetClockTime()}";
		}

		public void OnDataPointHistorySwiped(int newCount) {
			HistorySlider.SetValueWithoutNotify(newCount);
		}

		public void OnVisualizationOptionChanged(VisualizationOption currentVisOption) {

			HistorySlider.value = 0;

			if (IsActive) {

				if (currentVisOption.Style.IsTimeSeries) {
					StartCoroutine(DelayToggle());
					return;
				}

				SetVisibility(Root, currentVisOption.Style.IsTimeSeries);
				SetVisibility(HistorySliderContainer, false);
				IsActive = false;

			}
			else {
				SetVisibility(Root, currentVisOption.Style.IsTimeSeries);
				SetVisibility(HistorySliderContainer, false);
				IsActive = false;
			}
		}

		private static void SetVisibility(VisualElement element, bool isVisible) {
			element.style.visibility = new StyleEnum<Visibility>(isVisible ? Visibility.Visible : Visibility.Hidden);
		}

		public void ToggleHistoryView() {

			IsActive = !IsActive;

			if (SelectedDataPoint) {
				SetVisibility(HistorySliderContainer, IsActive);
				SetVisibility(CurrentTimeLabel, IsActive);
			}

			HistorySlider.value = 0;
			CurrentTimeLabel.style.top = Dragger.localBound.yMax;
			historyViewToggled?.Invoke(IsActive);

		}

		public void OnDataPointsResultsUpdated() { }

		private IEnumerator DelayToggle() {
			yield return new WaitForSeconds(0.015f);
			historyViewToggled?.Invoke(IsActive);
		}

		private void GenerateTicks(int dataPointsCount) {
			// Clear existing ticks
			ClearTicks();

			// Get the total height of the slider track where ticks will be placed
			float sliderTrackHeight = HistorySliderContainer.resolvedStyle.height;

			// Determine the interval for displaying ticks to avoid clutter for large data points
			int tickInterval = Mathf.CeilToInt(dataPointsCount / 20f);

			// Calculate the space between ticks
			float tickSpacing = sliderTrackHeight / (dataPointsCount / (float)tickInterval);

			// Generate ticks
			for (int i = 0; i < dataPointsCount; i += tickInterval) {
				VisualElement tick = new();
				tick.AddToClassList("slider-tick");

				// Set the size of the tick
				tick.style.width = 20; // The width of the tick mark, stretching out from the slider
				tick.style.height = 6; // The height of the tick mark

				// Calculate the vertical position of the tick
				float tickPosition = tickSpacing * (i / tickInterval);

				// The position is calculated from the bottom (sliderTrackHeight - position - half height of tick)
				// to correctly align with the vertical slider's orientation
				tick.style.top = sliderTrackHeight - tickPosition - tick.resolvedStyle.height / 2;
				// Add the tick to the slider container
				HistorySliderContainer.Add(tick);
			}
		}

		private void ClearTicks() {
			// Get all tick elements and remove them
			List<VisualElement> ticks = HistorySliderContainer.Query(className: "slider-tick").ToList();
			foreach (VisualElement tick in ticks) {
				tick.RemoveFromHierarchy();
			}
		}

	}

}