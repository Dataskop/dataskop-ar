namespace Mapbox.Unity.MeshGeneration.Modifiers {

	using UnityEngine;
	using Data;
	using Components;
	using Interfaces;
	using System.Collections.Generic;
	using Map;
	using System;

	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Prefab Modifier")]
	public class PrefabModifier : GameObjectModifier {

		private Dictionary<GameObject, GameObject> _objects;
		[SerializeField]
		private SpawnPrefabOptions _options;
		private List<GameObject> _prefabList = new();

		public override void Initialize() {
			if (_objects == null) {
				_objects = new Dictionary<GameObject, GameObject>();
			}
		}

		public override void SetProperties(ModifierProperties properties) {
			_options = (SpawnPrefabOptions)properties;
			_options.PropertyHasChanged += UpdateModifier;
		}

		public override void Run(VectorEntity ve, UnityTile tile) {
			if (_options.prefab == null) {
				return;
			}

			GameObject go = null;

			if (_objects.ContainsKey(ve.GameObject)) {
				go = _objects[ve.GameObject];
			}
			else {
				go = Instantiate(_options.prefab);
				_prefabList.Add(go);
				_objects.Add(ve.GameObject, go);
				go.transform.SetParent(ve.GameObject.transform, false);
			}

			PositionScaleRectTransform(ve, tile, go);

			if (_options.AllPrefabsInstatiated != null) {
				_options.AllPrefabsInstatiated(_prefabList);
			}
		}

		public void PositionScaleRectTransform(VectorEntity ve, UnityTile tile, GameObject go) {
			RectTransform goRectTransform;
			IFeaturePropertySettable settable = null;
			Vector3 centroidVector = new();

			foreach (Vector3 point in ve.Feature.Points[0]) {
				centroidVector += point;
			}

			centroidVector = centroidVector / ve.Feature.Points[0].Count;

			go.name = ve.Feature.Data.Id.ToString();

			goRectTransform = go.GetComponent<RectTransform>();

			if (goRectTransform == null) {
				go.transform.localPosition = centroidVector;

				if (_options.scaleDownWithWorld) {
					go.transform.localScale = _options.prefab.transform.localScale * tile.TileScale;
				}
			}
			else {
				goRectTransform.anchoredPosition3D = centroidVector;

				if (_options.scaleDownWithWorld) {
					goRectTransform.localScale = _options.prefab.transform.localScale * tile.TileScale;
				}
			}

			settable = go.GetComponent<IFeaturePropertySettable>();

			if (settable != null) {
				settable.Set(ve.Feature.Properties);
			}
		}

		public override void Clear() {
			base.Clear();

			foreach (GameObject gameObject in _objects.Values) {
				gameObject.Destroy();
			}

			foreach (GameObject gameObject in _prefabList) {
				gameObject.Destroy();
			}
		}

	}

}