using System;
using DataskopAR.Interaction;
using DataskopAR.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace DataskopAR.UI {

	public class CalibratorUI : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private UIDocument calibratorUiDoc;
		[SerializeField] private Calibrator calibrator;

		[Header("Values")]
		[SerializeField] private int numberOfPhases;

#endregion

#region Properties

		private VisualElement CalibratorRoot { get; set; }

		private Label GuideLabel { get; set; }

		private Label StepLabel { get; set; }

		private Button CalibratorButton { get; set; }

		private VisualElement ProgressIndicatorContainer { get; set; }

		private VisualElement NorthAlignmentProgressBar { get; set; }
		private VisualElement NorthAlignmentProgressContainer { get; set; }
		private VisualElement RoomScanContainer { get; set; }
		private VisualElement RoomScanProgress { get; set; }

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

			ProgressIndicatorContainer = CalibratorRoot.Q<VisualElement>("ProgressIndicatorContainer");

			NorthAlignmentProgressBar = CalibratorRoot.Q<VisualElement>("NorthAlignmentProgress");
			NorthAlignmentProgressContainer = CalibratorRoot.Q<VisualElement>("NorthAlignmentContainer");
			RoomScanContainer = CalibratorRoot.Q<VisualElement>("RoomScanContainer");
			RoomScanProgress = CalibratorRoot.Q<VisualElement>("RoomScanProgress");

			PhaseCounter = 0;
			SetStepCounter(PhaseCounter);

		}

		public void SetVisibility(bool isVisible) {
			CalibratorRoot.style.visibility = new StyleEnum<Visibility>(isVisible ? Visibility.Visible : Visibility.Hidden);
		}

		public void OnCalibratorPhaseChanged(CalibratorPhase currentPhase) {

			switch (currentPhase) {
				case CalibratorPhase.Initial:
					StepLabel.visible = false;
					SetVisibility(true);
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.NorthAlignStart:
					StepLabel.visible = true;
					SetStepCounter(1);
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.NorthAlignProcess:
					SetProgressIndicatorStatus(true);
					NorthAlignmentProgressBar.visible = true;
					NorthAlignmentProgressContainer.visible = true;
					SetButtonEnabledStatus(false);
					break;
				case CalibratorPhase.NorthAlignFinish:
					NorthAlignmentProgressBar.visible = false;
					NorthAlignmentProgressContainer.visible = false;
					SetProgressIndicatorStatus(false);
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.GroundStart:
					SetStepCounter(1);
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.GroundProcess:
					SetButtonEnabledStatus(false);
					break;
				case CalibratorPhase.GroundFinish:
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.RoomStart:
					SetStepCounter(1);
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.RoomProcess:
					SetProgressIndicatorStatus(true);
					RoomScanProgress.visible = true;
					RoomScanContainer.visible = true;
					SetButtonEnabledStatus(false);
					break;
				case CalibratorPhase.RoomFinish:
					RoomScanProgress.visible = false;
					RoomScanContainer.visible = false;
					SetProgressIndicatorStatus(false);
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.End:
					StepLabel.visible = false;
					SetButtonEnabledStatus(true);
					break;
				case CalibratorPhase.None:
					SetButtonEnabledStatus(false);
					SetVisibility(false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			SetPhaseText(currentPhase);

		}

		public void SetButtonEnabledStatus(bool isEnabled) {
			CalibratorButton.visible = isEnabled;
			CalibratorButton.style.display = new StyleEnum<DisplayStyle>(isEnabled ? DisplayStyle.Flex : DisplayStyle.None);
		}

		private void SetProgressIndicatorStatus(bool isEnabled) {
			ProgressIndicatorContainer.visible = isEnabled;
			ProgressIndicatorContainer.style.display = new StyleEnum<DisplayStyle>(isEnabled ? DisplayStyle.Flex : DisplayStyle.None);
		}

		private void SetPhaseText(CalibratorPhase phase) {
			GuideLabel.text = CalibratorTextRepository.CalibratorGuideDict[phase];
			CalibratorButton.text = CalibratorTextRepository.CalibratorButtonTextDict[phase];
		}

		private void SetStepCounter(int nextPhaseCounter) {
			PhaseCounter += nextPhaseCounter;
			StepLabel.text = $"Phase {PhaseCounter}/{numberOfPhases}";
		}

		public void OnRoomCalibrationProgressReceived(float progressValue) {
			RoomScanProgress.style.scale = new Scale(new Vector2(1, progressValue));
		}

		public void OnNorthRotationSampleReceived(int currentSamples, int maxSamples) {
			NorthAlignmentProgressBar.style.scale = new Scale(new Vector2(MathExtensions.Map01(currentSamples, 0, maxSamples), 1));
		}

		private void OnDisable() {
			CalibratorButton.UnregisterCallback<ClickEvent>(e => { Calibrator.OnCalibratorContinued(); });
		}

#endregion

	}

}