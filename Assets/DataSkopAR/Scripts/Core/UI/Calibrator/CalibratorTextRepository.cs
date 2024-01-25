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
			{ CalibratorPhase.GroundStart, "Let's locate the ground in the next calibration step." },
			{ CalibratorPhase.GroundProcess, "Point your view in the direction of the floor and tap on the green rectangle that will appear on it! Make sure the rectangle is at the height of your floor." },
			{ CalibratorPhase.GroundFinish, "Ground calibration successful! You're all set for the next step." },
			{ CalibratorPhase.RoomStart, "Time to capture your surroundings for better AR tracking!" },
			{ CalibratorPhase.RoomProcess, "Slowly scan the room with your device until the blue progress bar fills up all the way." },
			{ CalibratorPhase.RoomFinish, "Room mapping is complete! Please continue to the next step." },
			{ CalibratorPhase.End, "All set! Hit 'Finish' to dive into the full application experience." },
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