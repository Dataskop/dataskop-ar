using System;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Data;

namespace Mapbox.Unity.MeshGeneration.Modifiers {

	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Smooth Line Modifier")]
	public class SmoothLineModifier : MeshModifier {

		public override ModifierType Type => ModifierType.Preprocess;

		public int _maxEdgeSectionCount = 40;
		public int _preferredEdgeSectionLength = 10;
		private int _counter, _counter2;

		public override void Run(VectorFeatureUnity feature, MeshData md, UnityTile tile = null) {
			_counter = feature.Points.Count;

			for (int i = 0; i < _counter; i++) {
				List<Vector3> nl = new();
				_counter2 = feature.Points[i].Count;

				for (int j = 1; j < _counter2; j++) {
					nl.Add(feature.Points[i][j - 1]);
					float dist = Vector3.Distance(feature.Points[i][j - 1], feature.Points[i][j]);
					float step = Math.Min(_maxEdgeSectionCount, dist / _preferredEdgeSectionLength);

					if (step > 1) {
						int counter = 1;

						while (counter < step) {
							Vector3 nv = Vector3.Lerp(
								feature.Points[i][j - 1], feature.Points[i][j], Mathf.Min(1, counter / step)
							);

							nl.Add(nv);
							counter++;
						}
					}

					nl.Add(feature.Points[i][j]);
				}

				feature.Points[i] = nl;
			}
		}

	}

}