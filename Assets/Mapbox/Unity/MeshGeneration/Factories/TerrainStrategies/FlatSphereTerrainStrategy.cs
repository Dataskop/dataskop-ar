using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;

namespace Mapbox.Unity.MeshGeneration.Factories.TerrainStrategies {

	public class FlatSphereTerrainStrategy : TerrainStrategy {

		public float Radius => _elevationOptions.modificationOptions.earthRadius;

		public override int RequiredVertexCount => _elevationOptions.modificationOptions.sampleCount *
		                                           _elevationOptions.modificationOptions.sampleCount;

		public override void Initialize(ElevationLayerProperties elOptions) {
			_elevationOptions = elOptions;
		}

		public override void RegisterTile(UnityTile tile) {
			if (_elevationOptions.unityLayerOptions.addToLayer &&
			    tile.gameObject.layer != _elevationOptions.unityLayerOptions.layerId) {
				tile.gameObject.layer = _elevationOptions.unityLayerOptions.layerId;
			}

			if ((int)tile.ElevationType != (int)ElevationLayerType.GlobeTerrain ||
			    tile.MeshFilter.sharedMesh.vertexCount != RequiredVertexCount) {
				tile.MeshFilter.sharedMesh.Clear();
				tile.ElevationType = TileTerrainType.Globe;
			}

			GenerateTerrainMesh(tile);
		}

		private void GenerateTerrainMesh(UnityTile tile) {
			List<Vector3> verts = new();
			int _sampleCount = _elevationOptions.modificationOptions.sampleCount;
			float _radius = _elevationOptions.modificationOptions.earthRadius;

			for (float x = 0; x < _sampleCount; x++) {
				for (float y = 0; y < _sampleCount; y++) {
					float xx = Mathf.Lerp(
						(float)tile.Rect.Min.x, (float)tile.Rect.Min.x + (float)tile.Rect.Size.x,
						x / (_sampleCount - 1)
					);

					float yy = Mathf.Lerp(
						(float)tile.Rect.Max.y, (float)tile.Rect.Max.y + (float)tile.Rect.Size.y,
						y / (_sampleCount - 1)
					);

					Vector2d ll = Conversions.MetersToLatLon(new Vector2d(xx, yy));

					float latitude = (float)(Mathf.Deg2Rad * ll.x);
					float longitude = (float)(Mathf.Deg2Rad * ll.y);

					float xPos = _radius * Mathf.Cos(latitude) * Mathf.Cos(longitude);
					float zPos = _radius * Mathf.Cos(latitude) * Mathf.Sin(longitude);
					float yPos = _radius * Mathf.Sin(latitude);

					Vector3 pp = new(xPos, yPos, zPos);
					verts.Add(pp);
				}
			}

			List<int> trilist = new();

			for (int y = 0; y < _sampleCount - 1; y++) {
				for (int x = 0; x < _sampleCount - 1; x++) {
					trilist.Add(y * _sampleCount + x);
					trilist.Add(y * _sampleCount + x + _sampleCount + 1);
					trilist.Add(y * _sampleCount + x + _sampleCount);

					trilist.Add(y * _sampleCount + x);
					trilist.Add(y * _sampleCount + x + 1);
					trilist.Add(y * _sampleCount + x + _sampleCount + 1);
				}
			}

			List<Vector2> uvlist = new();
			float step = 1f / (_sampleCount - 1);

			for (int i = 0; i < _sampleCount; i++) {
				for (int j = 0; j < _sampleCount; j++) {
					uvlist.Add(new Vector2(i * step, j * step));
				}
			}

			tile.MeshFilter.sharedMesh.SetVertices(verts);
			tile.MeshFilter.sharedMesh.SetTriangles(trilist, 0);
			tile.MeshFilter.sharedMesh.SetUVs(0, uvlist);
			tile.MeshFilter.sharedMesh.RecalculateBounds();
			tile.MeshFilter.sharedMesh.RecalculateNormals();

			tile.transform.localPosition = Constants.Math.Vector3Zero;
		}

		public override void UnregisterTile(UnityTile tile) { }

	}

}