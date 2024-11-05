using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class CachedDataDisplayUI : MonoBehaviour {

		[Header("Icons")]
		[SerializeField] private Sprite hourIcon;
		[SerializeField] private Sprite daysIcon;

		private VisualElement bottomDragger;
		private VisualElement cachedRangeContainer;
		private VisualElement cachedRangesDisplay;
		private Label currentEndRangeLabel;
		private Label currentStartRangeLabel;
		private VisualElement rangeContainer;
		private MinMaxSlider slider;
		private int sliderHeight = 580;
		private Button confirmFilterButton;
		private VisualElement topDragger;
		private Label totalEndTimeLabel;
		private Label totalStartTimeLabel;

		public void Init(VisualElement container) {
			cachedRangeContainer = container;

			totalEndTimeLabel = cachedRangeContainer.Q<Label>("LabelMinDate");
			totalStartTimeLabel = cachedRangeContainer.Q<Label>("LabelMaxDate");

			currentEndRangeLabel = cachedRangeContainer.Q<Label>("LabelMinValue");
			currentStartRangeLabel = cachedRangeContainer.Q<Label>("LabelMaxValue");

			slider = cachedRangeContainer.Q<MinMaxSlider>("MinMaxSlider");

			topDragger = cachedRangeContainer.Q<VisualElement>("unity-thumb-max");
			bottomDragger = cachedRangeContainer.Q<VisualElement>("unity-thumb-min");

			topDragger.hierarchy.Add(currentStartRangeLabel);
			bottomDragger.hierarchy.Add(currentEndRangeLabel);

			cachedRangesDisplay = cachedRangeContainer.Q<VisualElement>("CachedRangesDisplay");

			confirmFilterButton = cachedRangeContainer.Q<Button>("UnitSwitch");
			confirmFilterButton.RegisterCallback<ClickEvent>(_ => Debug.Log("Pressed Confirm Filter Button"));

		}

		public void Show() {
			cachedRangeContainer.visible = true;
		}

		public void Hide() {
			cachedRangeContainer.visible = false;
		}

		public void ClearData() {
			cachedRangesDisplay.Clear();
			totalStartTimeLabel.text = "";
			totalEndTimeLabel.text = "";
			currentStartRangeLabel.text = "";
			currentEndRangeLabel.text = "";
			slider.minValue = slider.lowLimit;
			slider.maxValue = slider.lowLimit;
		}

		public void UpdateMinMaxSlider(DateTime latestResultTime, DateTime firstResultTime) {

			slider.lowLimit = 1;
			TimeRange overAllRange = new(ClampTimeStamp(firstResultTime), ClampTimeStamp(latestResultTime));
			slider.highLimit = (int)overAllRange.Span.TotalDays + 1;

		}

		public void SetLabelPositionsForRange(DateTime rangeStartTime, DateTime rangeEndTime, DateTime latestResultTime,
			DateTime firstResultTime) {

			if (rangeEndTime < firstResultTime || rangeStartTime > latestResultTime) {
				return;
			}

			DateTime clampedStartTime = ClampTimeStamp(rangeStartTime);
			DateTime clampedEndTime = ClampTimeStamp(rangeEndTime);

			TimeRange cachedData = new(ClampTimeStamp(latestResultTime), clampedStartTime);
			slider.maxValue = 1 + (int)cachedData.Span.TotalDays + 1;

			TimeRange rangeToLatestResult = new(clampedEndTime, ClampTimeStamp(latestResultTime));
			slider.minValue = 1 + (int)rangeToLatestResult.Span.TotalDays;

		}

		public void SetGlobalTimeLabels(string first, string latest) {
			totalStartTimeLabel.text = first;
			totalEndTimeLabel.text = latest;
		}

		public void SetFilterLabelTexts(string startLabel, string endLabel) {
			currentStartRangeLabel.text = startLabel;
			currentEndRangeLabel.text = endLabel;
		}

		public void ClearCacheDisplay() {
			cachedRangesDisplay.Clear();
		}

		public void CreateCacheRect(DateTime rangeStartTime, DateTime rangeEndTime, DateTime latestResultTime) {

			float highLimit = slider.highLimit;

			DateTime clampedStartTime = ClampTimeStamp(rangeStartTime);
			DateTime clampedEndTime = ClampTimeStamp(rangeEndTime);

			DateTime latestResultTimeStamp = ClampTimeStamp(latestResultTime);

			TimeRange timeRangeCurrentRect = new(clampedStartTime, clampedEndTime);
			TimeRange rangeToLatestResult = new(latestResultTimeStamp, clampedEndTime);

			// Calculate the number of time units (hours or days) for the current rect and full range
			double rangeInUnits = timeRangeCurrentRect.Span.Days + 1;
			double unitsToLatestResult = rangeToLatestResult.Span.Days;
			int numberUnitsCurrentRect = (int)Mathf.Clamp((int)rangeInUnits, 1, highLimit);
			float calculatedWidth = Mathf.Round(sliderHeight / highLimit * numberUnitsCurrentRect);
			float startPosition = 10 + ((int)unitsToLatestResult > 0 ? sliderHeight / highLimit : 0) +
			                      sliderHeight / highLimit * (int)unitsToLatestResult;

			VisualElement rect = new() {
				style = {
					position = Position.Absolute,
					left = new StyleLength(startPosition),
					width = calculatedWidth,
					height = 12,
					marginTop = 0,
					marginLeft = 0,
					marginBottom = 0,
					marginRight = 0,
					paddingBottom = 0,
					paddingLeft = 0,
					paddingRight = 0,
					paddingTop = 0,
					backgroundColor = new StyleColor(new Color32(219, 105, 11, 200))
				}
			};

			rect.style.left = Math.Clamp(rect.style.left.value.value, 10, 590);
			rect.style.width = Math.Clamp(rect.style.width.value.value, 0, 590 - rect.style.left.value.value);
			cachedRangesDisplay.Add(rect);

		}

		private DateTime ClampTimeStamp(DateTime timeStamp) {
			return new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day);
		}

	}

}