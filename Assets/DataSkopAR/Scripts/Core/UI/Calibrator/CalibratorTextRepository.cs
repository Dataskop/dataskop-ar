using System.Collections.Generic;
using DataskopAR.Interaction;

namespace DataskopAR.UI {

	public static class CalibratorTextRepository {

#region Constants
		
		// @formatter:off

		public static readonly Dictionary<CalibratorPhase, string> CalibratorGuideDict = new() {
			{ CalibratorPhase.Initial, "Welcome! Let's fine-tune your experience. Start the calibration now for optimal performance!" },
			{ CalibratorPhase.NorthAlignStart, "Ready to align? Turn with your device until the compass lines up with North." },
			{ CalibratorPhase.NorthAlignFinish, "North alignment complete! Ready for the next step." },
			{ CalibratorPhase.GroundStart, "Grounding your view. Align the GREEN rectangle with the floor at your feet to continue." },
			{ CalibratorPhase.GroundFinish, "Ground calibration successful! You're all set for the next stage." },
			{ CalibratorPhase.RoomStart, "Time to capture your surroundings! Slowly scan the room with your device. A progress bar will fill up as the scan improves - the fuller, the better." },
			{ CalibratorPhase.RoomFinish, "Room mapping is complete! Just one more step to go." },
			{ CalibratorPhase.End, "All set! Hit 'Finish' to dive into the full application experience." }
		};

		public static readonly Dictionary<CalibratorPhase, string> CalibratorButtonTextDict = new() {
			{ CalibratorPhase.Initial, "Start" },
			{ CalibratorPhase.NorthAlignStart, "Continue" },
			{ CalibratorPhase.NorthAlignFinish, "Continue" },
			{ CalibratorPhase.GroundStart, "Continue" },
			{ CalibratorPhase.GroundFinish, "Continue" },
			{ CalibratorPhase.RoomStart, "Continue" },
			{ CalibratorPhase.RoomFinish, "Continue" },
			{ CalibratorPhase.End, "Finish" }
		};
		
		// @formatter:on

#endregion

	}

}