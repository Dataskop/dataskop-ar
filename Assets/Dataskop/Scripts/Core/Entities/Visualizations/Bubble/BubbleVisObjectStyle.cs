using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	[CreateAssetMenu(fileName = "Options", menuName = "VisOptions/Add Bubble Option...", order = 2)]
	public class BubbleVisObjectStyle : ScriptableObject, IVisObjectStyle {

		public VisObjectStyle[] styles;

		public VisObjectStyle[] Styles => styles;

	}

}