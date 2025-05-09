﻿namespace Mapbox.Unity.MeshGeneration {

	using Data;
	using UnityEngine;

	public class FeatureCollectionBase : ScriptableObject {

		public virtual void Initialize() { }

		public virtual void AddFeature(double[] position, VectorEntity ve) { }

	}

}