using System;
using DataskopAR.Data;
using DataskopAR.Interaction;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace DataskopAR.UI {

	public class CalibratorUI : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument calibratorUiDoc;
		[SerializeField] private CanvasGroup subCanvasGroup;
		[SerializeField] private Image calibrationProgressIndicator;
		[SerializeField] private Calibrator calibrator;

		[Header("Values")]
		[SerializeField] private int numberOfPhases;

#endregion

#region Properties

		public VisualElement CalibratorRoot { get; set; }

		public Label GuideLabel { get; set; }

		public Label StepLabel { get; set; }

		public Button CalibratorButton { get; set; }

		private CalibratorPhase CalibratorPhase { get; set; }

		private int PhaseCounter { get; set; }

		private Calibrator Calibrator => calibrator;

#endregion

#region Methods

		private void OnEnable() {

			CalibratorRoot = calibratorUiDoc.rootVisualElement.Q<VisualElement>("CalibratorContainer");
			SetVisibility(false);

			GuideLabel = CalibratorRoot.Q<Label>("GuideLabel");
			StepLabel = CalibratorRoot.Q<Label>("StepText");

			CalibratorButton = CalibratorRoot.Q<Button>("CalibratorButton");
			CalibratorButton.RegisterCallback<ClickEvent>(e => { Calibrator.OnCalibratorContinued(); });

			PhaseCounter = 0;
			SetStepCounter(PhaseCounter);

		}

		public void SetVisibility(bool isVisible) {
			CalibratorRoot.style.visibility = new StyleEnum<Visibility>(isVisible ? Visibility.Visible : Visibility.Hidden);
		}

		public void InitializeCalibration() {
			PhaseCounter = 1;
			CalibratorPhase = CalibratorPhase.Initial;
			SetStepCounter(PhaseCounter);
			SetPhaseText(CalibratorPhase.Initial);
		}

		public void OnCalibratorPhaseChanged(CalibratorPhase newPhase) {

			switch (CalibratorPhase) {
				case CalibratorPhase.Initial:
					//SetVisibility(true);
					SetPhaseText(CalibratorPhase.Initial);
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.NorthAlignStart:
					break;
				case CalibratorPhase.NorthAlignFinish:
					break;
				case CalibratorPhase.GroundStart:
					SetPhaseText(CalibratorPhase.GroundFinish);
					subCanvasGroup.alpha = 1;
					SetButtonEnabledStatus(false);
					break;
				case CalibratorPhase.GroundFinish:
					SetPhaseText(CalibratorPhase.End);
					subCanvasGroup.alpha = 0;
					break;
				case CalibratorPhase.RoomStart:
					break;
				case CalibratorPhase.RoomFinish:
					break;
				case CalibratorPhase.End:
					break;
				case CalibratorPhase.None:
					SetVisibility(false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

		}

		private void SetButtonEnabledStatus(bool isEnabled) {
			CalibratorButton.SetEnabled(isEnabled);
		}

		private void SetPhaseText(CalibratorPhase phase) {
			GuideLabel.text = CalibratorTextRepository.CalibratorGuideDict[phase];
			CalibratorButton.text = CalibratorTextRepository.CalibratorButtonTextDict[phase];
		}

		private void SetStepCounter(int nextPhaseCounter) {
			StepLabel.text = $"Step {nextPhaseCounter}/{numberOfPhases}";
		}

		public void OnRoomCalibrationProgressReceived(float progressValue) {

			calibrationProgressIndicator.fillAmount += progressValue;

			if (calibrationProgressIndicator.fillAmount >= 0.95f) {
				SetButtonEnabledStatus(true);
			}

		}

		private void OnDisable() {
			CalibratorButton.UnregisterCallback<ClickEvent>(e => { Calibrator.OnCalibratorContinued(); });
		}

#endregion

	}

}