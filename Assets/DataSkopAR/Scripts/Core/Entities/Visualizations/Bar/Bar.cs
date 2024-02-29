using System.Globalization;
using System.Linq;
using DataskopAR.Data;
using DataskopAR.Utils;
using TMPro;
using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	public class Bar : Visualization {

#region Fields

		[Header("References")] [SerializeField]
		private MeshRenderer pillarFillMeshRenderer;

		[SerializeField] private MeshRenderer pillarFrameMeshRenderer;
		[SerializeField] private Transform barFill;
		[SerializeField] private Transform barFrame;
		[SerializeField] private BarOptions options;
		[SerializeField] private BarTimeSeries barTimeSeries;
		[SerializeField] private BoxCollider barCollider;
		[SerializeField] private Transform canvasRotationAnchor;
		[SerializeField] private Transform visTransform;
		[SerializeField] private SpriteRenderer authorIconSpriteRenderer;

		[Header("Display References")] [SerializeField]
		private Canvas dataDisplay;

		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private TextMeshProUGUI valueTextMesh;
		[SerializeField] private TextMeshProUGUI minValueTextMesh;
		[SerializeField] private TextMeshProUGUI maxValueTextMesh;

		private Vector3 origin;

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

#endregion

#region Methods

		private void Awake() {
			TimeSeries.TimeSeriesBeforeSpawn += RotateVisualization;
			TimeSeries.TimeSeriesDespawned += ResetRotation;
		}

/*
        private void Update() {

                    if (Vector3.Distance(ARCamera.transform.position, canvasRotationAnchor.position) > 10f) {
                        return;
                    }


                    Vector3 targetDir = new(ARCamera.transform.position.x - dataDisplay.transform.position.x, 0,
                        ARCamera.transform.position.z - dataDisplay.transform.position.z);
                    float singleStep = 2.4f * Time.deltaTime;
                    //Vector3 newDir = Vector3.RotateTowards(dataDisplay.transform.forward, targetDir, singleStep, 0.0f);
                    //dataDisplay.transform.rotation = Quaternion.LookRotation(newDir);

        }
*/

		protected override void OnDataPointChanged() {
			base.OnDataPointChanged();
			Options = Instantiate(options);

			VisTransform.localScale *= Scale;
			dataDisplay.transform.localScale *= Scale;
			dataDisplay.transform.localPosition = new Vector3(0, dataDisplay.transform.localScale.y, 0);

			dataDisplay.worldCamera = ARCamera;
			OnMeasurementResultChanged(DataPoint.CurrentMeasurementResult);
			barCollider.enabled = true;
		}

		private void SetPillarHeight(float heightValue, float minValue, float maxValue) {
			heightValue = Mathf.Clamp(heightValue, minValue, maxValue);
			BarHeight = MathExtensions.Map01(heightValue, minValue, maxValue);
			Vector3 localScale = barFill.localScale;
			BarFillScale = new Vector3(localScale.x, BarHeight, localScale.z);
			localScale = BarFillScale;
			barFill.localScale = localScale;
		}

		private void SetDisplayValue(float value) {
			valueTextMesh.text = value.ToString("00.00", CultureInfo.InvariantCulture) + DataPoint.Attribute?.Unit;
		}

		private void SetDisplayValue(bool value) {
			valueTextMesh.text = value.ToString();
		}

		private void SetMinMaxDisplayValues(float min, float max) {
			minValueTextMesh.text = min.ToString("00.00", CultureInfo.InvariantCulture) + DataPoint.Attribute?.Unit;
			maxValueTextMesh.text = max.ToString("00.00", CultureInfo.InvariantCulture) + DataPoint.Attribute?.Unit;
		}

		private void RotateVisualization() {
			transform.SetPositionAndRotation(transform.position + new Vector3(0, 0.25f, 0), Quaternion.Euler(new Vector3(0, 0, 90)));
		}

		private void ResetRotation() {
			transform.SetPositionAndRotation(transform.position - new Vector3(0, 0.25f, 0), Quaternion.Euler(new Vector3(0, 0, 0)));
		}

		private void PositionVisualization(Vector3 offset) {
			transform.localPosition += offset;
		}

		public override void ApplyStyle() { }

		public override void Hover() {
			pillarFrameMeshRenderer.material = Options.styles[0].hoverMaterial;
			ShowUserDirectionCanvas();
		}

		public override void Select() {
			pillarFrameMeshRenderer.material = Options.styles[0].selectionMaterial;
			ShowUserDirectionCanvas();
			IsSelected = true;
		}

		public override void Deselect() {
			pillarFrameMeshRenderer.material = Options.styles[0].defaultMaterial;
			HideAllUserDirectionCanvas();
			IsSelected = false;
		}

		public override void OnTimeSeriesToggled(bool isActive) {
			if (isActive) {
				TimeSeries.SpawnSeries(timeSeriesConfiguration, DataPoint);
			}
			else {
				TimeSeries.DespawnSeries();
			}
		}

		public override void OnMeasurementResultsUpdated() {
			OnMeasurementResultChanged(DataPoint.MeasurementDefinition.GetLatestMeasurementResult());
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
				case MeasurementType.Bool: {
					bool receivedValue = mr.ReadAsBool();
					SetPillarHeight(receivedValue ? 1f : 0f, DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum);
					SetDisplayValue(receivedValue);
					break;
				}
				case MeasurementType.Float: {
					float receivedValue = mr.ReadAsFloat();
					SetPillarHeight(receivedValue, DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum);
					SetDisplayValue(receivedValue);
					SetMinMaxDisplayValues(DataPoint.Attribute.Minimum, DataPoint.Attribute.Maximum);
					break;
				}
			}

			pillarFillMeshRenderer.material.color = Options.fillColor;
			SetAuthorImage();
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