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

		[Header("Icons")]
		[SerializeField] private Sprite hourIcon;
		[SerializeField] private Sprite daysIcon;

		private VisualElement Root { get; set; }

		private VisualElement HistoryContainer { get; set; }

		private VisualElement Dragger { get; set; }

		private VisualElement RangeContainer { get; set; }

		private VisualElement RectContainer { get; set; }

		private VisualElement TopDragger { get; set; }

		private VisualElement BottomDragger { get; set; }

		private VisualElement SwitchUnitsIcon { get; set; }

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

			TopDragger.hierarchy.Add(StartRangeLabel);
			BottomDragger.hierarchy.Add(EndRangeLabel);

			RectContainer = Root.Q<VisualElement>("RectContainer");

			SwitchUnitsButton = Root.Q<Button>("UnitSwitch");
			SwitchUnitsButton.RegisterCallback<ClickEvent>(_ => ToggleUnitSwitch());

			SwitchUnitsIcon = SwitchUnitsButton.Q<VisualElement>("Icon");
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
				SelectedDataPoint.FocusedMeasurementResultChanged -= UpdateTimeLabel;
				SelectedDataPoint.MeasurementRangeChanged -= OnMeasurementRangeChanged;
			}

			SelectedDataPoint = selectedDataPoint;

			if (SelectedDataPoint == null) {
				CurrentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
				SetVisibility(HistoryContainer, false);
				SetVisibility(RangeContainer, false);
				return;
			}

			SelectedDataPoint.FocusedMeasurementResultChanged += UpdateTimeLabel;
			SelectedDataPoint.MeasurementRangeChanged += OnMeasurementRangeChanged;

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
			UpdateTimeLabel(SelectedDataPoint.FocusedMeasurement);

			// check if we are still on the same device before updating time range
			if (SelectedDataPoint == null) {
				return;
			}

			UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
			CreateCacheRect(SelectedDataPoint.MeasurementDefinition);

		}

		private int GetMeasurementCount() {
			return SelectedDataPoint == null ? 0 : SelectedDataPoint.MeasurementDefinition.GetLatestRange().Count;
		}

		private void UpdateTimeLabel(MeasurementResult focusedResult) {
			CurrentTimeLabel.text = focusedResult.GetDate();
		}

		private DateTime ClampTimeStamp(DateTime timeStamp) {
			return isHourly ? new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day, timeStamp.Hour, 0, 0)
				: new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day);
		}

		private string ShortTimeStamp(DateTime timeStamp) {
			return isHourly ? ClampTimeStamp(timeStamp).ToString("dd.MM. HH:mm") : timeStamp.ToString(AppOptions.DateCulture).Remove(6, 13);
		}

		private void UpdateMinMaxSlider(MeasurementDefinition def, MeasurementResultRange currentRange) {
			MeasurementResult firstResult = def.FirstMeasurementResult;
			MeasurementResult lastResult = def.LatestMeasurementResult;

			UltimateStartTime.text = firstResult.GetShortDate();
			UltimateEndTime.text = lastResult.GetShortDate();

			StartRangeLabel.text = ShortTimeStamp(currentRange.GetTimeRange().StartTime < firstResult.Timestamp ? firstResult.Timestamp
				: currentRange.GetTimeRange().StartTime);
			EndRangeLabel.text = ShortTimeStamp(currentRange.GetTimeRange().EndTime > lastResult.Timestamp ? lastResult.Timestamp
				: currentRange.GetTimeRange().EndTime);

			MinMaxSlider.lowLimit = 0;
			TimeRange overAllRange = new(ClampTimeStamp(firstResult.Timestamp), ClampTimeStamp(lastResult.Timestamp));
			MinMaxSlider.highLimit = isHourly ? (int)overAllRange.Span.TotalHours : (int)overAllRange.Span.TotalDays;

			DateTime clampedStartTime = ClampTimeStamp(currentRange.GetTimeRange().StartTime);
			DateTime clampedEndTime = ClampTimeStamp(currentRange.GetTimeRange().EndTime);

			TimeRange cachedData = new(ClampTimeStamp(lastResult.Timestamp), clampedStartTime);
			MinMaxSlider.maxValue = isHourly ? (int)cachedData.Span.TotalHours
				: (int)cachedData.Span.TotalDays;

			TimeRange rangeToLatestResult = new(clampedEndTime, ClampTimeStamp(lastResult.Timestamp));
			MinMaxSlider.minValue = isHourly ? (int)rangeToLatestResult.Span.TotalHours
				: (int)rangeToLatestResult.Span.TotalDays;

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

		private void SetVisibility(VisualElement element, bool isVisible) {
			element.style.display = new StyleEnum<DisplayStyle>(isVisible ? DisplayStyle.Flex : DisplayStyle.None);
		}

		public void SetHistoryViewState(bool newState) {

			IsActive = newState;

			if (SelectedDataPoint) {
				SetVisibility(HistoryContainer, IsActive);
				SetVisibility(CurrentTimeLabel, IsActive);
				SetVisibility(RangeContainer, IsActive);
				StartCoroutine(GenerateTicks(GetMeasurementCount()));
			}

			historyViewToggled?.Invoke(IsActive);

		}

		public void OnDateFiltered() {
			IsActive = true;

			if (SelectedDataPoint) {
				SetVisibility(HistoryContainer, IsActive);
				SetVisibility(CurrentTimeLabel, IsActive);
				SetVisibility(RangeContainer, IsActive);
				StartCoroutine(GenerateTicks(GetMeasurementCount()));
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

				int numberUnitsCurrentRect = (int)totalUnitsCurrentRect;
				float calculatedWidth = (sliderHeight / highLimit) * numberUnitsCurrentRect;
				float startPosition = 0;
				
				StyleLength leftPosition =
					new StyleLength((float)((sliderHeight) / highLimit * (highLimit - totalUnitsCurrentRectRange)));

				VisualElement rect = new() {
					style = {
						position = Position.Absolute,
						left = new StyleLength(startPosition), // calculate left position (because of transform) to be drawn from EndTime of currentRange up
						width = 20 + calculatedWidth, // calculate width (because of transform) to correspond to number of days
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

				//rect.style.width = rect.style.left.value.value + rect.style.width.value.value > 600 ? calculatedWidth - 1 : calculatedWidth;
				// make sure the width and left position are within the slider bounds
				rect.style.left = Math.Clamp(rect.style.left.value.value, 0, 580);
				rect.style.width = Math.Clamp(rect.style.width.value.value, 20, 600 - rect.style.left.value.value);
				RectContainer.Add(rect);

			}
		}

		private void ToggleUnitSwitch() {
			isHourly = !isHourly;
			SwitchUnitsIcon.style.backgroundImage = new StyleBackground(isHourly ? hourIcon : daysIcon);
			UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
			CreateCacheRect(SelectedDataPoint.MeasurementDefinition);
		}

		private void OnMeasurementRangeChanged() {
			CreateCacheRect(SelectedDataPoint.MeasurementDefinition);
			UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
		}

	}

}