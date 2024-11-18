using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class CachedDataDisplayUI : MonoBehaviour {

		public event Action<TimeRange> OnFilterRequested;

		private const int sliderHeight = 480;

		private VisualElement bottomDragger;
		private VisualElement cachedRangeContainer;
		private VisualElement cachedRangesDisplay;
		private Label currentEndRangeLabel;
		private Label currentStartRangeLabel;
		private VisualElement rangeContainer;
		private MinMaxSlider slider;
		private Button confirmFilterButton;
		private VisualElement topDragger;
		private Label totalEndTimeLabel;
		private Label totalStartTimeLabel;

		private DateTime earliestDate;
		private DateTime latestDate;
		private TimeRange currentFilterRange;

		public void Init(VisualElement container) {
			cachedRangeContainer = container;

			totalEndTimeLabel = cachedRangeContainer.Q<Label>("LabelMinDate");
			totalStartTimeLabel = cachedRangeContainer.Q<Label>("LabelMaxDate");

			currentEndRangeLabel = cachedRangeContainer.Q<Label>("LabelMinValue");
			currentStartRangeLabel = cachedRangeContainer.Q<Label>("LabelMaxValue");

			slider = cachedRangeContainer.Q<MinMaxSlider>("FilterSlider");

			topDragger = cachedRangeContainer.Q<VisualElement>("unity-thumb-max");
			bottomDragger = cachedRangeContainer.Q<VisualElement>("unity-thumb-min");

			slider.RegisterCallback<ChangeEvent<Vector2>>(
				e =>
				{
					currentFilterRange = GetTimeRangeOfFilter(e.newValue);
					SetFilterLabelTexts(
						currentFilterRange.StartTime.ToShortDateString(), currentFilterRange.EndTime.ToShortDateString()
					);
				}
			);

			topDragger.hierarchy.Add(currentStartRangeLabel);
			bottomDragger.hierarchy.Add(currentEndRangeLabel);

			cachedRangesDisplay = cachedRangeContainer.Q<VisualElement>("CachedRangesDisplay");

			confirmFilterButton = cachedRangeContainer.Q<Button>("UnitSwitch");
			confirmFilterButton.RegisterCallback<ClickEvent>(_ => OnFilterRequested?.Invoke(currentFilterRange));
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
			slider.lowLimit = 0;
			TimeRange overAllRange = new(ClampTimeStamp(firstResultTime), ClampTimeStamp(latestResultTime));
			slider.highLimit = (int)overAllRange.Span.TotalDays;
			earliestDate = firstResultTime;
			latestDate = latestResultTime;
		}

		public void SetLabelPositionsForRange(DateTime rangeStartTime, DateTime rangeEndTime, DateTime latestResultTime,
			DateTime firstResultTime) {

			if (rangeEndTime < firstResultTime || rangeStartTime > latestResultTime) {
				return;
			}

			DateTime clampedStartTime = ClampTimeStamp(rangeStartTime);
			DateTime clampedEndTime = ClampTimeStamp(rangeEndTime);
			TimeRange cachedData = new(ClampTimeStamp(latestResultTime), clampedStartTime);
			slider.maxValue = (int)cachedData.Span.TotalDays + 1;
			TimeRange rangeToLatestResult = new(clampedEndTime, ClampTimeStamp(latestResultTime));
			slider.minValue = (int)rangeToLatestResult.Span.TotalDays;

		}

		public void SetGlobalTimeLabels(string first, string latest) {
			totalStartTimeLabel.text = first;
			totalEndTimeLabel.text = latest;
		}

		public void SetFilterLabelTexts(string startLabel, string endLabel) {
			currentStartRangeLabel.text = startLabel[..6];
			currentEndRangeLabel.text = endLabel[..6];
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

		private TimeRange GetTimeRangeOfFilter(Vector2 newValue) {
			DateTime topDate =
				earliestDate.Add(new TimeSpan((int)slider.highLimit - (int)Mathf.Round(newValue.y), 0, 0, 0));

			DateTime bottomDate = latestDate.Subtract(new TimeSpan((int)Mathf.Round(newValue.x), 0, 0, 0));
			return new TimeRange(topDate, bottomDate);
		}

		private DateTime ClampTimeStamp(DateTime timeStamp) {
			return new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day);
		}

	}

}
