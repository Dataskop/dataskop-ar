using System.Collections.Generic;

namespace DataSkopAR.UI {

	public static class CalibratorTextRepository {

		public static readonly Dictionary<CalibratorPhase, string> CalibratorGuideDict = new() {
			{ CalibratorPhase.Initial, "To guarantee the best experience, please calibrate the application first!" },
			{ CalibratorPhase.GroundStart, "Please try to align the GREEN rectangle on the lowest point (the floor normally) where you are standing." },
			{ CalibratorPhase.GroundFinish, "Ground Calibrated! Now you can proceed to scan the surroundings to improve the AR application in general." },
			{ CalibratorPhase.End, "Done! Click on the 'Finish' button to continue to the main application." }
		};

		public static readonly Dictionary<CalibratorPhase, string> CalibratorButtonTextDict = new() {
			{ CalibratorPhase.Initial, "Start" },
			{ CalibratorPhase.GroundStart, "Continue" },
			{ CalibratorPhase.GroundFinish, "Continue" },
			{ CalibratorPhase.End, "Finish" }
		};

	}

}