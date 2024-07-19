using System;
using System.Globalization;
using Dataskop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class DotVisObject : MonoBehaviour, IVisObject {

		[Header("References")]
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] private Image visRenderer;
		[SerializeField] private Image boolIconRenderer;
		[SerializeField] private Image authorIconImageRenderer;
		[SerializeField] private Sprite[] boolIcons;

		[Header("Values")]
		[SerializeField] private Color32 boolTrueColor;
		[SerializeField] private Color32 boolFalseColor;
		[SerializeField] private AnimationCurve animationCurveSelect;
		[SerializeField] private AnimationCurve animationCurveDeselect;
		[SerializeField] private float animationTimeOnSelect;
		[SerializeField] private float animationTimeOnDeselect;
		[SerializeField] private float selectionScale;

		private Vector3 animationTarget;
		private Coroutine animationCoroutine;
		private Coroutine moveLineCoroutine;
		private bool isSelected;

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public int Index { get; set; }

		public bool IsFocused { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; } = {
			MeasurementType.Float,
			MeasurementType.Bool
		};

		public CanvasGroup DataDisplay => dataDisplay;

		public TextMeshProUGUI IDTextMesh => idTextMesh;

		public TextMeshProUGUI ValueTextMesh => valueTextMesh;

		public TextMeshProUGUI DateTextMesh => dateTextMesh;

		public Image VisRenderer => visRenderer;

		public Image BoolIconRenderer => boolIconRenderer;

		public Image AuthorIconRenderer => authorIconImageRenderer;

		public void SetDisplayData(VisualizationResultDisplayData displayData) {

			idTextMesh.text = displayData.Result.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();

			switch (displayData.Type) {
				case MeasurementType.Float: {
					float receivedValue = displayData.Result.ReadAsFloat();
					boolIconRenderer.enabled = false;
					valueTextMesh.alpha = 1;
					valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {displayData.Attribute.Unit}";
					dateTextMesh.text = displayData.Result.GetTime();
					break;
				}
				case MeasurementType.Bool: {
					valueTextMesh.alpha = 1;
					boolIconRenderer.enabled = false;
					valueTextMesh.text = displayData.Result.ReadAsBool().ToString();
					int boolValue = displayData.Result.ReadAsBool() ? 1 : 0;
					boolIconRenderer.color = displayData.Result.ReadAsBool() ? boolTrueColor : boolFalseColor;
					boolIconRenderer.sprite = boolValue == 0 ? boolIcons[0] : boolIcons[1];
					dateTextMesh.text = displayData.Result.GetTime();
					break;
				}
			}

			if (displayData.Result.Author != null) {
				authorIconImageRenderer.sprite = displayData.AuthorSprite;
				authorIconImageRenderer.enabled = true;
			}
			else {
				authorIconImageRenderer.enabled = false;
			}
		}

		public void OnHover() {
			HasHovered?.Invoke(Index);
		}

		public void OnSelect() {

			if (animationCoroutine != null) {
				StopCoroutine(animationCoroutine);
				visRenderer.transform.localScale = animationTarget;
			}

			animationTarget = visRenderer.transform.localScale * selectionScale;

			animationCoroutine = StartCoroutine(Lerper.TransformLerpOnCurve(
				visRenderer.transform,
				TransformValue.Scale,
				visRenderer.transform.localScale,
				animationTarget,
				animationTimeOnSelect,
				animationCurveSelect,
				OnAnimationFinished
			));

			isSelected = true;
			HasSelected?.Invoke(Index);

		}

		public void OnDeselect() {

			if (isSelected) {
				if (animationCoroutine != null) {
					StopCoroutine(animationCoroutine);
					visRenderer.transform.localScale = animationTarget;
				}

				animationTarget = visRenderer.transform.localScale / selectionScale;

				animationCoroutine = StartCoroutine(Lerper.TransformLerpOnCurve(
					visRenderer.transform,
					TransformValue.Scale,
					visRenderer.transform.localScale,
					animationTarget,
					animationTimeOnDeselect,
					animationCurveDeselect,
					OnAnimationFinished
				));

				isSelected = false;
			}

			HasDeselected?.Invoke(Index);
		}

		public void ShowDisplay() {
			dataDisplay.alpha = 1;
		}

		public void HideDisplay() {
			dataDisplay.alpha = 0;
		}

		public void SetMaterial(Material newMaterial) {
			visRenderer.material = newMaterial;
			valueTextMesh.color = newMaterial.color;
		}

		public void Delete() {
			Destroy(gameObject);
		}

		private void OnAnimationFinished() {
			animationCoroutine = null;
		}

	}

}