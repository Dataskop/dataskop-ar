using System;
using DataskopAR.Data;
using DataskopAR.Interaction;
using UnityEngine;

namespace DataskopAR.Entities.Visualizations {

	public abstract class Visualization : MonoBehaviour {

#region Events

		public Action SwipedUp;
		public Action SwipedDown;

#endregion

#region Fields

		[Header("Vis Values")]
		[SerializeField] private Vector3 offset;
		[SerializeField] private float scaleFactor;
		[SerializeField] protected TimeSeriesConfig timeSeriesConfiguration;

		private DataPoint dataPoint;

#endregion

#region Properties

		public DataPoint DataPoint {
			get => dataPoint;
			set {
				dataPoint = value;
				if (value != null)
					OnDatapointChanged();
			}
		}

		public VisualizationOption VisOption { get; set; }
		public Camera ARCamera { get; set; }
		public bool IsSelected { get; set; }
		public bool IsSpawned => DataPoint != null;
		public abstract Transform VisTransform { get; }
		public abstract MeasurementType[] AllowedMeasurementTypes { get; set; }

		/// <summary>
		/// The offset of the visualization to the ground.
		/// </summary>
		public Vector3 Offset {
			get => offset;
			set => offset = value;
		}

		/// <summary>
		/// The factor that gets multiplied with the objects default size.
		/// </summary>
		public float Scale {
			get => scaleFactor;
			set => scaleFactor = value;
		}

#endregion

#region Methods

		public void Start() {
			ARCamera = Camera.main;
		}

		/// <summary>
		///  Creates a visualization for a given Data Point.
		/// </summary>
		protected virtual void OnDatapointChanged() {
			DataPoint.MeasurementResultChanged += OnMeasurementResultChanged;
		}

		/// <summary>
		/// Gets called when the user points the reticule over the visible visualization.
		/// </summary>
		public abstract void Hover();

		/// <summary>
		/// Gets called when the visualization gets selected.
		/// </summary>
		public abstract void Select();

		/// <summary>
		/// Gets called when the visualization gets deselected.
		/// </summary>
		public abstract void Deselect();

		/// <summary>
		/// Gets called before the visualization is removed.
		/// </summary>
		public virtual void Despawn() {
			DataPoint.MeasurementResultChanged -= OnMeasurementResultChanged;
			DataPoint = null;
			Destroy(gameObject);
		}

		/// <summary>
		/// Gets called when the time view gets toggled.
		/// </summary>
		/// <param name="isActive"></param>
		public abstract void OnTimeSeriesToggled(bool isActive);

		public abstract void OnMeasurementResultsUpdated();

		public abstract void OnMeasurementResultChanged(MeasurementResult mr);

		public abstract void ApplyStyle();

		public void Swiped(Swipe swipe) {

			if (UIInteractionDetection.IsPointerOverUi && !UIInteractionDetection.HasPointerStartedOverSlider)
				return;

			if (swipe.StartingGameObject == null)
				return;

			if (!swipe.StartingGameObject.CompareTag("Vis")) return;

			switch (swipe.Direction.y) {
				case > 0f:
					SwipedUp?.Invoke();
					break;
				case < 0f:
					SwipedDown?.Invoke();
					break;
			}

		}

#endregion

	}

}