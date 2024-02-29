using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	[CreateAssetMenu(fileName = "Options", menuName = "VisOptions/Add Bubble Option...", order = 2)]
	public class BubbleOptions : ScriptableObject {

		public Style[] materialOptions;

	}

}