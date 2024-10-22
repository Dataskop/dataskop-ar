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

		private HistorySliderUI historySlider;

		public bool IsActive { get; set; }

		private DataPoint SelectedDataPoint { get; set; }

		private void Start() {
			historySlider.Hide();
			cachedDataDisplay.Hide();
			historyUIDocument.rootVisualElement.visible = false;
		}

		private void OnEnable() {
			IsActive = false;
			historySlider = new HistorySliderUI(historyUIDocument.rootVisualElement.Q<VisualElement>("HistoryContainer"));
			cachedDataDisplay.Init(historyUIDocument.rootVisualElement.Q<VisualElement>("CachedDataDisplay"));
			historySlider.SliderValueChanged += OnHistorySliderMoved;
			//TODO: Subscribe to the relevant events for the history slider and the cached data.
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

			//TODO: If no DataPoint is selected and Date is filtered, what to do?

		}

		public void OnHistorySwiped(int newCount) {
			historySlider.SetValue(newCount);
		}

		public void OnVisualizationOptionChanged(VisualizationOption currentVisOption) {

			if (currentVisOption.Style.IsTimeSeries) {
				StartCoroutine(DelayShow());
				return;
			}

			if (!currentVisOption.Style.IsTimeSeries) {
				HideHistory();
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
				historySlider.ClearData();
				cachedDataDisplay.ClearData();
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