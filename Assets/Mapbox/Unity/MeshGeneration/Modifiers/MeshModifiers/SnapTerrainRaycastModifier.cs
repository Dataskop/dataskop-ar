using System.Collections.Generic;

namespace Mapbox.Unity.MeshGeneration.Modifiers {

	using Map;
	using Data;
	using UnityEngine;

	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Snap Terrain Raycast Modifier")]
	public class SnapTerrainRaycastModifier : MeshModifier {

		private const int RAY_LENGTH = 50;

		[SerializeField]
		private LayerMask _terrainMask;
		private double scaledX;
		private double scaledY;

		public override ModifierType Type => ModifierType.Preprocess;

		public override void Run(VectorFeatureUnity feature, MeshData md, UnityTile tile = null) {
			scaledX = tile.Rect.Size.x * tile.TileScale;
			scaledY = tile.Rect.Size.y * tile.TileScale;

			foreach (List<Vector3> sub in feature.Points) {
				for (int i = 0; i < sub.Count; i++) {
					float h = tile.QueryHeightData(
						(float)((sub[i].x + md.PositionInTile.x + scaledX / 2) / scaledX),
						(float)((sub[i].z + md.PositionInTile.z + scaledY / 2) / scaledY)
					);

					RaycastHit hit;
					Vector3 rayCenter =
						new(
							sub[i].x + md.PositionInTile.x + tile.transform.position.x,
							h + RAY_LENGTH / 2,
							sub[i].z + md.PositionInTile.z + tile.transform.position.z
						);

					if (Physics.Raycast(rayCenter, Vector3.down, out hit, RAY_LENGTH * 5, _terrainMask)) {
						sub[i] += new Vector3(0, hit.point.y + md.PositionInTile.y - tile.transform.position.y, 0);
					}
					else {
						// Raycasting sometimes fails at terrain boundaries, fallback to tile height data.
						sub[i] += new Vector3(0, h, 0);
					}
				}
			}
		}

	}

}