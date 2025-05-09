﻿using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Map;
using Mapbox.Map;
using Mapbox.Utils;
using UnityEngine.Rendering;

namespace Mapbox.Unity.MeshGeneration.Factories.TerrainStrategies {

	public class LowPolyTerrainStrategy : TerrainStrategy, IElevationBasedTerrainStrategy {

		protected Dictionary<UnwrappedTileId, Mesh> _meshData;
		private Mesh _stitchTarget;
		private MeshData _currentTileMeshData;
		private MeshData _stitchTargetMeshData;
		private List<Vector3> _newVertexList;
		private List<Vector3> _newNormalList;
		private List<Vector2> _newUvList;
		private List<int> _newTriangleList;
		private Vector3 _newDir;
		private int _vertA, _vertB, _vertC;
		private int _counter;

		public override void Initialize(ElevationLayerProperties elOptions) {
			_elevationOptions = elOptions;
			_meshData = new Dictionary<UnwrappedTileId, Mesh>();
			_currentTileMeshData = new MeshData();
			_stitchTargetMeshData = new MeshData();
			int sampleCountSquare = _elevationOptions.modificationOptions.sampleCount *
			                        _elevationOptions.modificationOptions.sampleCount;

			_newVertexList = new List<Vector3>(sampleCountSquare);
			_newNormalList = new List<Vector3>(sampleCountSquare);
			_newUvList = new List<Vector2>(sampleCountSquare);
			_newTriangleList = new List<int>();
		}

		public override void UnregisterTile(UnityTile tile) {
			_meshData.Remove(tile.UnwrappedTileId);
		}

		public override void RegisterTile(UnityTile tile) {
			if (_elevationOptions.unityLayerOptions.addToLayer &&
			    tile.gameObject.layer != _elevationOptions.unityLayerOptions.layerId) {
				tile.gameObject.layer = _elevationOptions.unityLayerOptions.layerId;
			}

			if ((int)tile.ElevationType != (int)ElevationLayerType.LowPolygonTerrain ||
			    tile.MeshFilter.sharedMesh.vertexCount != RequiredVertexCount) {
				tile.MeshFilter.sharedMesh.Clear();
				CreateBaseMesh(tile);
				tile.ElevationType = TileTerrainType.LowPoly;
			}

			GenerateTerrainMesh(tile);
		}

		private void CreateBaseMesh(UnityTile tile) {
			//TODO use arrays instead of lists
			_newVertexList.Clear();
			_newNormalList.Clear();
			_newUvList.Clear();
			_newTriangleList.Clear();

			int cap = _elevationOptions.modificationOptions.sampleCount - 1;

			for (float y = 0; y < cap; y++) {
				for (float x = 0; x < cap; x++) {
					float x1 = tile.TileScale *
					           (float)(Mathd.Lerp(tile.Rect.Min.x, tile.Rect.Max.x, x / cap) - tile.Rect.Center.x);

					float y1 = tile.TileScale *
					           (float)(Mathd.Lerp(tile.Rect.Min.y, tile.Rect.Max.y, y / cap) - tile.Rect.Center.y);

					float x2 = tile.TileScale * (float)(Mathd.Lerp(tile.Rect.Min.x, tile.Rect.Max.x, (x + 1) / cap) -
					                                    tile.Rect.Center.x);

					float y2 = tile.TileScale * (float)(Mathd.Lerp(tile.Rect.Min.y, tile.Rect.Max.y, (y + 1) / cap) -
					                                    tile.Rect.Center.y);

					int triStart = _newVertexList.Count;
					_newVertexList.Add(new Vector3(x1, 0, y1));
					_newVertexList.Add(new Vector3(x2, 0, y1));
					_newVertexList.Add(new Vector3(x1, 0, y2));
					//--
					_newVertexList.Add(new Vector3(x2, 0, y1));
					_newVertexList.Add(new Vector3(x2, 0, y2));
					_newVertexList.Add(new Vector3(x1, 0, y2));

					_newNormalList.Add(Constants.Math.Vector3Up);
					_newNormalList.Add(Constants.Math.Vector3Up);
					_newNormalList.Add(Constants.Math.Vector3Up);
					//--
					_newNormalList.Add(Constants.Math.Vector3Up);
					_newNormalList.Add(Constants.Math.Vector3Up);
					_newNormalList.Add(Constants.Math.Vector3Up);

					_newUvList.Add(new Vector2(x / cap, 1 - y / cap));
					_newUvList.Add(new Vector2((x + 1) / cap, 1 - y / cap));
					_newUvList.Add(new Vector2(x / cap, 1 - (y + 1) / cap));
					//--
					_newUvList.Add(new Vector2((x + 1) / cap, 1 - y / cap));
					_newUvList.Add(new Vector2((x + 1) / cap, 1 - (y + 1) / cap));
					_newUvList.Add(new Vector2(x / cap, 1 - (y + 1) / cap));

					_newTriangleList.Add(triStart);
					_newTriangleList.Add(triStart + 1);
					_newTriangleList.Add(triStart + 2);
					//--
					_newTriangleList.Add(triStart + 3);
					_newTriangleList.Add(triStart + 4);
					_newTriangleList.Add(triStart + 5);
				}
			}

			Mesh mesh = tile.MeshFilter.sharedMesh;
			mesh.indexFormat = IndexFormat.UInt32;
			mesh.SetVertices(_newVertexList);
			mesh.SetNormals(_newNormalList);
			mesh.SetUVs(0, _newUvList);
			mesh.SetTriangles(_newTriangleList, 0);
			mesh.RecalculateBounds();
		}

