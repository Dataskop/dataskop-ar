namespace Mapbox.Unity.MeshGeneration.Modifiers {

	using Data;

	public interface IReplacementCriteria {

		bool ShouldReplaceFeature(VectorFeatureUnity feature);

	}

}