﻿using UnityEngine;

namespace Mapbox.Unity.MeshGeneration.Components {

	public class VertexDebuggerGizmo : MonoBehaviour {

#if UNITY_EDITOR
		[SerializeField] private float _radius = .2f;

		[SerializeField] private Color _color = new(0, 1, 0, .5f);

		[Multiline(10)]
		public string Triangles;

		private Mesh _mesh;

		protected virtual void Start() {
			MeshFilter mf = GetComponent<MeshFilter>();

			if (mf) {
				_mesh = mf.mesh;
				int[] tris = _mesh.triangles;
				Triangles = "";

				for (int i = 0; i < tris.Length; i += 3) {
					Triangles += tris[i] + "," + tris[i + 1] + "," + tris[i + 2] + "\r\n";
				}
			}
		}

		protected virtual void OnDrawGizmosSelected() {
			if (_mesh) {
				Vector3[] verts = _mesh.vertices;

				for (int i = 0; i < verts.Length; i++) {
					Gizmos.color = _color;
					Gizmos.DrawWireCube(transform.position + verts[i] * transform.lossyScale.x, Vector3.one * _radius);
				}
			}
		}
#endif

	}

}