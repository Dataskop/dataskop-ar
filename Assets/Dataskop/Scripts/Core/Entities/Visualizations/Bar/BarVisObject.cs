using System;
using System.Globalization;
using Dataskop.Data;
using Dataskop.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class BarVisObject : MonoBehaviour, IVisObject {

		[Header("References")]
		[SerializeField] private CanvasGroup dataDisplay;
		[SerializeField] private CanvasGroup authorDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] private TextMeshProUGUI maxValueTextMesh;
		[SerializeField] private TextMeshProUGUI minValueTextMesh;
		[SerializeField] private Image boolIconRenderer;
		[SerializeField] private Image authorIconImageRenderer;
		[SerializeField] private Sprite[] boolIcons;
		[SerializeField] private MeshRenderer barFillMeshRenderer;
		[SerializeField] private MeshRenderer barFrameMeshRenderer;
		[SerializeField] private Transform barFill;
		[SerializeField] private Transform barFrame;

		[Header("Values")]
		[SerializeField] private Color32 boolTrueColor;
		[SerializeField] private Color32 boolFalseColor;

		private Vector3 animationTarget;
		private bool isSelected;

		public int Index { get; set; }

		public bool IsFocused { get; set; }

		public CanvasGroup DataDisplay => dataDisplay;

		public TextMeshProUGUI IDTextMesh => idTextMesh;

		public TextMeshProUGUI ValueTextMesh => valueTextMesh;

		public TextMeshProUGUI DateTextMesh => dateTextMesh;

		public Image BoolIconRenderer => boolIconRenderer;

		public Image AuthorIconRenderer => authorIconImageRenderer;

		public event Action<int> HasHovered;

		public event Action<int> HasSelected;

		public event Action<int> HasDeselected;

		public void SetDisplayData(VisualizationResultDisplayData displayData) {

			idTextMesh.text = displayData.Result.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();

			switch (displayData.Type) {
				case MeasurementType.Bool: {
					float receivedValue = displayData.Result.ReadAsBool() ? 1 : 0;
					SetPillarHeight(receivedValue, displayData.Attribute.Minimum, displayData.Attribute.Maximum, 0.01f,
						barFrame.localScale.y);
					valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {displayData.Attribute?.Unit}";
					dateTextMesh.text = displayData.Result.GetTime();
					break;
				}
				case MeasurementType.Float: {
					float receivedValue = displayData.Result.ReadAsFloat();
					SetPillarHeight(receivedValue, displayData.Attribute.Minimum, displayData.Attribute.Maximum, 0.01f,
						barFrame.localScale.y);
					valueTextMesh.text = receivedValue.ToString("00.00", CultureInfo.InvariantCulture) + $" {displayData.Attribute?.Unit}";
					minValueTextMesh.text = displayData.Attribute?.Minimum.ToString("00.00", CultureInfo.InvariantCulture) +
					                        $" {displayData.Attribute?.Unit}";
					maxValueTextMesh.text = displayData.Attribute?.Maximum.ToString("00.00", CultureInfo.InvariantCulture) +
					                        $" {displayData.Attribute?.Unit}";
					dateTextMesh.text = displayData.Result.GetTime();
					break;
				}
			}

			if (displayData.Result.Author != string.Empty) {
				authorIconImageRenderer.sprite = displayData.AuthorSprite;
				authorIconImageRenderer.enabled = true;
			}
			else {
				authorIconImageRenderer.enabled = false;
			}

			Rotate(displayData.ActiveHistory);

		}

		public void OnHover() {
			HasHovered?.Invoke(Index);
		}

		public void OnSelect() {
			isSelected = true;
			HasSelected?.Invoke(Index);
		}

		public void OnDeselect() {
			if (isSelected) {
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

		/// <summary>
		/// Applies materials to the vis object.
		/// </summary>
		/// <param name="materials"><br/>[0] Bar Frame<br/>[1] Bar Fill</param>
		public void SetMaterials(params Material[] materials) {

			if (materials.Length < 2) return;

			barFrameMeshRenderer.material = materials[0];
			barFillMeshRenderer.material = materials[1];
			valueTextMesh.color = materials[0].color;
		}

		public void Delete() {
			Destroy(gameObject);
		}

		private void Rotate(bool isRotated) {

			if (isRotated) {
				transform.localRotation = Quaternion.Euler(0, 0, -90);
				dataDisplay.transform.localRotation = Quaternion.Euler(0, 0, 90);
				authorDisplay.transform.localRotation = Quaternion.Euler(0, 0, 90);
			}
			else {
				dataDisplay.transform.localRotation = Quaternion.Euler(0, 0, 0);
				authorDisplay.transform.localRotation = Quaternion.Euler(0, 0, 0);
				transform.localRotation = Quaternion.Euler(0, 0, 0);
			}

			dataDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(
				isRotated ? barFrame.localScale.y * 100 : barFrame.localScale.x * 100,
				isRotated ? barFrame.localScale.x * 100 : barFrame.localScale.y * 100
			);

			authorDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(
				isRotated ? barFrame.localScale.y * 100 : barFrame.localScale.x * 100,
				isRotated ? barFrame.localScale.x * 100 : barFrame.localScale.y * 100
			);

			RectTransform maxValueTransform = maxValueTextMesh.GetComponent<RectTransform>();
			maxValueTransform.anchorMin = isRotated ? new Vector2(1, 0) : new Vector2(0, 1);
			maxValueTransform.pivot = isRotated ? new Vector2(1, 0.5f) : new Vector2(0.5f, 1);

			maxValueTransform.sizeDelta = isRotated
				? new Vector2(80, maxValueTransform.sizeDelta.y)
				: new Vector2(maxValueTransform.sizeDelta.x, 10);

			RectTransform minValueTransform = minValueTextMesh.GetComponent<RectTransform>();
			minValueTransform.anchorMax = isRotated ? new Vector2(0, 1) : new Vector2(1, 0);
			minValueTransform.pivot = isRotated ? new Vector2(0, 0.5f) : new Vector2(0.5f, 0);

			minValueTransform.sizeDelta = isRotated
				? new Vector2(80, minValueTransform.sizeDelta.y)
				: new Vector2(minValueTransform.sizeDelta.x, 10);

			RectTransform authorIconTransform = authorIconImageRenderer.GetComponent<RectTransform>();
			authorIconTransform.anchorMax = isRotated ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);
			authorIconTransform.anchorMin = isRotated ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);
			authorIconTransform.pivot = isRotated ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);
			authorIconTransform.anchoredPosition = isRotated ? new Vector2(40, 0) : new Vector2(0, -40);

			maxValueTextMesh.alignment = isRotated ? TextAlignmentOptions.Right : TextAlignmentOptions.Center;
			minValueTextMesh.alignment = isRotated ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
		}

		private void SetPillarHeight(float heightValue, float minValue, float maxValue, float minBarHeight, float maxBarHeight) {
			heightValue = Mathf.Clamp(heightValue, minValue, maxValue);
			float barHeight = MathExtensions.Map(heightValue, minValue, maxValue, minBarHeight, maxBarHeight);
			Vector3 localScale = barFill.localScale;
			Vector3 barFillScale = new(localScale.x, barHeight, localScale.z);
			localScale = barFillScale;
			barFill.localScale = localScale;
		}

	}

}