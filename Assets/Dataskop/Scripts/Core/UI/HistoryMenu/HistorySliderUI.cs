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
			dragger.hierarchy.Add(currentTimeLabel);
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
			
			List<VisualElement> ticks = historySlider.Query(className: "slider-tick").ToList();

			foreach (VisualElement tick in ticks) {
				tick.RemoveFromHierarchy();
			}
			
		}

		private void GenerateTicks(int dataPointsCount) {

			ClearTicks();

			float sliderTrackHeight = historySlider.resolvedStyle.height;
			int tickInterval = Mathf.CeilToInt(dataPointsCount / 20f);
			float tickSpacing = sliderTrackHeight / (dataPointsCount / (float)tickInterval);

			for (int i = 0; i <= dataPointsCount; i += tickInterval) {

				VisualElement tick = new();
				tick.AddToClassList("slider-tick");

				tick.style.width = 20;
				tick.style.height = 2;

				float tickPosition = -9 + tickSpacing * ((float)i / tickInterval);

				tick.style.top = sliderTrackHeight - tickPosition + tick.style.height.value.value / 2 -
				                 dragger.resolvedStyle.height / 2;

				tick.style.left = 50;
				historySlider.Add(tick);

			}

		}

	}

}
