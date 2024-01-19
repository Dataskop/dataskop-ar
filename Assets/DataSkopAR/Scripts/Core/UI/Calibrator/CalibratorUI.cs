using System;
using DataskopAR.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace DataskopAR.UI {

	public class CalibratorUI : MonoBehaviour {

#region Fields

		[Header("Events")]
		public UnityEvent onCalibratorPhaseStarted;
		public UnityEvent onGroundCalibrationStarted;
		public UnityEvent onGroundCalibrationFinished;
		public UnityEvent onCalibratorPhaseEnded;

		[Header("References")]
		[SerializeField] private UIDocument calibratorUiDoc;
		[SerializeField] private CanvasGroup subCanvasGroup;
		[SerializeField] private Image calibrationProgressIndicator;

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

#endregion

#region Methods

		private void OnEnable() {
			CalibratorRoot = calibratorUiDoc.rootVisualElement.Q<VisualElement>("CalibratorContainer");
			GuideLabel = CalibratorRoot.Q<Label>("GuideLabel");
			StepLabel = CalibratorRoot.Q<Label>("StepText");
			CalibratorButton = CalibratorRoot.Q<Button>("CalibratorButton");
			CalibratorButton.RegisterCallback<ClickEvent>(e => { TriggerNextPhase(); });
		}

		private void Start() {

			InitializeCalibration();

			if (DataPointsManager.IsDemoScene) {
				SkipCalibration();
			}

		}

		private void SkipCalibration() {
			ToggleCalibrator(false);
			CalibratorPhase = CalibratorPhase.End;
			TriggerNextPhase();
		}

		public void SkipCalibrationInput(InputAction.CallbackContext ctx) {
			if (ctx.performed) {
				if (CalibratorPhase != CalibratorPhase.End)
					SkipCalibration();
			}
		}

		public void ToggleCalibrator(bool isVisible) {
			CalibratorRoot.style.visibility = new StyleEnum<Visibility>(isVisible ? Visibility.Visible : Visibility.Hidden);
		}

		public void InitializeCalibration() {
			PhaseCounter = 1;
			CalibratorPhase = CalibratorPhase.Initial;
			SetStepCounter(PhaseCounter);
			SetPhaseText(CalibratorPhase.Initial);
			ToggleCalibrator(true);
			onCalibratorPhaseStarted?.Invoke();
		}

		public void TriggerNextPhase() {

			SetStepCounter(PhaseCounter++);

			switch (CalibratorPhase) {
				case CalibratorPhase.Initial:
					SetPhaseText(CalibratorPhase.GroundStart);
					SetButtonEnabledStatus(false);
					CalibratorPhase = CalibratorPhase.GroundStart;
					onGroundCalibrationStarted?.Invoke();
					break;
				case CalibratorPhase.GroundStart:
					SetPhaseText(CalibratorPhase.GroundFinish);
					CalibratorPhase = CalibratorPhase.GroundFinish;
					onGroundCalibrationFinished?.Invoke();
					subCanvasGroup.alpha = 1;
					SetButtonEnabledStatus(false);
					break;
				case CalibratorPhase.GroundFinish:
					SetPhaseText(CalibratorPhase.End);
					CalibratorPhase = CalibratorPhase.End;
					subCanvasGroup.alpha = 0;
					break;
				case CalibratorPhase.End:
					onCalibratorPhaseEnded?.Invoke();
					ToggleCalibrator(false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

		}

		public void SetButtonEnabledStatus(bool isEnabled) {
			CalibratorButton.SetEnabled(isEnabled);
		}

		private void SetPhaseText(CalibratorPhase phase) {
			SetGuideText(CalibratorTextRepository.CalibratorGuideDict[phase]);
			SetButtonText(CalibratorTextRepository.CalibratorButtonTextDict[phase]);
		}

		private void SetGuideText(string guideText) {
			GuideLabel.text = guideText;
		}

		private void SetButtonText(string buttonText) {
			CalibratorButton.text = buttonText;
		}

		private void SetStepCounter(int nextPhaseCounter) {
			StepLabel.text = $"Step {nextPhaseCounter}/{numberOfPhases}";
		}

		public void OnRoomCalibrationProgressReceived(float progressValue) {

			calibrationProgressIndicator.fillAmount += progressValue;

			Debug.Log(progressValue);

			if (calibrationProgressIndicator.fillAmount >= 0.95f) {
				SetButtonEnabledStatus(true);
			}

		}

		private void OnDisable() {
			CalibratorButton.UnregisterCallback<ClickEvent>(e => { TriggerNextPhase(); });
		}

#endregion

	}

}