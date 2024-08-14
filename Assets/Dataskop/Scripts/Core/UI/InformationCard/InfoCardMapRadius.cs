using System.Collections;
using System.Collections.Generic;
using Dataskop.Data;
using UnityEngine;

namespace Dataskop {

	public class InfoCardMapRadius : MonoBehaviour {

		[SerializeField] private SpriteRenderer spriteRenderer;

		public void OnProjectLoaded() {
			spriteRenderer.enabled = true;
		}

	}

}