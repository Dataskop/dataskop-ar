using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace DataskopAR.Interaction {

	public class Calibrator : MonoBehaviour {

#region Events

		public UnityEvent calibrationInitialized;
		public UnityEvent<CalibratorPhase> phaseChanged;
		public UnityEvent calibrationFinished;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private GroundLevelCalibrator groundLevelCalibrator;
		[SerializeField] private NorthAlignmentCalibrator northAlignmentCalibrator;
		[SerializeField] private RoomCalibrator roomCalibrator;

		private CalibratorPhase phase;

#endregion

#region Properties

		public ICalibration ActiveCalibration { get; private set; }

		/// <summary>
		/// The current Calibration Phase.
		/// </summary>
		public CalibratorPhase Phase {
			get => phase;
			private set {
				phase = value;
				phaseChanged?.Invoke(Phase);
			}
		}

#endregion

#region Methods

		private void Start() {
			Initialize();
		}

		public void Initialize() {

			if (ActiveCalibration == null) {
				calibrationInitialized?.Invoke();
				Phase = CalibratorPhase.Initial;
			}

		}

		public void OnCalibratorContinued() {

			switch (Phase) {

				case CalibratorPhase.Initial:
					Phase = CalibratorPhase.NorthAlignStart;
					ActiveCalibration = northAlignmentCalibrator.Enable();
					break;
				case CalibratorPhase.NorthAlignStart:
					Phase = CalibratorPhase.NorthAlignFinish;
					ActiveCalibration.Disable();
					break;
				case CalibratorPhase.NorthAlignFinish:
					Phase = CalibratorPhase.GroundStart;
					ActiveCalibration = groundLevelCalibrator.Enable();
					break;
				case CalibratorPhase.GroundStart:
					Phase = CalibratorPhase.GroundFinish;
					ActiveCalibration.Disable();
					break;
				case CalibratorPhase.GroundFinish:
					Phase = CalibratorPhase.RoomStart;
					ActiveCalibration = roomCalibrator.Enable();
					break;
				case CalibratorPhase.RoomStart:
					Phase = CalibratorPhase.RoomFinish;
					ActiveCalibration.Disable();
					break;
				case CalibratorPhase.RoomFinish:
					Phase = CalibratorPhase.End;
					ActiveCalibration = null;
					break;
				case CalibratorPhase.End:
					Phase = CalibratorPhase.None;
					calibrationFinished?.Invoke();
					break;
				default:
					throw new ArgumentOutOfRangeException();

			}

		}

		public void SkipCalibrationInputReceived(InputAction.CallbackContext ctx) {

			if (ctx.performed) {

				if (ActiveCalibration == null) {
					return;
				}

				ActiveCalibration.Disable();
				ActiveCalibration = null;
				Phase = CalibratorPhase.None;
				calibrationFinished?.Invoke();

			}

		}

		public void OnCalibrationStepCompleted() { }

#endregion

	}

}