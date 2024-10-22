using System;
using System.Collections;
using System.Collections.Generic;
using Dataskop.Data;
using Dataskop.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class HistorySliderUI : MonoBehaviour {

		public Action<int, int> sliderValueChanged;

		private VisualElement historyContainer;
		private VisualElement dragger;
		private SliderInt historySlider;
		private Label currentTimeLabel;
		private string currentAttributeId;
		private string currentDeviceId;

		private bool IsActive { get; set; }

		/// <summary>
		/// Initializes the UI Element by finding its elements based on the given root element.
		/// </summary>
		/// <param name="container">The root element.</param>
		public void Init(VisualElement container) {
			historyContainer = container;
			historySlider = historyContainer.Q<SliderInt>("Slider");
			historySlider.RegisterCallback<ChangeEvent<int>>(SliderValueChanged);
			currentTimeLabel = historyContainer.Q<Label>("CurrentTime");
			dragger = historySlider.Q<VisualElement>("unity-dragger");
		}

		public void Show() {
			historyContainer.visible = true;
			StartCoroutine(GenerateTicks(GetMeasurementCount()));
		}

		public void Hide() {
			historyContainer.visible = false;
		}

		private void SliderValueChanged(ChangeEvent<int> e) {
			sliderValueChanged?.Invoke(e.newValue, e.previousValue);
		}

		public void SetValue(int val) {
			historySlider.SetValueWithoutNotify(val);
		}

		public void OnDataPointSelected(DataPoint selectedDataPoint) { }

		public void OnDataPointDeselected() { }

		public void OnDataPointSelectionChanged(DataPoint selectedDataPoint) {

			if (SelectedDataPoint != null) {
				SelectedDataPoint.FocusedMeasurementResultChanged -= UpdateTimeLabel;
				SelectedDataPoint.MeasurementRangeChanged -= OnMeasurementRangeChanged;
			}

			SelectedDataPoint = selectedDataPoint;

			if (SelectedDataPoint == null) {
				currentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
				SetVisibility(historyContainer, false);
				SetVisibility(RangeContainer, false);
				return;
			}

			SelectedDataPoint.FocusedMeasurementResultChanged += UpdateTimeLabel;
			SelectedDataPoint.MeasurementRangeChanged += OnMeasurementRangeChanged;

			if (!IsActive) {
				return;
			}

			SetVisibility(historyContainer, true);
			SetVisibility(RangeContainer, true);

			int newResultsCount = GetMeasurementCount();

			historySlider.highValue = newResultsCount - 1;
			historySlider.SetValueWithoutNotify(SelectedDataPoint.FocusedIndex);

			StartCoroutine(GenerateTicks(newResultsCount));
			currentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
			UpdateTimeLabel(SelectedDataPoint.FocusedMeasurement);

			// check if we are still on the same device before updating time range
			if (SelectedDataPoint == null) {
				return;
			}

			UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
			CreateCacheRect(SelectedDataPoint.MeasurementDefinition);

		}

		private void UpdateTimeLabel(MeasurementResult focusedResult) {
			currentTimeLabel.text = focusedResult.GetDate();
		}

		private void SetVisibility(VisualElement element, bool isVisible) {
			element.style.display = new StyleEnum<DisplayStyle>(isVisible ? DisplayStyle.Flex : DisplayStyle.None);
		}

		public void SetHistoryViewState(bool newState) {

			IsActive = newState;

			if (SelectedDataPoint) {
				SetVisibility(historyContainer, IsActive);
				SetVisibility(currentTimeLabel, IsActive);
				SetVisibility(rangeContainer, IsActive);
				StartCoroutine(GenerateTicks(GetMeasurementCount()));
			}

			historyViewToggled?.Invoke(IsActive);

		}

		private void OnMeasurementRangeChanged() {
			CreateCacheRect(SelectedDataPoint.MeasurementDefinition);
			UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
		}

		private void ClearTicks() {
			// Get all tick elements and remove them
			List<VisualElement> ticks = historySlider.Query(className: "slider-tick").ToList();
			foreach (VisualElement tick in ticks) {
				tick.RemoveFromHierarchy();
			}
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
			float sliderTrackHeight = historySlider.resolvedStyle.height;

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
				tick.style.top = sliderTrackHeight - tickPosition + tick.style.height.value.value / 2 - dragger.resolvedStyle.height / 2;
				tick.style.left = 50;

				// Add the tick to the slider container
				historySlider.Add(tick);

			}

		}

	}

}