using UnityEngine;
namespace DataskopAR {

	public static class FPSManager {

#region Methods

		public static void SetApplicationTargetFrameRate(int targetFrameRate) {
			Application.targetFrameRate = targetFrameRate;
		}

#endregion

	}

}