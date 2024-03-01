using System.Collections;
using System.Globalization;
using System.Linq;
using DataskopAR.Data;
using DataskopAR.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DataskopAR.Entities.Visualizations {

	public class Bubble : Visualization {

#region Fields

		[Header("References")]
		[SerializeField] private Image visImageRenderer;
		[SerializeField] private Transform visTransform;
		[SerializeField] private BubbleOptions options;
		[SerializeField] private BubbleTimeSeries bubbleTimeSeries;
		[SerializeField] private Transform dropShadow;
		[SerializeField] private LineRenderer groundLine;
		[SerializeField] private LineRenderer labelLine;
		[SerializeField] private Image authorIconImageRenderer;

		[Header("Display References")]
		[SerializeField] private Transform dataDisplay;
		[SerializeField] private CanvasGroup dataDisplayGroup;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;

		[Header("Additional Values")]
		[HideInInspector] public float maxScale;
		[HideInInspector] public float minScale;

		[SerializeField] private AnimationCurve curve;
		private Coroutine scaleRoutine;
		private Coroutine groundLineRoutine;
		private Coroutine labelRoutine;
		private Coroutine labelLineRoutineLower;
		private Coroutine labelLineRoutineUpper;
		private Vector3 prevScale;

#endregion

#region Properties

		public VisualizationType Type => VisualizationType.bubble;
		private float BubbleSize { get; set; }
		private BubbleOptions Options { get; set; }
		public override MeasurementType[] AllowedMeasurementTypes { get; set; } = {
			MeasurementType.Float,
			MeasurementType.Bool
		};
		private BubbleTimeSeries TimeSeries => bubbleTimeSeries;
		private float MaxScale => maxScale;
		private float MinScale => minScale;
		public override Transform VisTransform => visTransform;
		private Vector3 DisplayOrigin { get; set; }

#endregion

#region Methods

		protected override void OnDataPointChanged() {
			base.OnDataPointChanged();
			Options = Instantiate(options);

			visImageRenderer.enabled = false;

			VisTransform.localPosition = Offset;

			Transform displayTransform = dataDisplay.transform;
			displayTransform.localScale *= Scale;
			displayTransform.localPosition = Offset;
			DisplayOrigin = displayTransform.localPosition;

			idTextMesh.text = DataPoint.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();
			OnMeasurementResultChanged(DataPoint.CurrentMeasurementResult);

			groundLine.startWidth = 0.0075f;
			groundLine.endWidth = 0.0075f;
			groundLine.SetPosition(1, dropShadow.localPosition);

			labelLine.startWidth = 0.0075f;
			labelLine.endWidth = 0.0075f;
		}

		public override void OnMeasurementResultsUpdated() {
			OnMeasurementResultChanged(DataPoint.MeasurementDefinition.GetLatestMeasurementResult());
		}

		public override void ApplyStyle() {
			dropShadow.gameObject.SetActive(DataPoint.Vis.VisOption.Style.HasDropShadow);
			groundLine.gameObject.SetActive(DataPoint.Vis.VisOption.Style.HasGroundLine);
		}

		public override void OnMeasurementResultChanged(MeasurementResult mr) {
			if (!AllowedMeasurementTypes.Contains(DataPoint.MeasurementDefinition.MeasurementType)) {
				NotificationHandler.Add(new Notification() {
					Category = NotificationCategory.Error,
					Text = "Value Type not supported by this visualization.",
					DisplayDuration = 5f
				});
				return;
			}

			switch (DataPoint.MeasurementDefinition.MeasurementType) {

				case MeasurementType.Float: {
					float receivedValue = mr.ReadAsFloat();
					valueTextMesh.text = receivedValue.ToString(CultureInfo.InvariantCulture) + DataPoint.Attribute?.Unit;
					dateTextMesh.text = mr.GetTime();
					SetBubbleSize(receivedValue, DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum, MinScale, MaxScale);
					break;
				}

				case MeasurementType.Bool: {
					bool receivedValue = mr.ReadAsBool();
					valueTextMesh.text = receivedValue.ToString();
					dateTextMesh.text = mr.GetTime();
					SetBubbleSize(receivedValue ? 1 : 0, 0, 1, MinScale, MaxScale);
					break;
				}

			}

			SetAuthorImage();

		}

		private void SetBubbleSize(float value, float minValue, float maxValue, float minScale, float maxScale) {

			value = Mathf.Clamp(value, minValue, maxValue);
			
			// calc min and max area out of desired min and max scale
			float minArea = Mathf.PI * Mathf.Pow(minScale, 2);
			float maxArea = Mathf.PI * Mathf.Pow(maxScale, 2);

			// calc mapped area from the value
			float newArea = MathExtensions.Map(value, minValue, maxValue, minArea, maxArea);

			// calc radius out of the area
			BubbleSize = Mathf.Sqrt(newArea / Mathf.PI);

			if (!visImageRenderer.enabled) {
				VisTransform.localScale = new Vector3(BubbleSize, BubbleSize, BubbleSize);
				visImageRenderer.enabled = true;
			}
			else {

				if (scaleRoutine != null) {
					StopCoroutine(scaleRoutine);
					VisTransform.localScale = new Vector3(BubbleSize, BubbleSize, BubbleSize);
				}

				scaleRoutine = StartCoroutine(LerperHelper.TransformLerpOnCurve(VisTransform, TransformValue.Scale, VisTransform.localScale,
					new Vector3(BubbleSize, BubbleSize, BubbleSize), 0.1f, curve, null));

			}

			dropShadow.transform.localScale = new Vector3(BubbleSize * 0.4f, BubbleSize * 0.4f, BubbleSize * 0.4f);

		}

		private void OnBubbleSizeChanged() {

			groundLineRoutine = StartCoroutine(MoveLinePointTo(groundLine, 0,
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - visImageRenderer.sprite.bounds.size.y * 0.75f,
					VisTransform.localPosition.z),
				0.1f));

			if (TimeSeries.IsSpawned) {
				return;
			}

			if (dataDisplay.transform.localScale.y > VisTransform.localScale.y) {

				labelRoutine = StartCoroutine(LerperHelper.TransformLerp(dataDisplay.transform, TransformValue.Position,
					dataDisplay.transform.localPosition,
					new Vector3(DisplayOrigin.x + visImageRenderer.sprite.bounds.size.x * 0.5f + 0.2f, DisplayOrigin.y, DisplayOrigin.z),
					0.1f,
					null));

				if (DataPoint.Vis.IsSelected) {

					labelLine.enabled = true;

					labelLineRoutineLower = StartCoroutine(MoveLinePointTo(labelLine, 0,
						new Vector3(VisTransform.localPosition.x + visImageRenderer.sprite.bounds.size.x * 0.75f,
							VisTransform.localPosition.y,
							VisTransform.localPosition.z),
						0.1f));

					labelLineRoutineUpper = StartCoroutine(MoveLinePointTo(labelLine, 1,
						new Vector3(DisplayOrigin.x + visImageRenderer.sprite.bounds.size.x * 0.5f + 0.1f, DisplayOrigin.y,
							DisplayOrigin.z),
						0.1f));
				}

			}
			else {
				labelLine.enabled = false;

				labelRoutine = StartCoroutine(LerperHelper.TransformLerp(dataDisplay.transform, TransformValue.Position,
					dataDisplay.transform.localPosition,
					DisplayOrigin, 0.1f, null));
			}

		}

		private IEnumerator MoveLinePointTo(LineRenderer line, int index, Vector3 target, float duration) {

			float current = 0f;

			while (current <= duration) {

				current += Time.deltaTime;
				float currentPercentage = Mathf.Clamp01(current / duration);

				line.SetPosition(index, Vector3.LerpUnclamped(line.GetPosition(index), target, currentPercentage));

				yield return null;

			}

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

		public override void OnTimeSeriesToggled(bool isActive) {

			if (isActive) {
				groundLine.enabled = false;
				labelLine.enabled = false;
				TimeSeries.SpawnSeries(timeSeriesConfiguration, DataPoint);
			}
			else {
				TimeSeries.DespawnSeries();
				labelLine.enabled = true;
				groundLine.enabled = true;
			}

		}

		public override void Hover() {
			visImageRenderer.material = Options.styles[0].hoverMaterial;
			dataDisplayGroup.alpha = 1;
		}

		public override void Select() {
			visImageRenderer.material = Options.styles[0].selectionMaterial;
			dataDisplayGroup.alpha = 1;
			IsSelected = true;
		}

		public override void Deselect() {
			visImageRenderer.material = Options.styles[0].defaultMaterial;
			dataDisplayGroup.alpha = 0;
			IsSelected = false;
		}

#endregion

	}

}