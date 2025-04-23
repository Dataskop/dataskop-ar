namespace Mapbox.Unity.MeshGeneration.Modifiers {

	using UnityEngine;
	using Components;
	using Data;

	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Tag Modifier")]
	public class TagModifier : GameObjectModifier {

		[SerializeField]
		private string _tag;

		public override void Run(VectorEntity ve, UnityTile tile) {
			ve.GameObject.tag = _tag;
		}

	}

}