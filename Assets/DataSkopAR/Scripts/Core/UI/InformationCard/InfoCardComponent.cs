using UnityEngine;
using UnityEngine.UIElements;
namespace DataskopAR.UI {

	public abstract class InfoCardComponent : MonoBehaviour {

		protected abstract VisualElement ComponentRoot { get; set; }

		public abstract void Init(VisualElement infoCard);

		public virtual void Hide() {
			ComponentRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
		}

		public virtual void Show() {
			ComponentRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
		}

	}

}