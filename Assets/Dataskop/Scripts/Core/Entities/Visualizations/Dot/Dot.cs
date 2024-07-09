using System.Collections;
using System.Linq;
using Dataskop.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Dataskop.Entities.Visualizations {

	public class Dot : Visualization {

		[Header("References")]
		[SerializeField] private GameObject visPrefab;
		[SerializeField] private DotOptions options;
		[SerializeField] private DotTimeSeries dotTimeSeries;
		[SerializeField] private Transform dropShadow;
		[SerializeField] private LineRenderer groundLine;

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

		private DotOptions Options { get; set; }

		private DotTimeSeries TimeSeries => dotTimeSeries;

		public override Transform VisTransform => transform;

		public override MeasurementType[] AllowedMeasurementTypes { get; set; } = {
			MeasurementType.Float,
			MeasurementType.Bool
		};

		private int CurrentFocusIndex { get; set; } = 0;

		protected override void OnDataPointChanged() {

			base.OnDataPointChanged();
			Type = VisualizationType.Dot;
			Options = Instantiate(options);

			VisObjects = new IVisObject[TimeSeries.Configuration.visibleHistoryCount];
			GameObject visObject = Instantiate(visPrefab, transform);
			VisObjects[0] = visObject.GetComponent<IVisObject>();

			VisTransform.localScale *= Scale;
			dropShadow.transform.localScale *= Scale;
			VisTransform.root.localPosition = Offset;
			dropShadow.transform.localPosition -= Offset;

			/*
			SetLinePosition(groundLine,
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - visImageRenderer.sprite.bounds.size.y * 0.75f,
					VisTransform.localPosition.z),
				dropShadow.localPosition);

			groundLine.startWidth = 0.0075f;
			groundLine.endWidth = 0.0075f;
			*/

			OnMeasurementResultChanged(DataPoint.CurrentMeasurementResult);

		}

		public override void OnMeasurementResultChanged(MeasurementResult mr) {

			if (!AllowedMeasurementTypes.Contains(mr.MeasurementDefinition.MeasurementType)) {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "Value Type not supported by this visualization.",
					DisplayDuration = 5f
				});
				return;
			}

			VisObjects[0].SetDisplayData(new VisualizationResultDisplayData {
				Result = mr,
				Type = mr.MeasurementDefinition.MeasurementType,
				Attribute = DataPoint.Attribute,
				AuthorSprite = DataPoint.AuthorRepository.AuthorSprites[mr.Author]
			});

		}

		public override void ApplyStyle(VisualizationStyle style) {
			dropShadow.gameObject.SetActive(style.HasDropShadow);
			groundLine.gameObject.SetActive(style.HasGroundLine);
		}

		public override void OnMeasurementResultsUpdated() {
			OnMeasurementResultChanged(DataPoint.MeasurementDefinition.GetLatestMeasurementResult());

			if (TimeSeries.IsSpawned) {
				TimeSeries.OnMeasurementResultsUpdated(DataPoint.MeasurementDefinition.MeasurementResults.ToArray());
			}

		}

		public override void OnTimeSeriesToggled(bool isActive) {

			if (isActive) {
				groundLine.enabled = false;
				TimeSeries.Spawn(timeSeriesConfiguration, DataPoint, transform);
			}
			else {
				groundLine.enabled = true;
				TimeSeries.DespawnSeries();
			}

		}

		public override void Hover() {
			//visImageRenderer.material = Options.styles[0].hoverMaterial;
			//valueTextMesh.color = hoverColor;
		}

		public override void Select() {

			if (animationCoroutine != null) {
				CancelAnimation();
			}

			//animationTarget = visTransform.localScale * selectionScale;

			/*
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
			//valueTextMesh.color = selectColor;
			*/
			IsSelected = true;

		}

		public override void Deselect() {

			if (IsSelected) {
				if (animationCoroutine != null) {
					CancelAnimation();
				}
			}
			//animationTarget = visTransform.localScale / selectionScale;

			/*
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
		*/
			//valueTextMesh.color = deselectColor;
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
			/*
			moveLineCoroutine = StartCoroutine(MoveLinePointTo(0,
				new Vector3(VisTransform.localPosition.x, VisTransform.localPosition.y - visImageRenderer.sprite.bounds.size.y * 0.75f,
					VisTransform.localPosition.z),
				0.1f));
				*/
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

	}

}