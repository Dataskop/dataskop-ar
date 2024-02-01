using System.Collections.Generic;
using DataskopAR.Interaction;

namespace DataskopAR.UI {

	public static class CalibratorTextRepository {

#region Constants
		
		// @formatter:off

		public static readonly Dictionary<CalibratorPhase, string> CalibratorGuideDict = new() {
			{ CalibratorPhase.Initial, "" }, // Currently unused phase
			{ CalibratorPhase.NorthAlignStart, "Welcome! Let's fine-tune your experience. Start the calibration now for optimal performance! First we are going to calibrate the north alignment." },
			{ CalibratorPhase.NorthAlignProcess, "Grabbing north rotation samples..." },
			{ CalibratorPhase.NorthAlignFinish, "" }, // Currently unused phase
			{ CalibratorPhase.GroundStart, "North alignment complete! Let's locate the ground in the next calibration phase." },
			{ CalibratorPhase.GroundProcess, "Find a green rectangle on the floor! Tap on it when you see it aligned with the floor." },
			{ CalibratorPhase.GroundFinish, "" }, // Currently unused phase
			{ CalibratorPhase.RoomStart, "Ground calibration successful! You're all set for the next phase. Time to capture your surroundings for better AR tracking!" },
			{ CalibratorPhase.RoomProcess, "Slowly scan the room with your device until the blue progress bar fills up all the way." },
			{ CalibratorPhase.RoomFinish, "" }, // Currently unused phase
			{ CalibratorPhase.End, "Room mapping is complete! Hit 'Finish' to dive into the full application experience." },
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