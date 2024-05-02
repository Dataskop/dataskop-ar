using System;
using UnityEngine;

namespace Dataskop {

	public class WorldPointerEventArgs : EventArgs {

		public int pointerId;

		public Vector2 screenPosition;

		public WorldPointerEventArgs(Vector2 pointerPos, int id) {

			screenPosition = pointerPos;
			pointerId = id;

		}

	}

}