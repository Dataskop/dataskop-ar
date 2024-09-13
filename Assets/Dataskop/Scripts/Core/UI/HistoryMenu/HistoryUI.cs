using System;
using System.Collections;
using System.Collections.Generic;
using Dataskop.Data;
using Dataskop.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Position = UnityEngine.UIElements.Position;

namespace Dataskop.UI {

	public class HistoryUI : MonoBehaviour {

		[Header("Events")]
		public UnityEvent<int, int> sliderChanged;
		public UnityEvent<bool> historyViewToggled;

		[Header("References")]
		[SerializeField] private UIDocument historyMenuDoc;
		private string currentAttributeId;

		private string currentDeviceId;

		private VisualElement Root { get; set; }

		private VisualElement HistoryContainer { get; set; }

		private VisualElement Dragger { get; set; }

		private VisualElement RangeContainer { get; set; }

		private VisualElement RectContainer { get; set; }

		private VisualElement TopDragger { get; set; }

		private VisualElement BottomDragger { get; set; }

		private Button SwitchUnitsButton { get; set; }

		private SliderInt HistorySlider { get; set; }

		private MinMaxSlider MinMaxSlider { get; set; }

		private bool IsActive { get; set; }

		private Label CurrentTimeLabel { get; set; }

		private Label UltimateEndTime { get; set; }

		private Label UltimateStartTime { get; set; }

		private Label EndRangeLabel { get; set; }

		private Label StartRangeLabel { get; set; }

		private DataPoint SelectedDataPoint { get; set; }

		private bool isHourly;

		private void Start() {
			SetVisibility(HistoryContainer, false);
			SetVisibility(RangeContainer, false);
		}

		private void OnEnable() {

			Root = historyMenuDoc.rootVisualElement;
			HistoryContainer = Root.Q<VisualElement>("HistoryContainer");

			HistorySlider = HistoryContainer.Q<SliderInt>("Slider");
			HistorySlider.RegisterCallback<ChangeEvent<int>>(SliderValueChanged);

			CurrentTimeLabel = HistoryContainer.Q<Label>("CurrentTime");
			Dragger = HistorySlider.Q<VisualElement>("unity-dragger");
			Dragger.RegisterCallback<GeometryChangedEvent>(_ => AdjustTimeLabelPosition());

			RangeContainer = Root.Q<VisualElement>("RangeContainer");

			UltimateEndTime = RangeContainer.Q<Label>("LabelMinDate");
			UltimateStartTime = RangeContainer.Q<Label>("LabelMaxDate");

			EndRangeLabel = RangeContainer.Q<Label>("LabelMinValue");
			StartRangeLabel = RangeContainer.Q<Label>("LabelMaxValue");

			MinMaxSlider = RangeContainer.Q<MinMaxSlider>("MinMaxSlider");

			TopDragger = RangeContainer.Q<VisualElement>("unity-thumb-max");
			BottomDragger = RangeContainer.Q<VisualElement>("unity-thumb-min");

			TopDragger.RegisterCallback<GeometryChangedEvent>(_ => AdjustTopDateLabelPositions());
			BottomDragger.RegisterCallback<GeometryChangedEvent>(_ => AdjustBottomDateLabelPositions());

			RectContainer = Root.Q<VisualElement>("RectContainer");

			SwitchUnitsButton = Root.Q<Button>("UnitSwitch");
			SwitchUnitsButton.RegisterCallback<ClickEvent>(_ => ToggleUnitSwitch(SelectedDataPoint));
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
				SetVisibility(RangeContainer, false);
				return;
			}

			SelectedDataPoint.FocusedIndexChanged += UpdateTimeLabel;

			if (!IsActive) {
				return;
			}

			SetVisibility(HistoryContainer, true);
			SetVisibility(RangeContainer, true);

			int newResultsCount = GetMeasurementCount();

			HistorySlider.highValue = newResultsCount - 1;
			HistorySlider.SetValueWithoutNotify(SelectedDataPoint.FocusedIndex);

			StartCoroutine(GenerateTicks(newResultsCount));
			CurrentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
			UpdateTimeLabel(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.FocusedIndex);

