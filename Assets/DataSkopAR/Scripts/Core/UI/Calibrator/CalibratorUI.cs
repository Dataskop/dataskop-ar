using System;
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

		public void OnCalibratorPhaseChanged(CalibratorPhase currentPhase) {

			switch (currentPhase) {
				case CalibratorPhase.Initial:
					SetVisibility(true);
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.NorthAlignStart:
					SetButtonEnabledStatus(false);
					break;
				case CalibratorPhase.NorthAlignFinish:
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.GroundStart:
					SetButtonEnabledStatus(false);
					break;
				case CalibratorPhase.GroundFinish:
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.RoomStart:
					subCanvasGroup.alpha = 1;
					SetButtonEnabledStatus(false);
					break;
				case CalibratorPhase.RoomFinish:
					SetButtonEnabledStatus(true);
					subCanvasGroup.alpha = 0;
					break;
				case CalibratorPhase.End:
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.None:
					SetVisibility(false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			SetPhaseText(currentPhase);

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