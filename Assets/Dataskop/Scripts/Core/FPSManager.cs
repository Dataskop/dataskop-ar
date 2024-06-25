using UnityEngine;

namespace Dataskop {

	public static class FPSManager {

		public static void SetApplicationTargetFrameRate(int targetFrameRate) {
			Application.targetFrameRate = targetFrameRate;
		}

	}

}