using System.Collections;
using Dataskop.Data;
using Dataskop.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class HistoryControllerUI : MonoBehaviour {

		public UnityEvent<int, int> historySliderValueChanged;

		[SerializeField] private UIDocument historyUIDocument;
		[SerializeField] private CachedDataDisplayUI cachedDataDisplay;
		[SerializeField] private DataPointsManager dataPointsManager;

		private HistorySliderUI historySlider;

		public bool IsActive { get; set; }

		private DataPoint SelectedDataPoint { get; set; }

		private void Start() {
			historySlider.Hide();
			cachedDataDisplay.Hide();
			historySlider.ClearData();
			cachedDataDisplay.ClearData();
			historyUIDocument.rootVisualElement.visible = false;
		}

		private void OnEnable() {
			IsActive = false;
			historySlider = new HistorySliderUI(historyUIDocument.rootVisualElement.Q<VisualElement>("HistoryContainer"));
			cachedDataDisplay.Init(historyUIDocument.rootVisualElement.Q<VisualElement>("CachedDataDisplay"));
			historySlider.SliderValueChanged += OnHistorySliderMoved;
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
				cachedDataDisplay.UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
				cachedDataDisplay.CreateCacheRect(SelectedDataPoint.MeasurementDefinition);
				historySlider.UpdateTicks(SelectedDataPoint.MeasurementCount);
				historySlider.SetLimits(0, SelectedDataPoint.MeasurementCount - 1);
				historySlider.UpdateTimeLabel(SelectedDataPoint.FocusedMeasurement.GetDateText());
			}
			else {
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

			if (!IsActive) return;

			if (currentVisOption.Style.IsTimeSeries) {
				StartCoroutine(DelayShow());
			}

		}

		public void OnAttributeChanged(DataAttribute newAttribute) {

			if (!IsActive) return;

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

				//TODO: Get somehow the data of all the MDs and display that.
				
				
				
				historySlider.ClearData();
				cachedDataDisplay.ClearData();
				return;
			}

			SelectedDataPoint.FocusedMeasurementResultChanged += OnFocusedResultChanged;
			SelectedDataPoint.MeasurementRangeChanged += OnMeasurementRangeChanged;

			int newResultsCount = SelectedDataPoint.MeasurementCount;

			historySlider.SetLimits(0, newResultsCount - 1);
			historySlider.SetValue(SelectedDataPoint.FocusedIndex);
			historySlider.UpdateTicks(newResultsCount);
			historySlider.UpdateTimeLabel(SelectedDataPoint.FocusedMeasurement.GetDateText());
			cachedDataDisplay.UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
			cachedDataDisplay.CreateCacheRect(SelectedDataPoint.MeasurementDefinition);

		}

		private void OnHistorySliderMoved(int newCount, int prevCount) {
			historySliderValueChanged?.Invoke(newCount, prevCount);
		}

		private void OnFocusedResultChanged(MeasurementResult result) {
			historySlider.UpdateTimeLabel(result.GetDateText());
		}

		private void OnMeasurementRangeChanged() {
			cachedDataDisplay.CreateCacheRect(SelectedDataPoint.MeasurementDefinition);
			cachedDataDisplay.UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
		}

		private IEnumerator DelayShow() {
			yield return new WaitForEndOfFrame();
			ShowHistory();
		}

	}

}