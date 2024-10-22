using System;
using Dataskop.Data;
using Dataskop.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class HistoryControllerUI : MonoBehaviour {

		[SerializeField] private UIDocument historyUIDocument;
		[SerializeField] private HistorySliderUI historySlider;
		[SerializeField] private CachedDataDisplayUI cachedDataDisplay;

		public bool IsActive { get; set; }

		private DataPoint SelectedDataPoint { get; set; }

		private void OnEnable() {
			IsActive = false;
			historySlider.Init(historyUIDocument.rootVisualElement.Q<VisualElement>("HistoryContainer"));
			cachedDataDisplay.Init(historyUIDocument.rootVisualElement.Q<VisualElement>("CachedDataDisplay"));
			historySlider.Hide();
			cachedDataDisplay.Hide();
			historyUIDocument.rootVisualElement.visible = false;
			
			//TODO: Subscribe to the relevant events for the history slider and the cached data.
		}

		public void ShowHistory() {
			historySlider.Show();
			cachedDataDisplay.Show();
		}

		public void HideHistory() {
			historySlider.Hide();
			cachedDataDisplay.Hide();
		}

		public void OnDateFiltered() {

			IsActive = true;

			if (SelectedDataPoint) {
				ShowHistory();
			}

		}

		public void OnHistorySwiped(int newCount) {
			historySlider.SetValueWithoutNotify(newCount);
			AdjustTimeLabelPosition();
		}

		public void OnVisualizationOptionChanged(VisualizationOption currentVisOption) {

			if (IsActive) {

				if (currentVisOption.Style.IsTimeSeries) {
					StartCoroutine(DelayToggle());
					return;
				}

				if (currentVisOption.Style.IsTimeSeries) {
					SetVisibility(Root, currentVisOption.Style.IsTimeSeries);
					SetVisibility(historyContainer, false);
					SetVisibility(RangeContainer, false);
				}
				
				IsActive = false;

			}
			else {
				SetVisibility(Root, currentVisOption.Style.IsTimeSeries);
				SetVisibility(historyContainer, false);
				SetVisibility(RangeContainer, false);
				IsActive = false;
			}

		}

		public void OnDataPointSelectionChanged(DataPoint selectedDataPoint) {

			if (SelectedDataPoint != null) {
				SelectedDataPoint.FocusedMeasurementResultChanged -= UpdateTimeLabel;
				SelectedDataPoint.MeasurementRangeChanged -= OnMeasurementRangeChanged;
			}

			SelectedDataPoint = selectedDataPoint;

			if (SelectedDataPoint == null) {
				currentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
				SetVisibility(historyContainer, false);
				SetVisibility(RangeContainer, false);
				return;
			}

			SelectedDataPoint.FocusedMeasurementResultChanged += UpdateTimeLabel;
			SelectedDataPoint.MeasurementRangeChanged += OnMeasurementRangeChanged;

			if (!IsActive) {
				return;
			}

			SetVisibility(historyContainer, true);
			SetVisibility(RangeContainer, true);

			int newResultsCount = GetMeasurementCount();

			historySlider.highValue = newResultsCount - 1;
			historySlider.SetValueWithoutNotify(SelectedDataPoint.FocusedIndex);

			StartCoroutine(GenerateTicks(newResultsCount));
			currentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
			UpdateTimeLabel(SelectedDataPoint.FocusedMeasurement);

			// check if we are still on the same device before updating time range
			if (SelectedDataPoint == null) {
				return;
			}

			UpdateMinMaxSlider(SelectedDataPoint.MeasurementDefinition, SelectedDataPoint.CurrentMeasurementRange);
			CreateCacheRect(SelectedDataPoint.MeasurementDefinition);

		}

	}

}