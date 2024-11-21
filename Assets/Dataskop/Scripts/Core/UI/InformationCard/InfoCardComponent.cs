using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public abstract class InfoCardComponent : MonoBehaviour {

		protected abstract VisualElement ComponentRoot { get; set; }

		public abstract void Init(VisualElement infoCard);

		public virtual void Hide() {
			ComponentRoot.visible = false;
		}

		public virtual void Show() {
			ComponentRoot.visible = true;
		}

	}

}
