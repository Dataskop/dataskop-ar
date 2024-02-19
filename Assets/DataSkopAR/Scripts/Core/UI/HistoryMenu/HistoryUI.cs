using System.Collections;
using DataskopAR.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class HistoryUI : MonoBehaviour {

#region Fields

		[Header("Events")]
		public UnityEvent<int, int> sliderChanged;
		public UnityEvent<bool> historyViewToggled;

		[Header("References")]
		[SerializeField] private UIDocument historyMenuDoc;
		[SerializeField] private DataManager dataManager;

		private bool isExternalInteraction;

#endregion

#region Properties

		private VisualElement Root { get; set; }
		private VisualElement HistoryContainer { get; set; }
		private VisualElement HistorySliderContainer { get; set; }
		private VisualElement Dragger { get; set; }
		private SliderInt HistorySlider { get; set; }
		private bool IsActive { get; set; }
		private Label CurrentTimeLabel { get; set; }
		private DataPoint SelectedDataPoint { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			Root = historyMenuDoc.rootVisualElement;
			HistoryContainer = Root.Q<VisualElement>("HistoryContainer");
			HistorySliderContainer = HistoryContainer.Q<VisualElement>("HistorySliderContainer");

			HistorySliderContainer.RegisterCallback<PointerDownEvent>((e) => { UIInteractionDetection.IsPointerOverUi = true; });

			HistorySlider = HistorySliderContainer.Q<SliderInt>("Slider");
			HistorySlider.RegisterCallback<ChangeEvent<int>>(SliderValueChanged);
			HistorySlider.RegisterCallback<PointerDownEvent>(e => { UIInteractionDetection.HasPointerStartedOverSlider = true; });
			HistorySlider.RegisterCallback<PointerUpEvent>(e => { UIInteractionDetection.HasPointerStartedOverSlider = false; });

			CurrentTimeLabel = HistorySliderContainer.Q<Label>("CurrentTime");
			Dragger = HistorySlider.Q<VisualElement>("unity-dragger");

#if UNITY_EDITOR

			HistorySliderContainer.RegisterCallback<PointerEnterEvent>((e) => { UIInteractionDetection.IsPointerOverUi = true; });
			HistorySlider.RegisterCallback<PointerDownEvent>(e => { UIInteractionDetection.HasPointerStartedOverSlider = true; });
			HistorySliderContainer.RegisterCallback<PointerLeaveEvent>((e) => { UIInteractionDetection.IsPointerOverUi = false; });
			HistorySlider.RegisterCallback<PointerUpEvent>(e => { UIInteractionDetection.HasPointerStartedOverSlider = false; });

#endif
		}

		private void Start() {
			SetVisibility(Root, false);
			HistorySlider.highValue = dataManager.FetchAmount - 1;
		}

		private void SliderValueChanged(ChangeEvent<int> e) {

			if (!isExternalInteraction) {
				sliderChanged?.Invoke(e.newValue, e.previousValue);
			}

			AdjustTimeLabelPosition();

		}

		private void AdjustTimeLabelPosition() {
			CurrentTimeLabel.style.top = Dragger.localBound.yMax;
		}

		public void OnFetchedAmountChanged(int newValue) {
			HistorySlider.highValue = newValue - 1;
		}

		public void OnDataPointSelectionChanged(DataPoint selectedDataPoint) {

			if (SelectedDataPoint != null) {
				SelectedDataPoint.MeasurementResultChanged -= UpdateTimeLabel;
			}

			SelectedDataPoint = selectedDataPoint;

			if (SelectedDataPoint == null) {
				CurrentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
				return;
			}

			SelectedDataPoint.MeasurementResultChanged += UpdateTimeLabel;
			UpdateTimeLabel(SelectedDataPoint.CurrentMeasurementResult);

			if (IsActive) {
				CurrentTimeLabel.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
			}

		}

		private void UpdateTimeLabel(MeasurementResult currentDataPointMeasurementResult) {
			CurrentTimeLabel.text = $"{currentDataPointMeasurementResult.GetDate()}<br>{currentDataPointMeasurementResult.GetClockTime()}";
		}

		public void OnDataPointHistorySwiped(int newCount) {
			isExternalInteraction = true;
			HistorySlider.value = newCount;
			isExternalInteraction = false;
		}

		public void OnVisualizationOptionChanged(VisualizationOption currentVisOption) {

			HistorySlider.value = 0;

			if (IsActive) {

				if (currentVisOption.Style.IsTimeSeries) {
					StartCoroutine(DelayToggle());
					return;
				}

				SetVisibility(Root, currentVisOption.Style.IsTimeSeries);
				SetVisibility(HistorySliderContainer, false);
				IsActive = false;

			}
			else {
				SetVisibility(Root, currentVisOption.Style.IsTimeSeries);
				SetVisibility(HistorySliderContainer, false);
				IsActive = false;
			}
		}

		private static void SetVisibility(VisualElement element, bool isVisible) {
			element.style.visibility = new StyleEnum<Visibility>(isVisible ? Visibility.Visible : Visibility.Hidden);
		}

		public void ToggleHistoryView() {

			IsActive = !IsActive;
			SetVisibility(HistorySliderContainer, IsActive);

			if (SelectedDataPoint) {
				SetVisibility(CurrentTimeLabel, IsActive);
			}

			HistorySlider.value = 0;
			CurrentTimeLabel.style.top = Dragger.localBound.yMax;
			historyViewToggled?.Invoke(IsActive);

		}

		public void OnDataPointsResultsUpdated() {

			if (IsActive) {
				ToggleHistoryView();
			}

		}

		private void OnDisable() {
			HistorySliderContainer.UnregisterCallback<ChangeEvent<int>>(e => sliderChanged?.Invoke(e.newValue, e.previousValue));
		}

		private IEnumerator DelayToggle() {
			yield return new WaitForSeconds(0.015f);
			historyViewToggled?.Invoke(IsActive);
		}

#endregion

	}

}