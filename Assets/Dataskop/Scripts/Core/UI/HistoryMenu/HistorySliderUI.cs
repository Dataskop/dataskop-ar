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
			currentTimeLabel.text = $"{text[..10]}\n{text[11..]}";
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

			List<VisualElement> ticks = historyContainer.Query(className: "slider-tick").ToList();

			foreach (VisualElement tick in ticks) {
				tick.RemoveFromHierarchy();
			}

		}

		private void GenerateTicks(int dataPointsCount) {

			ClearTicks();

			float sliderTrackHeight = historySlider.resolvedStyle.height;
			int tickInterval = Mathf.CeilToInt(dataPointsCount / 20f);
			float tickSpacing = (sliderTrackHeight - 3) / (dataPointsCount / (float)tickInterval);

			for (int i = 0; i <= dataPointsCount; i += tickInterval) {

				VisualElement tick = new();
				tick.AddToClassList("slider-tick");
				tick.style.width = 12;
				tick.style.height = 3;
				tick.style.left = 16;
				tick.pickingMode = PickingMode.Ignore;

				float tickPosition = tickSpacing * ((float)i / tickInterval);
				tick.style.top = 106 + sliderTrackHeight - tickSpacing * (1f / tickInterval) - tickPosition +
				                 tick.style.height.value.value / 2 -
				                 dragger.resolvedStyle.height / 2;

				historyContainer.hierarchy.Add(tick);
				tick.SendToBack();

			}

		}

	}

}