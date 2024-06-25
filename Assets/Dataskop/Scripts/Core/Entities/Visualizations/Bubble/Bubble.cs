using System.Collections;
using System.Globalization;
using System.Linq;
using Dataskop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class Bubble : Visualization {

 

		[Header("References")]
		[SerializeField] private Image visImageRenderer;
		[SerializeField] private Transform visTransform;
		[SerializeField] private BubbleOptions options;
		[SerializeField] private BubbleTimeSeries bubbleTimeSeries;
		[SerializeField] private Transform dropShadow;
		[SerializeField] private LineRenderer groundLine;
		[SerializeField] private LineRenderer labelLine;
		[SerializeField] private Transform timeElementsContainer;

		[Header("Display References")]
		[SerializeField] private Transform dataDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;
		[SerializeField] private Image authorIconImageRenderer;

		[Header("Additional Values")]
		[HideInInspector] public float minScale;
		[HideInInspector] public float maxScale;
		[SerializeField] private AnimationCurve curve;

		private Coroutine scaleRoutine;
		private Coroutine groundLineRoutine;
		private Coroutine labelRoutine;
		private Coroutine labelLineRoutineLower;
		private Coroutine labelLineRoutineUpper;
		private Vector3 prevScale;
		private float bubbleSize;

  

 

		private float BubbleSize {
			get => bubbleSize;
			set {
				bubbleSize = value;
				OnBubbleSizeChanged();
			}
		}

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

  

 

		protected override void OnDataPointChanged() {
			base.OnDataPointChanged();
			Type = VisualizationType.Bubble;
			Options = Instantiate(options);

			visImageRenderer.enabled = false;

			VisTransform.root.localPosition = Offset;

			Transform displayTransform = dataDisplay.transform;
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

		public override void ApplyStyle(VisualizationStyle style) {
			dropShadow.gameObject.SetActive(style.HasDropShadow);
			groundLine.gameObject.SetActive(style.HasGroundLine);
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

				case MeasurementType.Float: {
					float receivedValue = mr.ReadAsFloat();
					valueTextMesh.text = receivedValue.ToString(CultureInfo.InvariantCulture) + $" {DataPoint.Attribute?.Unit}";
					dateTextMesh.text = mr.GetTime();
					BubbleSize = BubbleUtils.CalculateRadius(receivedValue, DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum,
						MinScale, MaxScale);
					break;
				}

				case MeasurementType.Bool: {
					bool receivedValue = mr.ReadAsBool();
					valueTextMesh.text = receivedValue.ToString();
					dateTextMesh.text = mr.GetTime();
					BubbleSize = BubbleUtils.CalculateRadius(receivedValue ? 1 : 0, 0, 1, MinScale, MaxScale);
					break;
				}

			}

			SetAuthorImage();

		}

		private void OnBubbleSizeChanged() {

			Vector3 newBubbleScale = new(BubbleSize, BubbleSize, BubbleSize);

			if (!visImageRenderer.enabled) {
				VisTransform.localScale = newBubbleScale;
				dataDisplay.localScale = newBubbleScale;
				authorIconImageRenderer.transform.localScale = newBubbleScale * 0.85f;
				visImageRenderer.enabled = true;
			}
			else {

				if (scaleRoutine != null) {
					StopCoroutine(scaleRoutine);
				}

				scaleRoutine = StartCoroutine(Lerper.TransformLerpOnCurve(VisTransform, TransformValue.Scale, VisTransform.localScale,
					newBubbleScale, 0.12f, curve, null));
				dataDisplay.localScale = newBubbleScale;

			}

			dropShadow.transform.localScale = newBubbleScale;

			groundLineRoutine = StartCoroutine(MoveLinePointTo(groundLine, 0,
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - visImageRenderer.sprite.bounds.size.y * 0.75f,
					VisTransform.localPosition.z),
				0.1f));

			if (TimeSeries.IsSpawned) {
				return;
			}

			if (dataDisplay.transform.localScale.y > VisTransform.localScale.y) {

				labelRoutine = StartCoroutine(Lerper.TransformLerp(dataDisplay.transform, TransformValue.Position,
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

				labelRoutine = StartCoroutine(Lerper.TransformLerp(dataDisplay.transform, TransformValue.Position,
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
				TimeSeries.Spawn(timeSeriesConfiguration, DataPoint, timeElementsContainer);
			}
			else {
				TimeSeries.DespawnSeries();
				labelLine.enabled = true;
				groundLine.enabled = true;
			}

		}

		public override void Hover() {
			visImageRenderer.material = Options.styles[0].hoverMaterial;
			valueTextMesh.color = hoverColor;
		}

		public override void Select() {
			visImageRenderer.material = Options.styles[0].selectionMaterial;
			valueTextMesh.color = selectColor;
			IsSelected = true;
		}

		public override void Deselect() {
			visImageRenderer.material = Options.styles[0].defaultMaterial;
			valueTextMesh.color = deselectColor;
			IsSelected = false;
		}

  

	}

}