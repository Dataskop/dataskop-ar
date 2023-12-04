using UnityEngine;

namespace DataskopAR {

	public class FPSManager : MonoBehaviour {

#region Methods

		private void Start() {
			Application.targetFrameRate = 30;
		}

#endregion

	}

}