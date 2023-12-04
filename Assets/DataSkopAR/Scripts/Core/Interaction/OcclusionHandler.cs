using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace DataskopAR {

	public class OcclusionHandler : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private AROcclusionManager arOcclusionManager;

#endregion

#region Methods

		public void ToggleOcclusion() {

			if (arOcclusionManager.requestedEnvironmentDepthMode == EnvironmentDepthMode.Disabled) {
				arOcclusionManager.requestedEnvironmentDepthMode = EnvironmentDepthMode.Fastest;
				return;
			}

			arOcclusionManager.requestedEnvironmentDepthMode = EnvironmentDepthMode.Disabled;

		}

		public void OcclusionEnabler() {
			arOcclusionManager.requestedEnvironmentDepthMode = EnvironmentDepthMode.Fastest;
		}

		public void OcclusionDisabler() {
			arOcclusionManager.requestedEnvironmentDepthMode = EnvironmentDepthMode.Disabled;
		}

#endregion

	}

}