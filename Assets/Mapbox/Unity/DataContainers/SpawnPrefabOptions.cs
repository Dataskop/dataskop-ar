namespace Mapbox.Unity.Map {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using MeshGeneration.Modifiers;
	using System;
	using Map;

	[Serializable]
	public class SpawnPrefabOptions : ModifierProperties {

		public override Type ModifierType => typeof(PrefabModifier);

		public GameObject prefab;
		public bool scaleDownWithWorld = true;
		[NonSerialized]
		public Action<List<GameObject>> AllPrefabsInstatiated = delegate { };

	}

}