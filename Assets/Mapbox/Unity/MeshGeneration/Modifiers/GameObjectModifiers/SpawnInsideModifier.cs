// HACK:
// This will work out of the box, but it's intended to be an example of how to approach
// procedural decoration like this.
// A better approach would be to operate on the geometry itself.

namespace Mapbox.Unity.MeshGeneration.Modifiers {

	using Data;
	using Components;
	using UnityEngine;
	using System.Collections.Generic;
	using System;

	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Spawn Inside Modifier")]
	public class SpawnInsideModifier : GameObjectModifier {

		[SerializeField] private int _spawnRateInSquareMeters;

		[SerializeField] private int _maxSpawn = 1000;

		[SerializeField] private GameObject[] _prefabs;

		[SerializeField] private LayerMask _layerMask;

		[SerializeField] private bool _scaleDownWithWorld;

		[SerializeField] private bool _randomizeScale;

		[SerializeField] private bool _randomizeRotation;

		private int _spawnedCount;

		private Dictionary<GameObject, List<GameObject>> _objects;
		private Queue<GameObject> _pool;

		public override void Initialize() {
			if (_objects == null || _pool == null) {
				_objects = new Dictionary<GameObject, List<GameObject>>();
				_pool = new Queue<GameObject>();
			}
		}

		public override void Run(VectorEntity ve, UnityTile tile) {
			_spawnedCount = 0;
			Bounds bounds = ve.Mesh.bounds;
			Vector3 center = ve.Transform.position + bounds.center;
			center.y = 0;

			int area = (int)(bounds.size.x * bounds.size.z);
			int spawnCount = Mathf.Min(area / _spawnRateInSquareMeters, _maxSpawn);

			while (_spawnedCount < spawnCount) {
				float x = UnityEngine.Random.Range(-bounds.extents.x, bounds.extents.x);
				float z = UnityEngine.Random.Range(-bounds.extents.z, bounds.extents.z);
				Ray ray = new(center + new Vector3(x, 100, z), Vector3.down * 2000);

				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 150, _layerMask)) {
					int index = UnityEngine.Random.Range(0, _prefabs.Length);
					Transform transform = GetObject(index, ve.GameObject).transform;
					transform.position = hit.point;

					if (_randomizeRotation) {
						transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(-180f, 180f), 0);
					}

					if (!_scaleDownWithWorld) {
						transform.localScale = Vector3.one / tile.TileScale;
					}

					if (_randomizeScale) {
						Vector3 scale = transform.localScale;
						float y = UnityEngine.Random.Range(scale.y * .7f, scale.y * 1.3f);
						scale.y = y;
						transform.localScale = scale;
					}

				}

				_spawnedCount++;
			}
		}

		public override void OnPoolItem(VectorEntity vectorEntity) {
			if (_objects.ContainsKey(vectorEntity.GameObject)) {
				foreach (GameObject item in _objects[vectorEntity.GameObject]) {
					item.SetActive(false);
					_pool.Enqueue(item);
				}

				_objects[vectorEntity.GameObject].Clear();
				_objects.Remove(vectorEntity.GameObject);
			}
		}

		public override void Clear() {
			foreach (GameObject go in _pool) {
				go.Destroy();
			}

			_pool.Clear();

			foreach (KeyValuePair<GameObject, List<GameObject>> tileObject in _objects) {
				foreach (GameObject go in tileObject.Value) {
					if (Application.isEditor && !Application.isPlaying) {
						DestroyImmediate(go);
					}
					else {
						Destroy(go);
					}
				}
			}

			_objects.Clear();
		}

		private GameObject GetObject(int index, GameObject go) {
			GameObject ob;

			if (_pool.Count > 0) {
				ob = _pool.Dequeue();
				ob.SetActive(true);
				ob.transform.SetParent(go.transform);
			}
			else {
				ob = (GameObject)Instantiate(_prefabs[index], go.transform, false);
			}

			if (_objects.ContainsKey(go)) {
				_objects[go].Add(ob);
			}
			else {
				_objects.Add(
					go, new List<GameObject>() {
						ob
					}
				);
			}

			return ob;
		}

	}

}