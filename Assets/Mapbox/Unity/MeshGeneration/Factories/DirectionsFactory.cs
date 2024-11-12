namespace Mapbox.Unity.MeshGeneration.Factories {

	using UnityEngine;
	using Directions;
	using System.Collections.Generic;
	using System.Linq;
	using Map;
	using Data;
	using Modifiers;
	using Utils;
	using Utilities;
	using System.Collections;

	public class DirectionsFactory : MonoBehaviour {

		[SerializeField] private AbstractMap _map;

		[SerializeField] private MeshModifier[] MeshModifiers;
		[SerializeField] private Material _material;

		[SerializeField] private Transform[] _waypoints;
		private List<Vector3> _cachedWaypoints;

		[SerializeField]
		[Range(1, 10)]
		private float UpdateFrequency = 2;

		private Directions _directions;
		private int _counter;

		private GameObject _directionsGO;
		private bool _recalculateNext;

		protected virtual void Awake() {
			if (_map == null) {
				_map = FindObjectOfType<AbstractMap>();
			}

			_directions = MapboxAccess.Instance.Directions;
			_map.OnInitialized += Query;
			_map.OnUpdated += Query;
		}

		public void Start() {
			_cachedWaypoints = new List<Vector3>(_waypoints.Length);

			foreach (Transform item in _waypoints) {
				_cachedWaypoints.Add(item.position);
			}

			_recalculateNext = false;

			foreach (MeshModifier modifier in MeshModifiers) {
				modifier.Initialize();
			}

			StartCoroutine(QueryTimer());
		}

		protected virtual void OnDestroy() {
			_map.OnInitialized -= Query;
			_map.OnUpdated -= Query;
		}

		private void Query() {
			int count = _waypoints.Length;
			Vector2d[] wp = new Vector2d[count];

			for (int i = 0; i < count; i++) {
				wp[i] = _waypoints[i].GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
			}

			DirectionResource _directionResource = new(wp, RoutingProfile.Driving);
			_directionResource.Steps = true;
			_directions.Query(_directionResource, HandleDirectionsResponse);
		}

		public IEnumerator QueryTimer() {
			while (true) {
				yield return new WaitForSeconds(UpdateFrequency);

				for (int i = 0; i < _waypoints.Length; i++) {
					if (_waypoints[i].position != _cachedWaypoints[i]) {
						_recalculateNext = true;
						_cachedWaypoints[i] = _waypoints[i].position;
					}
				}

				if (_recalculateNext) {
					Query();
					_recalculateNext = false;
				}
			}
		}

		private void HandleDirectionsResponse(DirectionsResponse response) {
			if (response == null || null == response.Routes || response.Routes.Count < 1) {
				return;
			}

			MeshData meshData = new();
			List<Vector3> dat = new();

			foreach (Vector2d point in response.Routes[0].Geometry) {
				dat.Add(
					Conversions.GeoToWorldPosition(point.x, point.y, _map.CenterMercator, _map.WorldRelativeScale)
						.ToVector3xz()
				);
			}

			VectorFeatureUnity feat = new();
			feat.Points.Add(dat);

			foreach (MeshModifier mod in MeshModifiers.Where(x => x.Active)) {
				mod.Run(feat, meshData, _map.WorldRelativeScale);
			}

			CreateGameObject(meshData);
		}

		private GameObject CreateGameObject(MeshData data) {
			if (_directionsGO != null) {
				_directionsGO.Destroy();
			}

			_directionsGO = new GameObject("direction waypoint " + " entity");
			Mesh mesh = _directionsGO.AddComponent<MeshFilter>().mesh;
			mesh.subMeshCount = data.Triangles.Count;

			mesh.SetVertices(data.Vertices);
			_counter = data.Triangles.Count;

			for (int i = 0; i < _counter; i++) {
				List<int> triangle = data.Triangles[i];
				mesh.SetTriangles(triangle, i);
			}

			_counter = data.UV.Count;

			for (int i = 0; i < _counter; i++) {
				List<Vector2> uv = data.UV[i];
				mesh.SetUVs(i, uv);
			}

			mesh.RecalculateNormals();
			_directionsGO.AddComponent<MeshRenderer>().material = _material;
			return _directionsGO;
		}

	}

}