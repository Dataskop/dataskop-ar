using System.Collections;
using System.Globalization;
using System.Linq;
using DataskopAR.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DataskopAR.Entities.Visualizations {

	public class Dot : Visualization {

#region Fields

		[Header("References")]
		[SerializeField] private Image visImageRenderer;
		[SerializeField] private Transform visTransform;
		[SerializeField] private DotOptions options;
		[SerializeField] private DotTimeSeries dotTimeSeries;
		[SerializeField] private Transform dropShadow;
		[SerializeField] private LineRenderer groundLine;
		[SerializeField] private Image authorIconImageRenderer;
		[SerializeField] private Transform timeElementsContainer;

		[Header("Display References")]
		[SerializeField] private Transform dataDisplay;
		[SerializeField] private TextMeshProUGUI idTextMesh;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI dateTextMesh;

		[Header("Icon Values")]
		[SerializeField] private Image boolIcon;
		[SerializeField] private Sprite[] boolIcons;
		[SerializeField] private Color32 boolTrueColor;
		[SerializeField] private Color32 boolFalseColor;

		[Header("Animation Values")]
		[SerializeField] private AnimationCurve animationCurveSelect;
		[SerializeField] private AnimationCurve animationCurveDeselect;
		[SerializeField] private float animationTimeOnSelect;
		[SerializeField] private float animationTimeOnDeselect;
		[SerializeField] private float selectionScale;

		private Coroutine animationCoroutine;
		private Vector3 animationTarget;
		private Coroutine moveLineCoroutine;

#endregion

#region Properties

		public VisualizationType Type => VisualizationType.dot;

		private DotOptions Options { get; set; }

		private DotTimeSeries TimeSeries => dotTimeSeries;

		public override Transform VisTransform => visTransform;

		public override MeasurementType[] AllowedMeasurementTypes { get; set; } = {
			MeasurementType.Float,
			MeasurementType.Bool
		};

#endregion

#region Methods

		protected override void OnDataPointChanged() {

			base.OnDataPointChanged();
			Options = Instantiate(options);

			Transform displayTransform = dataDisplay.transform;

			VisTransform.localScale *= Scale;
			dropShadow.transform.localScale *= Scale;
			displayTransform.localScale *= Scale;

			VisTransform.root.localPosition = Offset;
			dropShadow.transform.localPosition -= Offset;

			SetLinePosition(groundLine,
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - visImageRenderer.sprite.bounds.size.y * 0.75f,
					VisTransform.localPosition.z),
				dropShadow.localPosition);

			groundLine.startWidth = 0.0075f;
			groundLine.endWidth = 0.0075f;

			idTextMesh.text = DataPoint.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();
			OnMeasurementResultChanged(DataPoint.CurrentMeasurementResult);

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
					boolIcon.enabled = false;
					valueTextMesh.alpha = 1;
					valueTextMesh.text = receivedValue.ToString(CultureInfo.InvariantCulture) + DataPoint.Attribute?.Unit;
					dateTextMesh.text = mr.GetTime();
					break;
				}
				case MeasurementType.Bool: {
					valueTextMesh.alpha = 1;
					boolIcon.enabled = false;
					valueTextMesh.text = mr.ReadAsBool().ToString();
					int boolValue = mr.ReadAsBool() ? 1 : 0;
					boolIcon.color = mr.ReadAsBool() ? boolTrueColor : boolFalseColor;
					boolIcon.sprite = boolIcons[boolValue];
					dateTextMesh.text = mr.GetTime();
					break;
				}
			}

			SetAuthorImage();

		}

		public override void ApplyStyle(VisualizationStyle style) {
			dropShadow.gameObject.SetActive(style.HasDropShadow);
			groundLine.gameObject.SetActive(style.HasGroundLine);
		}

		public override void OnMeasurementResultsUpdated() {
			OnMeasurementResultChanged(DataPoint.MeasurementDefinition.GetLatestMeasurementResult());
		}

		public override void OnTimeSeriesToggled(bool isActive) {

			if (isActive) {
				groundLine.enabled = false;
				TimeSeries.SpawnSeries(timeSeriesConfiguration, DataPoint, timeElementsContainer);
			}
			else {
				groundLine.enabled = true;
				TimeSeries.DespawnSeries();
			}

		}

		public override void Hover() {
			visImageRenderer.material = Options.styles[0].hoverMaterial;
		}

		public override void Select() {

			if (animationCoroutine != null) {
				CancelAnimation();
			}

			animationTarget = visTransform.localScale * selectionScale;

			animationCoroutine = StartCoroutine(Lerper.TransformLerpOnCurve(
				visTransform,
				TransformValue.Scale,
				VisTransform.localScale,
				animationTarget,
				animationTimeOnSelect,
				animationCurveSelect,
				OnScaleChanged
			));

			visImageRenderer.material = Options.styles[0].selectionMaterial;
			IsSelected = true;

		}

		public override void Deselect() {

			if (IsSelected) {
				if (animationCoroutine != null) {
					CancelAnimation();
				}

				animationTarget = visTransform.localScale / selectionScale;

				animationCoroutine = StartCoroutine(Lerper.TransformLerpOnCurve(
					visTransform,
					TransformValue.Scale,
					VisTransform.localScale,
					animationTarget,
					animationTimeOnDeselect,
					animationCurveDeselect,
					OnScaleChanged
				));
			}

			visImageRenderer.material = Options.styles[0].defaultMaterial;
			IsSelected = false;

		}

		private void CancelAnimation() {
			StopCoroutine(animationCoroutine);
			VisTransform.localScale = animationTarget;
		}

		private void SetLinePosition(LineRenderer lr, Vector3 startPoint, Vector3 endPoint) {
			lr.SetPosition(0, startPoint);
			lr.SetPosition(1, endPoint);
		}

		private void OnScaleChanged() {
			moveLineCoroutine = StartCoroutine(MoveLinePointTo(0,
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - visImageRenderer.sprite.bounds.size.y * 0.75f,
					VisTransform.localPosition.z),
				0.1f));
		}

		private IEnumerator MoveLinePointTo(int index, Vector3 target, float duration) {
			float current = 0f;

			while (current <= duration) {
				current += Time.deltaTime;
				float currentPercentage = Mathf.Clamp01(current / duration);

				groundLine.SetPosition(index, Vector3.LerpUnclamped(groundLine.GetPosition(index), target, currentPercentage));

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

#endregion

	}

}