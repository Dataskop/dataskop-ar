using System.Collections;
using System.Collections.Generic;
using Dataskop.Data;
using UnityEngine;

namespace Dataskop {

	public class InfoCardMapRadius : MonoBehaviour {

		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private DataPointsManager dataPointsManager;
		
		private void Start() {
			spriteRenderer.enabled = false;
		}

		public void OnProjectLoaded() {
			spriteRenderer.enabled = true;
			
			float diameter = dataPointsManager.NearbyDevicesDistance * 2;
			spriteRenderer.transform.localScale = new Vector3(diameter, diameter, 1);
		}
	}

}