		/// <summary>
		/// Creates the non-flat terrain mesh, using a grid by defined resolution (_sampleCount). Vertex order goes right & up. Normals are calculated manually and UV map is fitted/stretched 1-1.
		/// Any additional scripts or logic, like MeshCollider or setting layer, can be done here.
		/// </summary>
		/// <param name="tile"></param>
		// <param name="heightMultiplier">Multiplier for queried height value</param>
		private void GenerateTerrainMesh(UnityTile tile) {
			tile.MeshFilter.sharedMesh.GetVertices(_currentTileMeshData.Vertices);
			tile.MeshFilter.sharedMesh.GetNormals(_currentTileMeshData.Normals);

			int cap = _elevationOptions.modificationOptions.sampleCount - 1;

			for (float y = 0; y < cap; y++) {
				for (float x = 0; x < cap; x++) {
					_currentTileMeshData.Vertices[(int)(y * cap + x) * 6] = new Vector3(
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6].x,
						tile.QueryHeightData(x / cap, 1 - y / cap),
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6].z
					);

					_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 1] = new Vector3(
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 1].x,
						tile.QueryHeightData((x + 1) / cap, 1 - y / cap),
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 1].z
					);

					_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 2] = new Vector3(
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 2].x,
						tile.QueryHeightData(x / cap, 1 - (y + 1) / cap),
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 2].z
					);

					//--

					_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 3] = new Vector3(
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 3].x,
						tile.QueryHeightData((x + 1) / cap, 1 - y / cap),
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 3].z
					);

					_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 4] = new Vector3(
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 4].x,
						tile.QueryHeightData((x + 1) / cap, 1 - (y + 1) / cap),
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 4].z
					);

					_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 5] = new Vector3(
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 5].x,
						tile.QueryHeightData(x / cap, 1 - (y + 1) / cap),
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 5].z
					);

					_newDir = Vector3.Cross(
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 1] -
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6],
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 2] -
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6]
					);

					_currentTileMeshData.Normals[(int)(y * cap + x) * 6 + 0] = _newDir;
					_currentTileMeshData.Normals[(int)(y * cap + x) * 6 + 1] = _newDir;
					_currentTileMeshData.Normals[(int)(y * cap + x) * 6 + 2] = _newDir;
					//--
					_newDir = Vector3.Cross(
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 4] -
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 3],
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 5] -
						_currentTileMeshData.Vertices[(int)(y * cap + x) * 6 + 3]
					);

					_currentTileMeshData.Normals[(int)(y * cap + x) * 6 + 3] = _newDir;
					_currentTileMeshData.Normals[(int)(y * cap + x) * 6 + 4] = _newDir;
					_currentTileMeshData.Normals[(int)(y * cap + x) * 6 + 5] = _newDir;
				}
			}

			FixStitches(tile.UnwrappedTileId, _currentTileMeshData);
			tile.MeshFilter.sharedMesh.SetVertices(_currentTileMeshData.Vertices);
			tile.MeshFilter.sharedMesh.SetNormals(_currentTileMeshData.Normals);
			tile.MeshFilter.sharedMesh.RecalculateBounds();

			if (!_meshData.ContainsKey(tile.UnwrappedTileId)) {
				_meshData.Add(tile.UnwrappedTileId, tile.MeshFilter.sharedMesh);
			}

			if (_elevationOptions.colliderOptions.addCollider) {
				MeshCollider meshCollider = tile.Collider as MeshCollider;

				if (meshCollider) {
					meshCollider.sharedMesh = tile.MeshFilter.sharedMesh;
				}
			}
		}

		private void ResetToFlatMesh(UnityTile tile) {
			tile.MeshFilter.sharedMesh.GetVertices(_currentTileMeshData.Vertices);
			tile.MeshFilter.sharedMesh.GetNormals(_currentTileMeshData.Normals);

			_counter = _currentTileMeshData.Vertices.Count;

			for (int i = 0; i < _counter; i++) {
				_currentTileMeshData.Vertices[i] = new Vector3(
					_currentTileMeshData.Vertices[i].x,
					0,
					_currentTileMeshData.Vertices[i].z
				);

				_currentTileMeshData.Normals[i] = Constants.Math.Vector3Up;
			}

			tile.MeshFilter.sharedMesh.SetVertices(_currentTileMeshData.Vertices);
			tile.MeshFilter.sharedMesh.SetNormals(_currentTileMeshData.Normals);
		}

		/// <summary>
		/// Checkes all neighbours of the given tile and stitches the edges to achieve a smooth mesh surface.
		/// </summary>
		/// <param name="tileId"></param>
		/// <param name="mesh"></param>
		private void FixStitches(UnwrappedTileId tileId, MeshData mesh) {
			int meshVertCount = mesh.Vertices.Count;
			_stitchTarget = null;
			_meshData.TryGetValue(tileId.North, out _stitchTarget);
			int cap = _elevationOptions.modificationOptions.sampleCount - 1;

			if (_stitchTarget != null) {
				_stitchTarget.GetVertices(_stitchTargetMeshData.Vertices);
				_stitchTarget.GetNormals(_stitchTargetMeshData.Normals);

				for (int i = 0; i < cap; i++) {
					mesh.Vertices[6 * i] = new Vector3(
						mesh.Vertices[6 * i].x,
						_stitchTargetMeshData.Vertices[6 * cap * (cap - 1) + 6 * i + 2].y,
						mesh.Vertices[6 * i].z
					);

					mesh.Vertices[6 * i + 1] = new Vector3(
						mesh.Vertices[6 * i + 1].x,
						_stitchTargetMeshData.Vertices[6 * cap * (cap - 1) + 6 * i + 4].y,
						mesh.Vertices[6 * i + 1].z
					);

					mesh.Vertices[6 * i + 3] = new Vector3(
						mesh.Vertices[6 * i + 3].x,
						_stitchTargetMeshData.Vertices[6 * cap * (cap - 1) + 6 * i + 4].y,
						mesh.Vertices[6 * i + 3].z
					);
				}
			}

			_stitchTarget = null;
			_meshData.TryGetValue(tileId.South, out _stitchTarget);

			if (_stitchTarget != null) {
				_stitchTarget.GetVertices(_stitchTargetMeshData.Vertices);
				_stitchTarget.GetNormals(_stitchTargetMeshData.Normals);

				for (int i = 0; i < cap; i++) {
					mesh.Vertices[6 * cap * (cap - 1) + 6 * i + 2] = new Vector3(
						mesh.Vertices[6 * cap * (cap - 1) + 6 * i + 2].x,
						_stitchTargetMeshData.Vertices[6 * i].y,
						mesh.Vertices[6 * cap * (cap - 1) + 6 * i + 2].z
					);

					mesh.Vertices[6 * cap * (cap - 1) + 6 * i + 5] = new Vector3(
						mesh.Vertices[6 * cap * (cap - 1) + 6 * i + 5].x,
						_stitchTargetMeshData.Vertices[6 * i].y,
						mesh.Vertices[6 * cap * (cap - 1) + 6 * i + 5].z
					);

					mesh.Vertices[6 * cap * (cap - 1) + 6 * i + 4] = new Vector3(
						mesh.Vertices[6 * cap * (cap - 1) + 6 * i + 4].x,
						_stitchTargetMeshData.Vertices[6 * i + 3].y,
						mesh.Vertices[6 * cap * (cap - 1) + 6 * i + 4].z
					);
				}
			}

			_stitchTarget = null;
			_meshData.TryGetValue(tileId.West, out _stitchTarget);

			if (_stitchTarget != null) {
				_stitchTarget.GetVertices(_stitchTargetMeshData.Vertices);
				_stitchTarget.GetNormals(_stitchTargetMeshData.Normals);

				for (int i = 0; i < cap; i++) {
					mesh.Vertices[6 * cap * i] = new Vector3(
						mesh.Vertices[6 * cap * i].x,
						_stitchTargetMeshData.Vertices[6 * cap * i + 6 * cap - 5].y,
						mesh.Vertices[6 * cap * i].z
					);

					mesh.Vertices[6 * cap * i + 2] = new Vector3(
						mesh.Vertices[6 * cap * i + 2].x,
						_stitchTargetMeshData.Vertices[6 * cap * i + 6 * cap - 2].y,
						mesh.Vertices[6 * cap * i + 2].z
					);

					mesh.Vertices[6 * cap * i + 5] = new Vector3(
						mesh.Vertices[6 * cap * i + 5].x,
						_stitchTargetMeshData.Vertices[6 * cap * i + 6 * cap - 2].y,
						mesh.Vertices[6 * cap * i + 5].z
					);
				}
			}

			_stitchTarget = null;
			_meshData.TryGetValue(tileId.East, out _stitchTarget);

			if (_stitchTarget != null) {
				_stitchTarget.GetVertices(_stitchTargetMeshData.Vertices);
				_stitchTarget.GetNormals(_stitchTargetMeshData.Normals);

				for (int i = 0; i < cap; i++) {
					mesh.Vertices[6 * cap * i + 6 * cap - 5] = new Vector3(
						mesh.Vertices[6 * cap * i + 6 * cap - 5].x,
						_stitchTargetMeshData.Vertices[6 * cap * i].y,
						mesh.Vertices[6 * cap * i + 6 * cap - 5].z
					);

					mesh.Vertices[6 * cap * i + 6 * cap - 3] = new Vector3(
						mesh.Vertices[6 * cap * i + 6 * cap - 3].x,
						_stitchTargetMeshData.Vertices[6 * cap * i].y,
						mesh.Vertices[6 * cap * i + 6 * cap - 3].z
					);

					mesh.Vertices[6 * cap * i + 6 * cap - 2] = new Vector3(
						mesh.Vertices[6 * cap * i + 6 * cap - 2].x,
						_stitchTargetMeshData.Vertices[6 * cap * i + 5].y,
						mesh.Vertices[6 * cap * i + 6 * cap - 2].z
					);
				}
			}

			_stitchTarget = null;
			_meshData.TryGetValue(tileId.NorthWest, out _stitchTarget);

			if (_stitchTarget != null) {
				_stitchTarget.GetVertices(_stitchTargetMeshData.Vertices);
				_stitchTarget.GetNormals(_stitchTargetMeshData.Normals);

				mesh.Vertices[0] = new Vector3(
					mesh.Vertices[0].x,
					_stitchTargetMeshData.Vertices[meshVertCount - 2].y,
					mesh.Vertices[0].z
				);
			}

			_stitchTarget = null;
			_meshData.TryGetValue(tileId.NorthEast, out _stitchTarget);

			if (_stitchTarget != null) {
				_stitchTarget.GetVertices(_stitchTargetMeshData.Vertices);
				_stitchTarget.GetNormals(_stitchTargetMeshData.Normals);

				mesh.Vertices[6 * cap - 5] = new Vector3(
					mesh.Vertices[6 * cap - 5].x,
					_stitchTargetMeshData.Vertices[6 * (cap - 1) * cap + 2].y,
					mesh.Vertices[6 * cap - 5].z
				);

				mesh.Vertices[6 * cap - 3] = new Vector3(
					mesh.Vertices[6 * cap - 3].x,
					_stitchTargetMeshData.Vertices[6 * (cap - 1) * cap + 2].y,
					mesh.Vertices[6 * cap - 3].z
				);
			}

			_stitchTarget = null;
			_meshData.TryGetValue(tileId.SouthWest, out _stitchTarget);

			if (_stitchTarget != null) {
				_stitchTarget.GetVertices(_stitchTargetMeshData.Vertices);
				_stitchTarget.GetNormals(_stitchTargetMeshData.Normals);

				mesh.Vertices[6 * (cap - 1) * cap + 2] = new Vector3(
					mesh.Vertices[6 * (cap - 1) * cap + 2].x,
					_stitchTargetMeshData.Vertices[6 * cap - 5].y,
					mesh.Vertices[6 * (cap - 1) * cap + 2].z
				);

				mesh.Vertices[6 * (cap - 1) * cap + 5] = new Vector3(
					mesh.Vertices[6 * (cap - 1) * cap + 5].x,
					_stitchTargetMeshData.Vertices[6 * cap - 5].y,
					mesh.Vertices[6 * (cap - 1) * cap + 5].z
				);
			}

			_stitchTarget = null;
			_meshData.TryGetValue(tileId.SouthEast, out _stitchTarget);

			if (_stitchTarget != null) {
				_stitchTarget.GetVertices(_stitchTargetMeshData.Vertices);
				_stitchTarget.GetNormals(_stitchTargetMeshData.Normals);

				mesh.Vertices[6 * cap * cap - 2] = new Vector3(
					mesh.Vertices[6 * cap * cap - 2].x,
					_stitchTargetMeshData.Vertices[0].y,
					mesh.Vertices[6 * cap * cap - 2].z
				);
			}
		}

	}

}