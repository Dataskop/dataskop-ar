using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class HistorySliderUI {

		private readonly Label currentTimeLabel;
		private readonly VisualElement dragger;

		private readonly VisualElement historyContainer;
		private readonly SliderInt historySlider;

		private string currentAttributeId;
		private string currentDeviceId;

		public HistorySliderUI(VisualElement container) {
			historyContainer = container;
			historySlider = historyContainer.Q<SliderInt>("Slider");
			historySlider.RegisterCallback<ChangeEvent<int>>(SliderChanged);
			currentTimeLabel = historyContainer.Q<Label>("CurrentTime");
			dragger = historySlider.Q<VisualElement>("unity-dragger");
		}

		public event Action<int, int> SliderValueChanged;

		public void Show() {
			historyContainer.visible = true;
		}

		public void Hide() {
			historyContainer.visible = false;
		}

		public void SetValue(int val) {
			historySlider.SetValueWithoutNotify(val);
		}

		public void UpdateTicks(int count) {
			GenerateTicks(count);
		}

		public void UpdateTimeLabel(string text) {
			currentTimeLabel.text = text;
		}

		public void SetLimits(int low, int high) {
			historySlider.lowValue = low;
			historySlider.highValue = high;
		}

		public void ClearData() {
			ClearTicks();
			currentTimeLabel.text = "";
		}

		private void SliderChanged(ChangeEvent<int> e) {
			SliderValueChanged?.Invoke(e.newValue, e.previousValue);
		}

		private void ClearTicks() {
			// Get all tick elements and remove them
			List<VisualElement> ticks = historySlider.Query(className: "slider-tick").ToList();
			foreach (VisualElement tick in ticks) {
				tick.RemoveFromHierarchy();
			}
		}

		private void GenerateTicks(int dataPointsCount) {
			// Clear existing ticks
			ClearTicks();

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