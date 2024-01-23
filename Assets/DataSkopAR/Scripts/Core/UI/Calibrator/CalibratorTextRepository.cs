using System.Collections.Generic;
using DataskopAR.Interaction;

namespace DataskopAR.UI {

	public static class CalibratorTextRepository {

		public static readonly Dictionary<CalibratorPhase, string> CalibratorGuideDict = new() {
			{ CalibratorPhase.Initial, "To guarantee the best experience, please calibrate the application first!" },
			{ CalibratorPhase.NorthAlignStart, "" },
			{ CalibratorPhase.NorthAlignFinish, "" },
			{ CalibratorPhase.GroundStart, "Please try to align the GREEN rectangle on the lowest point (the floor normally) where you are standing." },
			{ CalibratorPhase.GroundFinish, "Ground Calibrated!" },
			{ CalibratorPhase.RoomStart, ""},
			{ CalibratorPhase.RoomFinish, ""},
			{ CalibratorPhase.End, "Done! Click on the 'Finish' button to continue to the main application." }
		};

		public static readonly Dictionary<CalibratorPhase, string> CalibratorButtonTextDict = new() {
			{ CalibratorPhase.Initial, "Start" },
			{ CalibratorPhase.NorthAlignStart, "Continue" },
			{ CalibratorPhase.NorthAlignFinish, "Continue" },
			{ CalibratorPhase.GroundStart, "Continue" },
			{ CalibratorPhase.GroundFinish, "Continue" },
			{ CalibratorPhase.RoomStart, "Continue"},
			{ CalibratorPhase.RoomFinish, "Continue"},
			{ CalibratorPhase.End, "Finish" }
		};

	}

}