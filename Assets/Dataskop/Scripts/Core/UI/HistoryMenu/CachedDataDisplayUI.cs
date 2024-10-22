using System;
using Dataskop.Data;
using UnityEngine;
using UnityEngine.UIElements;
using Position = UnityEngine.UIElements.Position;

namespace Dataskop.UI {

	public class CachedDataDisplayUI : MonoBehaviour {

		[Header("References")]
		[SerializeField] private UIDocument historyMenuDoc;

		[Header("Icons")]
		[SerializeField] private Sprite hourIcon;
		[SerializeField] private Sprite daysIcon;

		private VisualElement cachedRangeContainer;
		private VisualElement topDragger;
		private VisualElement bottomDragger;
		private VisualElement rangeContainer;
		private VisualElement cachedRangesDisplay;
		private VisualElement switchUnitsIcon;
		private Button switchUnitsButton;
		private Label totalEndTimeLabel;
		private Label totalStartTimeLabel;
		private Label currentEndRangeLabel;
		private Label currentStartRangeLabel;
		private MinMaxSlider slider;
		private bool isHourly;

		public void Init(VisualElement container) {
			cachedRangeContainer = container;
			cachedRangeContainer = historyMenuDoc.rootVisualElement.Q<VisualElement>("RangeContainer");

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

			switchUnitsButton = cachedRangeContainer.Q<Button>("UnitSwitch");
			switchUnitsButton.RegisterCallback<ClickEvent>(_ => ToggleUnitSwitch());

			switchUnitsIcon = switchUnitsButton.Q<VisualElement>("Icon");
		}

		public void Show() {
			cachedRangeContainer.visible = true;
		}

		public void Hide() {
			cachedRangeContainer.visible = false;
		}

		private void UpdateMinMaxSlider(MeasurementDefinition def, MeasurementResultRange currentRange) {
			MeasurementResult firstResult = def.FirstMeasurementResult;
			MeasurementResult lastResult = def.LatestMeasurementResult;

			totalStartTimeLabel.text = firstResult.GetShortDate();
			totalEndTimeLabel.text = lastResult.GetShortDate();

			slider.lowLimit = 1;
			TimeRange overAllRange = new(ClampTimeStamp(firstResult.Timestamp), ClampTimeStamp(lastResult.Timestamp));
			slider.highLimit = isHourly ? (int)overAllRange.Span.TotalHours : (int)overAllRange.Span.TotalDays + 1;

			if (currentRange.GetTimeRange().EndTime < firstResult.Timestamp ||
			    currentRange.GetTimeRange().StartTime > lastResult.Timestamp) {
				return;
			}

			DateTime clampedStartTime = ClampTimeStamp(currentRange.GetTimeRange().StartTime);
			DateTime clampedEndTime = ClampTimeStamp(currentRange.GetTimeRange().EndTime);

			currentStartRangeLabel.text = ShortTimeStamp(currentRange.GetTimeRange().StartTime < firstResult.Timestamp
				? firstResult.Timestamp
				: currentRange.GetTimeRange().StartTime);
			currentEndRangeLabel.text = ShortTimeStamp(currentRange.GetTimeRange().EndTime > lastResult.Timestamp ? lastResult.Timestamp
				: currentRange.GetTimeRange().EndTime);

			TimeRange cachedData = new(ClampTimeStamp(lastResult.Timestamp), clampedStartTime);
			slider.maxValue = isHourly ? (int)cachedData.Span.TotalHours
				: 1 + (int)cachedData.Span.TotalDays + 1;

			TimeRange rangeToLatestResult = new(clampedEndTime, ClampTimeStamp(lastResult.Timestamp));
			slider.minValue = isHourly ? (int)rangeToLatestResult.Span.TotalHours
				: 1 + (int)rangeToLatestResult.Span.TotalDays;

		}

		private void CreateCacheRect(MeasurementDefinition def) {
			// Clear the container for fresh data
			cachedRangesDisplay.Clear();
			MeasurementResult latestResult = def.LatestMeasurementResult;
			DateTime latestResultTimeStamp = ClampTimeStamp(latestResult.Timestamp);

			// Slider Data
			float highLimit = slider.highLimit;
			int sliderHeight = 580;

			foreach (MeasurementResultRange measurementResultRange in def.MeasurementResults) {

				if (measurementResultRange.GetTimeRange().EndTime < def.FirstMeasurementResult.Timestamp ||
				    measurementResultRange.GetTimeRange().StartTime > latestResult.Timestamp) {
					continue;
				}

				DateTime clampedStartTime = ClampTimeStamp(measurementResultRange.GetTimeRange().StartTime);
				DateTime clampedEndTime = ClampTimeStamp(measurementResultRange.GetTimeRange().EndTime);

				TimeRange timeRangeCurrentRect = new(clampedStartTime, clampedEndTime);
				TimeRange rangeToLatestResult = new(latestResultTimeStamp, clampedEndTime);

				// Calculate the number of time units (hours or days) for the current rect and full range
				double rangeInUnits = isHourly ? timeRangeCurrentRect.Span.TotalHours : timeRangeCurrentRect.Span.Days + 1;
				double unitsToLatestResult = isHourly ? rangeToLatestResult.Span.TotalHours : rangeToLatestResult.Span.Days;
				int numberUnitsCurrentRect = (int)Mathf.Clamp((int)rangeInUnits, 1, highLimit);
				float calculatedWidth = Mathf.Round((sliderHeight / highLimit) * numberUnitsCurrentRect);
				float startPosition = 10 + ((int)unitsToLatestResult > 0
					? (sliderHeight / highLimit) : 0) + (sliderHeight / highLimit) * (int)unitsToLatestResult;

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
		}

		private void ToggleUnitSwitch() {
			isHourly = !isHourly;
			switchUnitsIcon.style.backgroundImage = new StyleBackground(isHourly ? hourIcon : daysIcon);
			UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
			CreateCacheRect(SelectedDataPoint.MeasurementDefinition);
		}

		private DateTime ClampTimeStamp(DateTime timeStamp) {
			return isHourly ? new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day, timeStamp.Hour, 0, 0)
				: new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day);
		}

		private string ShortTimeStamp(DateTime timeStamp) {
			return isHourly ? ClampTimeStamp(timeStamp).ToString("dd.MM. HH:mm") : timeStamp.ToString(AppOptions.DateCulture).Remove(6, 13);
		}

	}

}