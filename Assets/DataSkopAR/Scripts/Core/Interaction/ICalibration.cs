using System;

namespace DataskopAR.Interaction {

	public interface ICalibration {

		public event Action CalibrationCompleted;

		public bool IsEnabled { get; set; }

		public ICalibration Enable();

		public void Disable();

	}

}