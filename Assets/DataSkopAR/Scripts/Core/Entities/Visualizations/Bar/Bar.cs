using System.Globalization;
using System.Linq;
using DataskopAR.Data;
using DataskopAR.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DataskopAR.Entities.Visualizations {

	public class Bar : Visualization {

#region Fields

		[Header("References")]
		[SerializeField] private MeshRenderer barFillMeshRenderer;
		[SerializeField] private MeshRenderer barFrameMeshRenderer;
		[SerializeField] private Transform barFill;
		[SerializeField] private Transform barFrame;
		[SerializeField] private BarOptions options;
		[SerializeField] private BarTimeSeries barTimeSeries;
		[SerializeField] private BoxCollider barCollider;
		[SerializeField] private Transform visTransform;
		[SerializeField] private Image authorIconImageRenderer;
		[SerializeField] private Transform timeElementsContainer;

		[Header("Display References")]
		[SerializeField] private Transform dataDisplay;
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI minValueTextMesh;
		[SerializeField] private TextMeshProUGUI maxValueTextMesh;

		private Vector3 origin;
		private bool isRotated;

#endregion

#region Properties

		public VisualizationType Type => VisualizationType.bar;

		private Vector3 BarFillScale { get; set; }

		private float BarHeight { get; set; }

		private BarOptions Options { get; set; }

		private BarTimeSeries TimeSeries => barTimeSeries;

		public override Transform VisTransform => visTransform;

		public override MeasurementType[] AllowedMeasurementTypes { get; set; } = {
			MeasurementType.Float,
			MeasurementType.Bool
		};

		private bool IsRotated {
			get => isRotated;
			set {
				isRotated = value;
				OnVisualizationRotated(IsRotated);
			}
		}

#endregion

#region Methods

		private void Awake() {
			TimeSeries.TimeSeriesBeforeSpawn += RotateVisualization;
			TimeSeries.TimeSeriesDespawned += ResetRotation;
		}

		protected override void OnDataPointChanged() {
			base.OnDataPointChanged();
			Options = Instantiate(options);

			VisTransform.root.localPosition = Offset;
			VisTransform.localScale *= Scale;

			OnMeasurementResultChanged(DataPoint.CurrentMeasurementResult);
			barCollider.enabled = true;
		}

		private void SetPillarHeight(float heightValue, float minValue, float maxValue, float minBarHeight, float maxBarHeight) {
			heightValue = Mathf.Clamp(heightValue, minValue, maxValue);
			BarHeight = MathExtensions.Map(heightValue, minValue, maxValue, minBarHeight, maxBarHeight);
			Vector3 localScale = barFill.localScale;
			BarFillScale = new Vector3(localScale.x, BarHeight, localScale.z);
			localScale = BarFillScale;
			barFill.localScale = localScale;
		}

		private void SetDisplayValue(float value) {
			valueTextMesh.text = value.ToString("00.00", CultureInfo.InvariantCulture) + $" {DataPoint.Attribute?.Unit}";
		}

		private void SetDisplayValue(bool value) {
			valueTextMesh.text = value.ToString();
		}

		private void SetMinMaxDisplayValues(float min, float max) {
			minValueTextMesh.text = min.ToString("00.00", CultureInfo.InvariantCulture) + $" {DataPoint.Attribute?.Unit}";
			maxValueTextMesh.text = max.ToString("00.00", CultureInfo.InvariantCulture) + $" {DataPoint.Attribute?.Unit}";
		}

		private void RotateVisualization() {
			VisTransform.localRotation = Quaternion.Euler(0, 0, -90);
			VisTransform.localPosition = new Vector3(barFrame.localScale.y * Scale * -0.5f, barFrame.localScale.x * Scale * 0.5f, 0);
			dataDisplay.localRotation = Quaternion.Euler(0, 0, 90);
			IsRotated = true;
		}

		private void ResetRotation() {
			dataDisplay.localRotation = Quaternion.Euler(0, 0, 0);
			VisTransform.localPosition = new Vector3(0, 0, 0);
			VisTransform.localRotation = Quaternion.Euler(0, 0, 0);
			IsRotated = false;
		}

		private void OnVisualizationRotated(bool rotationState) {

			dataDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(
				rotationState ? barFrame.localScale.y * 100 : barFrame.localScale.x * 100,
				rotationState ? barFrame.localScale.x * 100 : barFrame.localScale.y * 100
			);

			RectTransform maxValueTransform = maxValueTextMesh.GetComponent<RectTransform>();
			maxValueTransform.anchorMin = rotationState ? new Vector2(1, 0) : new Vector2(0, 1);
			maxValueTransform.pivot = rotationState ? new Vector2(1, 0.5f) : new Vector2(0.5f, 1);

			maxValueTransform.sizeDelta = rotationState
				? new Vector2(60, maxValueTransform.sizeDelta.y)
				: new Vector2(maxValueTransform.sizeDelta.x, 10);

			RectTransform minValueTransform = minValueTextMesh.GetComponent<RectTransform>();
			minValueTransform.anchorMax = rotationState ? new Vector2(0, 1) : new Vector2(1, 0);
			minValueTransform.pivot = rotationState ? new Vector2(0, 0.5f) : new Vector2(0.5f, 0);

			minValueTransform.sizeDelta = rotationState
				? new Vector2(60, minValueTransform.sizeDelta.y)
				: new Vector2(minValueTransform.sizeDelta.x, 10);

		}

		public override void ApplyStyle(VisualizationStyle style) { }

		public override void Hover() {
			barFrameMeshRenderer.material = Options.styles[0].hoverMaterial;
			ShowUserDirectionCanvas();
		}

		public override void Select() {
			barFrameMeshRenderer.material = Options.styles[0].selectionMaterial;
			ShowUserDirectionCanvas();
			IsSelected = true;
		}

		public override void Deselect() {
			barFrameMeshRenderer.material = Options.styles[0].defaultMaterial;
			HideAllUserDirectionCanvas();
			IsSelected = false;
		}

		public override void OnTimeSeriesToggled(bool isActive) {
			if (isActive) {
				RotateVisualization();
				TimeSeries.SpawnSeries(timeSeriesConfiguration, DataPoint, timeElementsContainer);
			}
			else {
				TimeSeries.DespawnSeries();
				ResetRotation();
			}
		}

		public override void OnMeasurementResultsUpdated() {
			OnMeasurementResultChanged(DataPoint.MeasurementDefinition.GetLatestMeasurementResult());
		}

		public override void OnMeasurementResultChanged(MeasurementResult mr) {
			if (!AllowedMeasurementTypes.Contains(DataPoint.MeasurementDefinition.MeasurementType)) {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "Value Type not supported by this visualization.",
					DisplayDuration = 5f
				});
				return;
			}

			switch (DataPoint.MeasurementDefinition.MeasurementType) {
				case MeasurementType.Bool: {
					bool receivedValue = mr.ReadAsBool();
					SetPillarHeight(receivedValue ? 1f : 0f, DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum, 0,
						barFrame.localScale.y);
					SetDisplayValue(receivedValue);
					break;
				}
				case MeasurementType.Float: {
					float receivedValue = mr.ReadAsFloat();
					SetPillarHeight(receivedValue, DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum, 0,
						barFrame.localScale.y);
					SetDisplayValue(receivedValue);
					SetMinMaxDisplayValues(DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum);
					break;
				}
			}

			barFillMeshRenderer.material.color = Options.fillColor;
			SetAuthorImage();
		}

		private void SetAuthorImage() {
			if (DataPoint.CurrentMeasurementResult.Author != string.Empty) {
				authorIconImageRenderer.sprite = DataPoint.AuthorRepository.AuthorSprites[DataPoint.CurrentMeasurementResult.Author];
				authorIconImageRenderer.enabled = true;
			}
			else {
				authorIconImageRenderer.enabled = false;
			}
		}

		private void ShowUserDirectionCanvas() {
			canvasGroup.alpha = 1;
		}

		private void HideAllUserDirectionCanvas() {
			canvasGroup.alpha = 0;
		}

		private void OnDisable() {
			TimeSeries.TimeSeriesBeforeSpawn -= RotateVisualization;
			TimeSeries.TimeSeriesDespawned -= ResetRotation;
		}

#endregion

	}

}