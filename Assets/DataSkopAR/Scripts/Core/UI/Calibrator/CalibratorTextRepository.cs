using System.Collections.Generic;
using DataskopAR.Interaction;

namespace DataskopAR.UI {

	public static class CalibratorTextRepository {

#region Constants
		
		// @formatter:off

		public static readonly Dictionary<CalibratorPhase, string> CalibratorGuideDict = new() {
			{ CalibratorPhase.Initial, "" }, // Currently unused phase
			{ CalibratorPhase.NorthAlignStart, "Welcome! Let's fine-tune your experience. Start the calibration now for optimal performance!\nFirst we are going to calibrate the north alignment." },
			{ CalibratorPhase.NorthAlignProcess, "Grabbing north rotation samples..." },
			{ CalibratorPhase.NorthAlignFinish, "" }, // Currently unused phase
			{ CalibratorPhase.GroundStart, "North alignment complete!\nLet's locate the ground in the next calibration phase." },
			{ CalibratorPhase.GroundProcess, "Find a green rectangle on the floor and tap on it when you see it aligned with the floor." },
			{ CalibratorPhase.GroundFinish, "" }, // Currently unused phase
			{ CalibratorPhase.RoomStart, "Ground calibration successful! \nTime to capture your surroundings in the next phase for better AR tracking!" },
			{ CalibratorPhase.RoomProcess, "Slowly scan the room with your device until the blue progress bar fills up all the way." },
			{ CalibratorPhase.RoomFinish, "" }, // Currently unused phase
			{ CalibratorPhase.End, "Room mapping is complete!\nHit 'Finish' to dive into the full application experience." },
			{ CalibratorPhase.None, "" }
		};

		public static readonly Dictionary<CalibratorPhase, string> CalibratorButtonTextDict = new() {
			{ CalibratorPhase.Initial, "Calibrate" },
			{ CalibratorPhase.NorthAlignStart, "Start" },
			{ CalibratorPhase.NorthAlignProcess, "" },
			{ CalibratorPhase.NorthAlignFinish, "Next" },
			{ CalibratorPhase.GroundStart, "Start" },
			{ CalibratorPhase.GroundProcess, "" },
			{ CalibratorPhase.GroundFinish, "Next" },
			{ CalibratorPhase.RoomStart, "Start" },
			{ CalibratorPhase.RoomProcess, "" },
			{ CalibratorPhase.RoomFinish, "Next" },
			{ CalibratorPhase.End, "Finish" },
			{ CalibratorPhase.None, "" }
		};
		
		// @formatter:on

#endregion

	}

}