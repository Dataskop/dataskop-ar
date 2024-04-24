using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace DataskopAR {

	[RequireComponent(typeof(AROcclusionManager))]
	public class OcclusionHandler : MonoBehaviour {

#region Fields

		private AROcclusionManager arOcclusionManager;

#endregion

#region Methods

		private void Awake() {
			arOcclusionManager = GetComponent<AROcclusionManager>();
		}

		public void ToggleOcclusion() {
			
			Debug.Log(arOcclusionManager.currentEnvironmentDepthMode);

			if (arOcclusionManager.descriptor?.environmentDepthImageSupported == Supported.Unsupported)
				return;

			arOcclusionManager.enabled = !arOcclusionManager.enabled;
		}

#endregion

	}

}