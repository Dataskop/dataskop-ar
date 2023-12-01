using UnityEngine;

namespace DataSkopAR {

	public class FPSManager : MonoBehaviour {

#region Methods

		private void Start() {
			Application.targetFrameRate = 30;
		}

#endregion

	}

}