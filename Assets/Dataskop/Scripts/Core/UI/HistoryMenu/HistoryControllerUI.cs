using System.Collections;
using Dataskop.Data;
using Dataskop.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class HistoryControllerUI : MonoBehaviour {

		[SerializeField] private UIDocument historyUIDocument;
		[SerializeField] private CachedDataDisplayUI cachedDataDisplay;

		private HistorySliderUI historySlider;

		public bool IsActive { get; set; }

		private DataPoint SelectedDataPoint { get; set; }

		private void OnEnable() {
			IsActive = false;
			historySlider = new HistorySliderUI(historyUIDocument.rootVisualElement.Q<VisualElement>("HistoryContainer"));
			cachedDataDisplay.Init(historyUIDocument.rootVisualElement.Q<VisualElement>("CachedDataDisplay"));

			//TODO: Subscribe to the relevant events for the history slider and the cached data.
		}

		private void Start() {
			historySlider.Hide();
			cachedDataDisplay.Hide();
			historyUIDocument.rootVisualElement.visible = false;
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

			IsActive = true;

			if (SelectedDataPoint) {
				ShowHistory();
			}
			//TODO: If no DataPoint is selected and Date is filtered, what to do?

		}

		public void OnHistorySwiped(int newCount) {
			historySlider.SetValue(newCount);
		}

		public void OnVisualizationOptionChanged(VisualizationOption currentVisOption) {

			if (IsActive) {

				if (currentVisOption.Style.IsTimeSeries) {
					StartCoroutine(DelayShow());
					return;
				}

				if (!currentVisOption.Style.IsTimeSeries) {
					HideHistory();
					IsActive = false;
				}

			}

		}

		public void OnDataPointSelectionChanged(DataPoint selectedDataPoint) {

			if (SelectedDataPoint != null) {
				SelectedDataPoint.FocusedMeasurementResultChanged -= OnFocusedResultChanged;
				SelectedDataPoint.MeasurementRangeChanged -= OnMeasurementRangeChanged;
			}

			SelectedDataPoint = selectedDataPoint;

			if (SelectedDataPoint == null) {
				
				//TODO: New functions to display empty Sliders.
				
				return;
			}

			SelectedDataPoint.FocusedMeasurementResultChanged += OnFocusedResultChanged;
			SelectedDataPoint.MeasurementRangeChanged += OnMeasurementRangeChanged;

			if (!IsActive) {
				return;
			}

			ShowHistory();

			int newResultsCount = SelectedDataPoint.MeasurementCount;

			historySlider.SetLimits(0, newResultsCount - 1);
			historySlider.SetValue(SelectedDataPoint.FocusedIndex);
			historySlider.UpdateTicks(newResultsCount);
			historySlider.UpdateTimeLabel(SelectedDataPoint.FocusedMeasurement.GetDateText());

			// check if we are still on the same device before updating time range
			if (SelectedDataPoint == null) {
				return;
			}

			cachedDataDisplay.UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
			cachedDataDisplay.CreateCacheRect(SelectedDataPoint.MeasurementDefinition);

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