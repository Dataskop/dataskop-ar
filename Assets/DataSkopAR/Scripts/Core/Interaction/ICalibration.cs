namespace DataskopAR.Interaction {

	public interface ICalibration {

		public bool IsEnabled { get; set; }

		public ICalibration Enable();

		public void Disable();

	}

}