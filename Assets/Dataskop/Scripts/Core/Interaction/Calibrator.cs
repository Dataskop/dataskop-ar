using System;
using Dataskop.Data;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

namespace Dataskop.Interaction {

	public class Calibrator : MonoBehaviour {

		[Header("UI Events")]
		public UnityEvent calibrationInitialized;

		public UnityEvent<CalibratorPhase> phaseChanged;
		public UnityEvent calibrationFinished;

		[Header("References")]
		[SerializeField] private GroundLevelCalibrator groundLevelCalibrator;

		[SerializeField] private NorthAlignmentCalibrator northAlignmentCalibrator;
		[SerializeField] private RoomCalibrator roomCalibrator;

		private CalibratorPhase currentPhase;

		[CanBeNull] public ICalibration ActiveCalibration { get; private set; }

		/// <summary>
		///     The current Calibration Phase.
		/// </summary>
		public CalibratorPhase CurrentPhase {
			get => currentPhase;

			private set {
				currentPhase = value;
				phaseChanged?.Invoke(CurrentPhase);
			}
		}

		public bool IsCalibrating { get; private set; }

		private void Awake() {
			FPSManager.SetApplicationTargetFrameRate(30);
		}

		private void Start() {
			Initialize();
		}

		private void OnEnable() {
			northAlignmentCalibrator.CalibrationCompleted += OnCalibratorContinued;
			groundLevelCalibrator.CalibrationCompleted += OnCalibratorContinued;
			roomCalibrator.CalibrationCompleted += OnCalibratorContinued;

		}

		private void OnDisable() {
			northAlignmentCalibrator.CalibrationCompleted -= OnCalibratorContinued;
			groundLevelCalibrator.CalibrationCompleted -= OnCalibratorContinued;
			roomCalibrator.CalibrationCompleted -= OnCalibratorContinued;
		}

		public void Initialize() {

			if (AppOptions.DemoMode) {
				ARTrackedImageManager arManager = (ARTrackedImageManager)FindObjectOfType(typeof(ARTrackedImageManager), true);
				arManager.enabled = true;
				DemoBoxHandler demoBoxHandler = (DemoBoxHandler)FindObjectOfType(typeof(DemoBoxHandler), true);
				demoBoxHandler.enabled = true;
				calibrationFinished?.Invoke();
				return;
			}

			if (ActiveCalibration != null) {
				return;
			}

			calibrationInitialized?.Invoke();
			CurrentPhase = CalibratorPhase.NorthAlignStart;
			IsCalibrating = true;

		}

		public void OnCalibratorContinued() {

			switch (CurrentPhase) {

				case CalibratorPhase.Initial:
					CurrentPhase = CalibratorPhase.NorthAlignStart;
					break;
				case CalibratorPhase.NorthAlignStart:
					CurrentPhase = CalibratorPhase.NorthAlignProcess;

					if (northAlignmentCalibrator != null) {
						ActiveCalibration = northAlignmentCalibrator.Enable();
					}

					break;
				case CalibratorPhase.NorthAlignProcess:
					CurrentPhase = CalibratorPhase.GroundStart;
					ActiveCalibration?.Disable();
					break;
				case CalibratorPhase.NorthAlignFinish:
					CurrentPhase = CalibratorPhase.GroundStart;
					break;
				case CalibratorPhase.GroundStart:
					CurrentPhase = CalibratorPhase.GroundProcess;

					if (northAlignmentCalibrator != null) {
						ActiveCalibration = groundLevelCalibrator.Enable();
					}

					break;
				case CalibratorPhase.GroundProcess:
					CurrentPhase = CalibratorPhase.RoomStart;
					ActiveCalibration?.Disable();
					break;
				case CalibratorPhase.GroundFinish:
					CurrentPhase = CalibratorPhase.RoomStart;
					break;
				case CalibratorPhase.RoomStart:
					CurrentPhase = CalibratorPhase.RoomProcess;

					if (northAlignmentCalibrator != null) {
						ActiveCalibration = roomCalibrator.Enable();
					}

					break;
				case CalibratorPhase.RoomProcess:
					CurrentPhase = CalibratorPhase.End;
					ActiveCalibration?.Disable();
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

				ActiveCalibration?.Disable();
				CurrentPhase = CalibratorPhase.End;

			}

		}

	}

}