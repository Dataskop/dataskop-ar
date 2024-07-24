using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	[CreateAssetMenu(fileName = "Options", menuName = "VisOptions/Add Dot Option...", order = 1)]
	public class DotVisObjectStyle : ScriptableObject, IVisObjectStyle {

		public VisObjectStyle[] styles;

		public VisObjectStyle[] Styles => styles;

	}

}