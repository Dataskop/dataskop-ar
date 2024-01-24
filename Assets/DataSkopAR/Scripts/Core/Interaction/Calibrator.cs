using System;
using System.Collections;
using DataskopAR.Data;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace DataskopAR.Interaction {

	public class Calibrator : MonoBehaviour {

#region Events

		public UnityEvent calibrationInitialized;
		public UnityEvent<CalibratorPhase> phaseChanged;
		public UnityEvent calibrationStepCompleted;
		public UnityEvent calibrationFinished;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private GroundLevelCalibrator groundLevelCalibrator;
		[SerializeField] private NorthAlignmentCalibrator northAlignmentCalibrator;
		[SerializeField] private RoomCalibrator roomCalibrator;

		private CalibratorPhase currentPhase;

#endregion

#region Properties

		public ICalibration ActiveCalibration { get; private set; }

		/// <summary>
		/// The current Calibration Phase.
		/// </summary>
		public CalibratorPhase CurrentPhase {
			get => currentPhase;
			private set {
				currentPhase = value;
				phaseChanged?.Invoke(CurrentPhase);
			}
		}

		public bool IsCalibrating { get; private set; }

#endregion

#region Methods

		private void OnEnable() {
			northAlignmentCalibrator.CalibrationCompleted += OnCalibratorContinued;
			groundLevelCalibrator.CalibrationCompleted += OnCalibratorContinued;
			roomCalibrator.CalibrationCompleted += OnCalibratorContinued;
		}

		private void Start() {
			Initialize();
		}

		public void Initialize() {

			if (DataPointsManager.IsDemoScene) {
				return;
			}

			if (ActiveCalibration != null) {
				return;
			}

			calibrationInitialized?.Invoke();
			CurrentPhase = CalibratorPhase.Initial;
			IsCalibrating = true;

		}

		public void OnCalibratorContinued() {

			switch (CurrentPhase) {

				case CalibratorPhase.Initial:
					CurrentPhase = CalibratorPhase.NorthAlignStart;
					break;
				case CalibratorPhase.NorthAlignStart:
					CurrentPhase = CalibratorPhase.NorthAlignProcess;
					ActiveCalibration = northAlignmentCalibrator.Enable();
					break;
				case CalibratorPhase.NorthAlignProcess:
					CurrentPhase = CalibratorPhase.NorthAlignFinish;
					ActiveCalibration.Disable();
					break;
				case CalibratorPhase.NorthAlignFinish:
					CurrentPhase = CalibratorPhase.GroundStart;
					break;
				case CalibratorPhase.GroundStart:
					CurrentPhase = CalibratorPhase.GroundProcess;
					ActiveCalibration = groundLevelCalibrator.Enable();
					break;
				case CalibratorPhase.GroundProcess:
					CurrentPhase = CalibratorPhase.GroundFinish;
					ActiveCalibration.Disable();
					break;
				case CalibratorPhase.GroundFinish:
					CurrentPhase = CalibratorPhase.RoomStart;
					break;
				case CalibratorPhase.RoomStart:
					CurrentPhase = CalibratorPhase.RoomProcess;
					ActiveCalibration = roomCalibrator.Enable();
					break;
				case CalibratorPhase.RoomProcess:
					CurrentPhase = CalibratorPhase.RoomFinish;
					ActiveCalibration.Disable();
					break;
				case CalibratorPhase.RoomFinish:
					CurrentPhase = CalibratorPhase.End;
					break;
				case CalibratorPhase.End:
					CurrentPhase = CalibratorPhase.None;
					calibrationFinished?.Invoke();
					ActiveCalibration = null;
					IsCalibrating = false;
					break;
				case CalibratorPhase.None:
					break;
				default:
					throw new ArgumentOutOfRangeException();

			}

		}

		public void SkipCalibrationInputReceived(InputAction.CallbackContext ctx) {

			if (ctx.performed) {

				if (!IsCalibrating) {
					return;
				}

				if (ActiveCalibration != null) {
					ActiveCalibration.Disable();
					ActiveCalibration = null;
				}

				CurrentPhase = CalibratorPhase.None;
				calibrationFinished?.Invoke();
				IsCalibrating = false;

			}

		}

		private void OnDisable() {
			northAlignmentCalibrator.CalibrationCompleted -= OnCalibratorContinued;
			groundLevelCalibrator.CalibrationCompleted -= OnCalibratorContinued;
			roomCalibrator.CalibrationCompleted -= OnCalibratorContinued;
		}

#endregion

	}

}