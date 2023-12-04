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
		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private DotOptions options;
		[SerializeField] private DotTimeSeries dotTimeSeries;
		[SerializeField] private Transform dropShadow;
		[SerializeField] private Transform visTransform;
		[SerializeField] private Transform timeSeriesTransform;
		[SerializeField] private LineRenderer groundLine;
		[SerializeField] private SpriteRenderer authorIconSpriteRenderer;

		[Header("Display References")]
		[SerializeField] private Canvas dataDisplay;
		[SerializeField] private CanvasGroup dataDisplayGroup;
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

		private Coroutine animationCoroutine;
		private Vector3 animationTarget;
		private Coroutine moveLineCoroutine;

#endregion

#region Properties

		public VisualizationType Type => VisualizationType.dot;
		private DotOptions Options { get; set; }
		private DotTimeSeries TimeSeries => dotTimeSeries;
		public override MeasurementType[] AllowedMeasurementTypes { get; set; } = { MeasurementType.Float, MeasurementType.Bool };

#endregion

#region Methods

		private void FixedUpdate() {
			if (IsSpawned) {
				AlignWithCamera();
			}
		}

		private void AlignWithCamera() {
			if (Vector3.Distance(ARCamera.transform.position, visTransform.position) < 50f) {
				visTransform.LookAt(new Vector3(ARCamera.transform.position.x, visTransform.position.y, ARCamera.transform.position.z),
					Vector3.up);
				dataDisplay.transform.LookAt(
					new Vector3(ARCamera.transform.position.x, dataDisplay.transform.position.y, ARCamera.transform.position.z),
					Vector3.up);
				timeSeriesTransform.transform.LookAt(
					new Vector3(ARCamera.transform.position.x, timeSeriesTransform.transform.position.y, ARCamera.transform.position.z),
					Vector3.up);

			}
		}

		public override void Create(DataPoint dataPoint) {
			DataPoint = dataPoint;
			Options = Instantiate(options);
			DataPoint.MeasurementResultChanged += OnMeasurementResultChanged;

			VisTransform = visTransform;
			Transform displayTransform = dataDisplay.transform;
			dataDisplay.worldCamera = ARCamera;

			VisTransform.localScale *= Scale;
			dropShadow.transform.localScale *= Scale;
			displayTransform.localScale *= Scale;

			VisTransform.localPosition = Offset;
			displayTransform.localPosition = Offset;

			SetLinePosition(groundLine,
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - spriteRenderer.bounds.size.y * 0.75f,
					VisTransform.localPosition.z),
				dropShadow.localPosition);
			groundLine.startWidth = 0.0075f;
			groundLine.endWidth = 0.0075f;

			idTextMesh.text = DataPoint.MeasurementDefinition.MeasurementDefinitionInformation.Name.ToUpper();
			OnMeasurementResultChanged(DataPoint.CurrentMeasurementResult);
			IsSpawned = true;

		}

		public override void OnMeasurementResultChanged(MeasurementResult mr) {

			if (!AllowedMeasurementTypes.Contains(DataPoint.MeasurementDefinition.MeasurementType)) {
				NotificationHandler.Add(new Notification() {
					Category = NotificationCategory.Error, Text = "Value Type not supported by this visualization.", DisplayDuration = 5f
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

		public override void ApplyStyle() {
			dropShadow.gameObject.SetActive(DataPoint.Vis.VisOption.Style.HasDropShadow);
			groundLine.gameObject.SetActive(DataPoint.Vis.VisOption.Style.HasGroundLine);
		}

		public override void OnMeasurementResultsUpdated() {
			OnMeasurementResultChanged(DataPoint.MeasurementDefinition.GetLatestMeasurementResult());
		}

		public override void OnTimeSeriesToggled(bool isActive) {
			if (isActive) {
				groundLine.enabled = false;
				TimeSeries.SpawnSeries(timeSeriesConfiguration, DataPoint);
			}
			else {
				groundLine.enabled = true;
				TimeSeries.DespawnSeries();
			}
		}

		public override void Hover() {
			spriteRenderer.material = Options.materialOptions[0].Hovered;
			dataDisplayGroup.alpha = 1;
		}

		public override void Select() {

			if (animationCoroutine != null) {
				CancelAnimation();
			}

			animationTarget = visTransform.localScale * 1.35f;

			animationCoroutine = StartCoroutine(LerperHelper.TransformLerpOnCurve(
				visTransform,
				TransformValue.Scale,
				VisTransform.localScale,
				animationTarget,
				animationTimeOnSelect,
				animationCurveSelect,
				OnScaleChanged
			));

			spriteRenderer.material = Options.materialOptions[0].Selected;
			dataDisplayGroup.alpha = 1;
			IsSelected = true;
		}

		public override void Deselect() {

			if (IsSelected) {

				if (animationCoroutine != null) {
					CancelAnimation();
				}

				animationTarget = visTransform.localScale / 1.35f;

				animationCoroutine = StartCoroutine(LerperHelper.TransformLerpOnCurve(
					visTransform,
					TransformValue.Scale,
					VisTransform.localScale,
					animationTarget,
					animationTimeOnDeselect,
					animationCurveDeselect,
					OnScaleChanged
				));

			}

			spriteRenderer.material = Options.materialOptions[0].Deselected;
			dataDisplayGroup.alpha = 0;
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
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - spriteRenderer.bounds.size.y * 0.75f,
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
				authorIconSpriteRenderer.sprite = DataPoint.AuthorRepository.AuthorSprites[DataPoint.CurrentMeasurementResult.Author];
				authorIconSpriteRenderer.enabled = true;
			}
			else {
				authorIconSpriteRenderer.enabled = false;
			}

		}

#endregion

	}

}