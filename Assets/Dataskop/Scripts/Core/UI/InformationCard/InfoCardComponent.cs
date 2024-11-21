using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public abstract class InfoCardComponent : MonoBehaviour {

		protected abstract VisualElement ComponentRoot { get; set; }

		public abstract void Init(VisualElement infoCard);

		public virtual void HideAll() {
			ComponentRoot.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);

			foreach (VisualElement c in ComponentRoot.Children()) {
				c.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
			}
		}

		public virtual void ShowAll() {

			ComponentRoot.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);

			foreach (VisualElement c in ComponentRoot.Children()) {
				c.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
			}

		}

	}

}
