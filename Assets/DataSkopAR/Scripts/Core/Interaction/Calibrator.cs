using System;
using UnityEngine;
using UnityEngine.Events;

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

		public void Initiate() {
			calibrationInitialized?.Invoke();
			Phase = CalibratorPhase.Initial;
		}

		public void OnCalibratorProgressButtonPressed() {

			switch (Phase) {

				case CalibratorPhase.Initial:
					Phase = CalibratorPhase.GroundStart;
					break;
				case CalibratorPhase.GroundStart:
					Phase = CalibratorPhase.GroundFinish;
					break;
				case CalibratorPhase.GroundFinish:
					Phase = CalibratorPhase.RoomStart;
					break;
				case CalibratorPhase.RoomStart:
					Phase = CalibratorPhase.RoomFinish;
					break;
				case CalibratorPhase.RoomFinish:
					Phase = CalibratorPhase.End;
					break;
				case CalibratorPhase.End:
					Phase = CalibratorPhase.None;
					break;
				default:
					throw new ArgumentOutOfRangeException();

			}

		}

		public void OnCalibrationStepCompleted() { }

#endregion

	}

}