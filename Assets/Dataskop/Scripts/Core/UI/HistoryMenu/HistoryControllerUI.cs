using System;
using System.Collections;
using System.Linq;
using Dataskop.Data;
using Dataskop.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class HistoryControllerUI : MonoBehaviour {

		public UnityEvent<int, int> historySliderValueChanged;
		public UnityEvent<TimeRange> dateFilterRequested;

		[SerializeField] private UIDocument historyUIDocument;
		[SerializeField] private CachedDataDisplayUI cachedDataDisplay;
		[SerializeField] private DataPointsManager dataPointsManager;

		private HistorySliderUI historySlider;

		private bool IsActive { get; set; }

		private DataPoint SelectedDataPoint { get; set; }

		private void Start() {
			historySlider.Hide();
			cachedDataDisplay.Hide();
			historySlider.ClearData();
			historySlider.ClearData();
			historyUIDocument.rootVisualElement.visible = false;
		}

		private void OnEnable() {

			IsActive = false;
			historySlider =
				new HistorySliderUI(historyUIDocument.rootVisualElement.Q<VisualElement>("HistoryContainer"));

			cachedDataDisplay.Init(historyUIDocument.rootVisualElement.Q<VisualElement>("CachedDataDisplay"));
			historySlider.SliderValueChanged += OnHistorySliderMoved;
			cachedDataDisplay.OnFilterRequested += OnFilterRequestSent;

		}

		public void OnHistoryButtonPressed() {

			if (IsActive) {
				HideHistory();
			}
			else {
				ShowHistory();
			}

		}

		public void ShowHistory() {

			if (SelectedDataPoint == null) {
				MeasurementResult earliest = dataPointsManager.GetEarliestResult();
				MeasurementResult latest = dataPointsManager.GetLatestResult();
				cachedDataDisplay.SetGlobalTimeLabels(
					earliest.Timestamp.ToShortDateString(), latest.Timestamp.ToShortDateString()
				);

				cachedDataDisplay.SetFilterLabelTexts(
					earliest.Timestamp.ToShortDateString(), latest.Timestamp.ToShortDateString()
				);

				cachedDataDisplay.UpdateMinMaxSlider(latest.Timestamp, earliest.Timestamp);
				cachedDataDisplay.SetLabelPositionsForRange(
					earliest.Timestamp, latest.Timestamp, latest.Timestamp, earliest.Timestamp
				);

				cachedDataDisplay.ClearCacheDisplay();
			}

			historySlider.Show();
			cachedDataDisplay.Show();
			IsActive = true;

		}

		public void HideHistory() {
			IsActive = false;
			historySlider.Hide();
			cachedDataDisplay.Hide();
		}

		public void OnDateFiltered() {

			ShowHistory();
			historySlider.SetValue(0);

			if (SelectedDataPoint) {
				UpdateCachedDataDisplay();
				historySlider.UpdateTicks(SelectedDataPoint.MeasurementCount);
				historySlider.SetLimits(0, SelectedDataPoint.MeasurementCount - 1);
				historySlider.UpdateTimeLabel(SelectedDataPoint.FocusedMeasurement.GetDateText());
			}
			else {
				MeasurementResult earliest = dataPointsManager.GetEarliestResult();
				MeasurementResult latest = dataPointsManager.GetLatestResult();
				cachedDataDisplay.SetGlobalTimeLabels(
					earliest.Timestamp.ToShortDateString(), latest.Timestamp.ToShortDateString()
				);

				cachedDataDisplay.SetFilterLabelTexts(
					earliest.Timestamp.ToShortDateString(), latest.Timestamp.ToShortDateString()
				);

				cachedDataDisplay.UpdateMinMaxSlider(latest.Timestamp, earliest.Timestamp);
				cachedDataDisplay.SetLabelPositionsForRange(
					earliest.Timestamp, latest.Timestamp, latest.Timestamp, earliest.Timestamp
				);

				cachedDataDisplay.ClearCacheDisplay();
				cachedDataDisplay.ClearData();
				historySlider.ClearData();
			}

		}

		public void OnHistorySwiped(int newCount) {
			historySlider.SetValue(newCount);
		}

		public void OnVisualizationOptionChanged(VisualizationOption currentVisOption) {

			if (!currentVisOption.Style.IsTimeSeries) {
				HideHistory();
			}

			if (!IsActive) {
				return;
			}

			if (currentVisOption.Style.IsTimeSeries) {
				StartCoroutine(DelayShow());
			}

		}

		public void OnAttributeChanged(DataAttribute newAttribute) {

			if (!IsActive) {
				return;
			}

			if (newAttribute.ID == "all") {
				IsActive = false;
				HideHistory();
				return;
			}

			historySlider.ClearData();
			cachedDataDisplay.ClearData();
		}

		public void OnDataPointSelectionChanged(DataPoint selectedDataPoint) {

			if (SelectedDataPoint != null) {
				SelectedDataPoint.FocusedMeasurementResultChanged -= OnFocusedResultChanged;
				SelectedDataPoint.MeasurementRangeChanged -= OnMeasurementRangeChanged;
			}

			SelectedDataPoint = selectedDataPoint;

			if (SelectedDataPoint == null) {
				MeasurementResult earliest = dataPointsManager.GetEarliestResult();
				MeasurementResult latest = dataPointsManager.GetLatestResult();
				cachedDataDisplay.SetGlobalTimeLabels(
					earliest.Timestamp.ToShortDateString(), latest.Timestamp.ToShortDateString()
				);

				cachedDataDisplay.SetFilterLabelTexts(
					earliest.Timestamp.ToShortDateString(), latest.Timestamp.ToShortDateString()
				);

				cachedDataDisplay.UpdateMinMaxSlider(latest.Timestamp, earliest.Timestamp);
				cachedDataDisplay.SetLabelPositionsForRange(
					earliest.Timestamp, latest.Timestamp, latest.Timestamp, earliest.Timestamp
				);

				cachedDataDisplay.ClearCacheDisplay();
				historySlider.ClearData();
				return;
			}

			SelectedDataPoint.FocusedMeasurementResultChanged += OnFocusedResultChanged;
			SelectedDataPoint.MeasurementRangeChanged += OnMeasurementRangeChanged;

			int newResultsCount = SelectedDataPoint.MeasurementCount;

			historySlider.SetLimits(0, newResultsCount - 1);
			historySlider.SetValue(SelectedDataPoint.FocusedIndex);
			historySlider.UpdateTicks(newResultsCount);
			historySlider.UpdateTimeLabel(SelectedDataPoint.FocusedMeasurement.GetDateText());

			UpdateCachedDataDisplay();

		}

		private void OnFilterRequestSent(TimeRange filter) {
			dateFilterRequested?.Invoke(filter);
		}

		private void UpdateCachedDataDisplay() {

			MeasurementResultRange currentRange = SelectedDataPoint.CurrentMeasurementRange;
			DateTime rangeStartTime = currentRange.GetTimeRange().StartTime;
			DateTime rangeEndTime = currentRange.GetTimeRange().EndTime;
			DateTime latestResultTime = SelectedDataPoint.MeasurementDefinition.LatestMeasurementResult.Timestamp;
			DateTime firstResultTime = SelectedDataPoint.MeasurementDefinition.FirstMeasurementResult.Timestamp;

			cachedDataDisplay.UpdateMinMaxSlider(latestResultTime, firstResultTime);
			cachedDataDisplay.SetLabelPositionsForRange(
				rangeStartTime, rangeEndTime, latestResultTime, firstResultTime
			);

			string startTimeText = ShortTimeStamp(rangeStartTime < firstResultTime ? firstResultTime : rangeStartTime);
			string endTimeText = ShortTimeStamp(rangeEndTime > latestResultTime ? latestResultTime : rangeEndTime);
			cachedDataDisplay.SetFilterLabelTexts(startTimeText, endTimeText);

			cachedDataDisplay.SetGlobalTimeLabels(
				SelectedDataPoint.MeasurementDefinition.FirstMeasurementResult.GetShortDateText(),
				SelectedDataPoint.MeasurementDefinition.MeasurementResults.First().GetTimeRange().EndTime
					.ToShortDateString()
			);

			cachedDataDisplay.ClearCacheDisplay();

			foreach (MeasurementResultRange mrr in SelectedDataPoint.MeasurementDefinition.MeasurementResults) {
				TimeRange tr = mrr.GetTimeRange();
				cachedDataDisplay.CreateCacheRect(tr.StartTime, tr.EndTime, latestResultTime);
			}

		}

		private void OnHistorySliderMoved(int newCount, int prevCount) {
			historySliderValueChanged?.Invoke(newCount, prevCount);
		}

		private void OnFocusedResultChanged(MeasurementResult result) {
			historySlider.UpdateTimeLabel(result.GetDateText());
		}

		private void OnMeasurementRangeChanged() {
			UpdateCachedDataDisplay();
		}

		private IEnumerator DelayShow() {
			yield return new WaitForEndOfFrame();
			ShowHistory();
		}

		private string ShortTimeStamp(DateTime timeStamp) {
			return timeStamp.ToString(AppOptions.DateCulture).Remove(6, 13);
		}

	}

}