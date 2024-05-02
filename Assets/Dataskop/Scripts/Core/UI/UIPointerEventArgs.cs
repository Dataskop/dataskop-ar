using System;
using Dataskop.UI;
using UnityEngine;

namespace Dataskop {

	public class UIPointerEventArgs : EventArgs {

		public Vector2 localPointerPosition;
		public int pointerId;
		public UISection uiPointerSection;

		public UIPointerEventArgs(Vector2 pointerPos, UISection section, int id) {

			localPointerPosition = pointerPos;
			uiPointerSection = section;
			pointerId = id;

		}

	}

}