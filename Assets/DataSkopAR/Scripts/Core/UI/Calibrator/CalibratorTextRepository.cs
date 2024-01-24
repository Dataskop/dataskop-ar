using System.Collections.Generic;
using DataskopAR.Interaction;

namespace DataskopAR.UI {

	public static class CalibratorTextRepository {

#region Constants
		
		// @formatter:off

		public static readonly Dictionary<CalibratorPhase, string> CalibratorGuideDict = new() {
			{ CalibratorPhase.Initial, "Welcome! Let's fine-tune your experience. Start the calibration now for optimal performance!" },
			{ CalibratorPhase.NorthAlignStart, "First we are going to calibrate the north alignment." },
			{ CalibratorPhase.NorthAlignProcess, "Grabbing north rotation samples..." },
			{ CalibratorPhase.NorthAlignFinish, "North alignment complete! Continue to the next step." },
			{ CalibratorPhase.GroundStart, "Grounding your view in the next calibration step." },
			{ CalibratorPhase.GroundProcess, "Please align the GREEN rectangle with the floor at your feet to continue." },
			{ CalibratorPhase.GroundFinish, "Ground calibration successful! You're all set for the next step." },
			{ CalibratorPhase.RoomStart, "Time to capture your surroundings for better AR tracking!" },
			{ CalibratorPhase.RoomProcess, "Slowly scan the room with your device until the blue progress bar fills up all the way." },
			{ CalibratorPhase.RoomFinish, "Room mapping is complete! Please continue to the next step." },
			{ CalibratorPhase.End, "All set! Hit 'Finish' to dive into the full application experience." },
			{ CalibratorPhase.None, "" }
		};

		public static readonly Dictionary<CalibratorPhase, string> CalibratorButtonTextDict = new() {
			{ CalibratorPhase.Initial, "Start" },
			{ CalibratorPhase.NorthAlignStart, "Start" },
			{ CalibratorPhase.NorthAlignProcess, "" },
			{ CalibratorPhase.NorthAlignFinish, "Continue" },
			{ CalibratorPhase.GroundStart, "Start" },
			{ CalibratorPhase.GroundProcess, "" },
			{ CalibratorPhase.GroundFinish, "Continue" },
			{ CalibratorPhase.RoomStart, "Start" },
			{ CalibratorPhase.RoomProcess, "" },
			{ CalibratorPhase.RoomFinish, "Continue" },
			{ CalibratorPhase.End, "Finish" },
			{ CalibratorPhase.None, "" }
		};
		
		// @formatter:on

#endregion

	}

}