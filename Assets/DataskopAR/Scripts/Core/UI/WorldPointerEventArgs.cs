using System;
using UnityEngine;

namespace DataskopAR {

	public class WorldPointerEventArgs : EventArgs {

		public Vector2 screenPosition;
		public int pointerId;

		public WorldPointerEventArgs(Vector2 pointerPos, int id) {

			screenPosition = pointerPos;
			pointerId = id;

		}

	}

}