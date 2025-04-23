namespace Mapbox.Unity.MeshGeneration.Filters {

	using UnityEngine;
	using Data;
	using System;

	public interface ILayerFeatureFilterComparer {

		bool Try(VectorFeatureUnity feature);

	}

	public class FilterBase : ILayerFeatureFilterComparer {

		public virtual string Key => "";

		public virtual bool Try(VectorFeatureUnity feature) {
			return true;
		}

		public virtual void Initialize() { }

	}

}