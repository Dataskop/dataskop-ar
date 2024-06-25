using System.Collections.Generic;
using UnityEngine;

namespace Dataskop.Data {

	public class AuthorRepository : MonoBehaviour {

 

		[SerializeField] private Sprite[] authorSprites;

  

 

		public IDictionary<string, Sprite> AuthorSprites { get; set; }

  

 

		private void Awake() {

			AuthorSprites = new Dictionary<string, Sprite>();

			foreach (Sprite s in authorSprites) {
				AuthorSprites.Add(s.name, s);
			}

		}

  

	}

}