			// check if we are still on the same device before updating time range
			if (SelectedDataPoint == null) {
				return;
			}
			UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition);
			// draw cache rects
			CreateCacheRect(SelectedDataPoint.MeasurementDefinition);
		}

		private int GetMeasurementCount() {

			if (SelectedDataPoint == null) {
				return 0;
			}

			return SelectedDataPoint.MeasurementDefinition.GetLatestRange().Count;
		}

		private void UpdateTimeLabel(MeasurementDefinition def, int index) {
			MeasurementResult focusedResult = def.GetMeasurementResult(index);
			CurrentTimeLabel.text = focusedResult.GetDate();
		}

		private DateTime ClampTimeStamp(DateTime timeStamp) {
			if (isHourly) {
				return new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day, timeStamp.Hour, 0, 0);
			}
			return new(timeStamp.Year, timeStamp.Month, timeStamp.Day);
		}

		private string ShortTimeStamp(DateTime timeStamp) {
			if (isHourly) {
				return ClampTimeStamp(timeStamp).ToString("dd.MM. HH:mm");
			}
			return timeStamp.ToString(AppOptions.DateCulture).Remove(6, 13);
		}

		private void UpdateMinMaxSlider(MeasurementDefinition def) {
			MeasurementResult firstResult = def.FirstMeasurementResult;
			MeasurementResult lastResult = def.GetLatestMeasurementResult();

			UltimateStartTime.text = firstResult.GetShortDate();
			UltimateEndTime.text = lastResult.GetShortDate();

			StartRangeLabel.text = ShortTimeStamp(def.GetLatestRange().GetTimeRange().StartTime);
			EndRangeLabel.text = ShortTimeStamp(def.GetLatestRange().GetTimeRange().EndTime);

			MinMaxSlider.lowLimit = 0;
			TimeRange overAllRange = new(ClampTimeStamp(firstResult.Timestamp), ClampTimeStamp(lastResult.Timestamp));
			MinMaxSlider.highLimit = isHourly ? (int)overAllRange.Span.TotalHours : (int)overAllRange.Span.TotalDays;

			DateTime clampedStartTime = ClampTimeStamp(def.GetLatestRange().GetTimeRange().StartTime);
			DateTime clampedEndTime = ClampTimeStamp(lastResult.Timestamp);

			MinMaxSlider.minValue = 0;
			TimeRange cachedData = new(clampedStartTime, clampedEndTime);
			MinMaxSlider.maxValue = isHourly ? (int)cachedData.Span.TotalHours : (int)cachedData.Span.TotalDays + (cachedData.Span.TotalDays == 0 ? 1 : 0) ;
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
				SetVisibility(RangeContainer, false);
				IsActive = false;

			}
			else {
				SetVisibility(Root, currentVisOption.Style.IsTimeSeries);
				SetVisibility(HistoryContainer, false);
				SetVisibility(RangeContainer, false);
				IsActive = false;
			}

		}

		private void AdjustTimeLabelPosition() {
			CurrentTimeLabel.style.top = Dragger.localBound.yMax - Dragger.resolvedStyle.height;
		}

		private void AdjustTopDateLabelPositions() {
			if (isHourly) {
				StartRangeLabel.style.left = TopDragger.localBound.xMax - TopDragger.resolvedStyle.width - 50;
				StartRangeLabel.style.top = 70;
			}
			else {
				StartRangeLabel.style.left = TopDragger.localBound.xMax - TopDragger.resolvedStyle.width - 25;
				StartRangeLabel.style.top = 41;
			}
		}

		private void AdjustBottomDateLabelPositions() {
			if (isHourly) {
				EndRangeLabel.style.left = BottomDragger.localBound.xMax - BottomDragger.resolvedStyle.width - 50;
				EndRangeLabel.style.top = 70;
			}
			else {
				EndRangeLabel.style.left = BottomDragger.localBound.xMax - BottomDragger.resolvedStyle.width - 25;
				EndRangeLabel.style.top = 41;
			}
		}

		private void SetVisibility(VisualElement element, bool isVisible) {
			element.style.display = new StyleEnum<DisplayStyle>(isVisible ? DisplayStyle.Flex : DisplayStyle.None);
		}

		public void ToggleHistoryView() {

			IsActive = !IsActive;

			if (SelectedDataPoint) {
				SetVisibility(HistoryContainer, IsActive);
				SetVisibility(CurrentTimeLabel, IsActive);
				SetVisibility(RangeContainer, IsActive);
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
				tick.style.top = sliderTrackHeight - tickPosition + tick.style.height.value.value / 2 - Dragger.resolvedStyle.height / 2;
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

		private void CreateCacheRect(MeasurementDefinition def) {
			// Clear the container for fresh data
			RectContainer.Clear();
			MeasurementResult firstResult = def.FirstMeasurementResult;
			DateTime firstResultTimestampClamped = ClampTimeStamp(firstResult.Timestamp);

			// Slider Data
			float highLimit = MinMaxSlider.highLimit;
			int sliderHeight = 600;

			foreach (MeasurementResultRange measurementResultRange in def.MeasurementResults) {
				DateTime clampedStartTime = ClampTimeStamp(measurementResultRange.GetTimeRange().StartTime);
				DateTime clampedEndTime = ClampTimeStamp(measurementResultRange.GetTimeRange().EndTime);

				TimeRange timeRangeCurrentRect = new(clampedStartTime, clampedEndTime);
				TimeRange timeRangeAllDataEndTimeCurrentRange = new(firstResultTimestampClamped, clampedEndTime);

				// Calculate the number of time units (hours or days) for the current rect and full range
				double totalUnitsCurrentRect = isHourly ? timeRangeCurrentRect.Span.TotalHours : timeRangeCurrentRect.Span.Days;
				double totalUnitsCurrentRectRange = isHourly ? timeRangeAllDataEndTimeCurrentRange.Span.TotalHours
					: timeRangeAllDataEndTimeCurrentRange.Span.Days;

				// Ensure minimum value of 1 for totalUnitsCurrentRect
				int numberUnitsCurrentRect = (int)totalUnitsCurrentRect + (totalUnitsCurrentRect == 0 ? 1 : 0);

				float calculatedWidth = (sliderHeight - 20) / highLimit * numberUnitsCurrentRect;
				StyleLength leftPosition = (StyleLength)((sliderHeight - 10) / highLimit * (highLimit - totalUnitsCurrentRectRange) + 10);

				VisualElement rect = new() {
					style = {
						position = Position.Absolute,
						left = leftPosition, // calculate left position (because of transform) to be drawn from EndTime of currentRange up
						width = calculatedWidth, // calculate width (because of transform) to correspond to number of days
						height = 10,
						marginTop = -5,
						marginLeft = 1,
						backgroundColor = new StyleColor(new Color32(219, 105, 11, 200))
					}
				};
				rect.style.width = rect.style.left.value.value + rect.style.width.value.value > 590 ? calculatedWidth - 10
					: calculatedWidth;
				RectContainer.Add(rect);
			}
		}

		private void ToggleUnitSwitch(DataPoint selectedDatapoint) {
			isHourly = !isHourly;
			UpdateMinMaxSlider(selectedDatapoint.MeasurementDefinition);
			CreateCacheRect(selectedDatapoint.MeasurementDefinition);
			AdjustTopDateLabelPositions();
			AdjustBottomDateLabelPositions();
		}

	}

}