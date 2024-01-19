using System;
using DataskopAR.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace DataskopAR.Interaction {

	public class Calibrator : MonoBehaviour {

#region Events

		public UnityEvent CalibrationInitialized;
		public UnityEvent<CalibratorPhase> PhaseChanged;

#endregion

#region Fields

		[Header("References")]
		[SerializeField] private GroundLevelCalibrator groundLevelCalibrator;
		[SerializeField] private NorthAlignmentCalibrator northAlignmentCalibrator;
		[SerializeField] private RoomCalibrator roomCalibrator;

		private CalibratorPhase phase;

#endregion

#region Properties

		/// <summary>
		/// The current Calibration Phase.
		/// </summary>
		public CalibratorPhase Phase {
			get => phase;
			private set {
				phase = value;
				PhaseChanged?.Invoke(Phase);
			}
		}

#endregion

#region Methods

		public void Initiate() {
			CalibrationInitialized?.Invoke();
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

		public void OnCalibrationStepCompleted() {
			
		}

#endregion

	}

}