namespace Mapbox.Unity.MeshGeneration.Components {

	using UnityEngine;

	public class VertexDebugger : MonoBehaviour {

		[Multiline(10)]
		public string Triangles;
		[Multiline(10)]
		public string Tangents;

		protected virtual void Start() {
			MeshFilter mf = GetComponent<MeshFilter>();

			if (mf) {
				Mesh mesh = mf.mesh;
				Vector3[] verts = mesh.vertices;
				Vector4[] tang = mesh.tangents;

				for (int i = 0; i < verts.Length; i++) {
					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					go.name = i.ToString();
					go.transform.SetParent(transform, false);
					go.transform.localPosition = verts[i];

					Debug.DrawLine(
						transform.position + verts[i],
						transform.position + verts[i] + new Vector3(tang[i].x, tang[i].y, tang[i].z) * .5f, Color.red,
						10000
					);
				}

				int[] tris = mesh.triangles;
				Triangles = "";

				for (int i = 0; i < tris.Length; i += 3) {
					Triangles += tris[i] + "," + tris[i + 1] + "," + tris[i + 2] + "\r\n";
				}
			}
		}

	}

}