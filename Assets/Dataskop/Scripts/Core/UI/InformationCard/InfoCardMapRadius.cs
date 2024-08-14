using System.Collections;
using System.Collections.Generic;
using Dataskop.Data;
using UnityEngine;

namespace Dataskop {

	public class InfoCardMapRadius : MonoBehaviour {

		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private DataPointsManager dataPointsManager;

		private float diameter;
		
		private void Awake() {
			spriteRenderer.enabled = false;
			
			diameter = dataPointsManager.NearbyDevicesDistance * 2;
		}

		public void OnProjectLoaded() {
			spriteRenderer.enabled = true;
			spriteRenderer.transform.localScale = new Vector3(diameter, diameter, 1);
		}
	}

}