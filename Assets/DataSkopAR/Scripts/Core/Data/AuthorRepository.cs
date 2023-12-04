using System.Collections.Generic;
using UnityEngine;

namespace DataskopAR.Data {

	public class AuthorRepository : MonoBehaviour {

#region Fields

		[SerializeField] private Sprite[] authorSprites;

#endregion

#region Properties

		public IDictionary<string, Sprite> AuthorSprites { get; set; }

#endregion

#region Methods

		private void Awake() {

			AuthorSprites = new Dictionary<string, Sprite>();

			foreach (Sprite s in authorSprites) {
				AuthorSprites.Add(s.name, s);
			}
			
		}

#endregion

	}

}