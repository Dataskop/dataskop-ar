using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Dataskop {

	[RequireComponent(typeof(AROcclusionManager))]
	public class OcclusionHandler : MonoBehaviour {

		private AROcclusionManager arOcclusionManager;

		private void Awake() {
			arOcclusionManager = GetComponent<AROcclusionManager>();
		}

		public void ToggleOcclusion() {

			if (arOcclusionManager.descriptor == null ||
			    arOcclusionManager.descriptor.environmentDepthImageSupported == Supported.Unsupported) {
				return;
			}

			arOcclusionManager.requestedEnvironmentDepthMode = arOcclusionManager.currentEnvironmentDepthMode switch {
				EnvironmentDepthMode.Disabled => EnvironmentDepthMode.Medium,
				EnvironmentDepthMode.Medium => EnvironmentDepthMode.Disabled,
				_ => arOcclusionManager.requestedEnvironmentDepthMode
			};

		}

	}

}