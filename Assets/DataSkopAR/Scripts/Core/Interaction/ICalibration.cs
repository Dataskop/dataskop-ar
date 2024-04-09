using System;
namespace DataskopAR.Interaction {

	public interface ICalibration {

		public bool IsEnabled { get; set; }

		public event Action CalibrationCompleted;

		public ICalibration Enable();

		public void Disable();

	}

}