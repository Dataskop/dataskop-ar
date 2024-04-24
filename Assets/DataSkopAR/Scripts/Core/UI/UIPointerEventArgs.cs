using System;
using DataskopAR.UI;
using UnityEngine;

namespace DataskopAR {

	public class UIPointerEventArgs : EventArgs {

		public Vector2 localPointerPosition;
		public UISection uiPointerSection;
		public int pointerId;

		public UIPointerEventArgs(Vector2 pointerPos, UISection section, int id) {

			localPointerPosition = pointerPos;
			uiPointerSection = section;
			pointerId = id;

		}

	}

}