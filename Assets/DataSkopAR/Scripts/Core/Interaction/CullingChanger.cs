using UnityEngine;

namespace DataSkopAR.Interaction {

	public class CullingChanger : MonoBehaviour {

#region Fields

		[SerializeField] private Camera arCamera;

#endregion

#region Methods

		public void ToggleCullingLayer(string layerName) {
			arCamera.cullingMask ^= 1 << LayerMask.NameToLayer(layerName);
		}

#endregion

	}

}