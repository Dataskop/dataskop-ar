using System;
using Dataskop.Data;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public abstract class Visualization : MonoBehaviour {

		[Header("Vis Values")]
		[SerializeField] private Vector3 offset;
		[SerializeField] private float scaleFactor;
		[SerializeField] protected TimeSeriesConfig timeSeriesConfiguration;
		[SerializeField] protected Color deselectColor;
		[SerializeField] protected Color hoverColor;
		[SerializeField] protected Color selectColor;
		[SerializeField] protected Color historyColor;

		private DataPoint dataPoint;

		public DataPoint DataPoint {
			get => dataPoint;

			set {
				dataPoint = value;
				if (value != null) {
					OnDataPointChanged();
				}
			}
		}

		public VisualizationOption VisOption { get; set; }

		public bool IsSelected { get; set; }

		public bool IsSpawned => DataPoint != null;

		public abstract Transform VisTransform { get; }

		public abstract MeasurementType[] AllowedMeasurementTypes { get; set; }

		protected IVisObject[] VisObjects { get; set; }

		/// <summary>
		///     The offset of the visualization to the ground.
		/// </summary>
		public Vector3 Offset {
			get => offset;
			set => offset = value;
		}

		/// <summary>
		///     The factor that gets multiplied with the objects default size.
		/// </summary>
		public float Scale {
			get => scaleFactor;
			set => scaleFactor = value;
		}

		public VisualizationType Type { get; protected set; }

		public event Action SwipedDown;

		public event Action SwipedUp;

		public event Action<int> VisObjectHovered;

		public event Action<int> VisObjectSelected;

		public event Action<int> VisObjectDeselected;

		/// <summary>
		///     Creates a visualization for a given Data Point.
		/// </summary>
		protected virtual void OnDataPointChanged() {
			DataPoint.MeasurementResultChanged += OnMeasurementResultChanged;
		}

		/// <summary>
		///     Gets called when the user points the reticule over the visible visualization.
		/// </summary>
		public abstract void OnVisObjectHovered(int index);

		/// <summary>
		///     Gets called when the visualization gets selected.
		/// </summary>
		public abstract void OnVisObjectSelected(int index);

		/// <summary>
		///     Gets called when the visualization gets deselected.
		/// </summary>
		public abstract void OnVisObjectDeselected(int index);

		/// <summary>
		///     Gets called before the visualization is removed.
		/// </summary>
		public virtual void Despawn() {
			DataPoint.MeasurementResultChanged -= OnMeasurementResultChanged;
			DataPoint = null;
			Destroy(gameObject);
		}

		/// <summary>
		///     Gets called when the time view gets toggled.
		/// </summary>
		/// <param name="isActive"></param>
		public abstract void OnTimeSeriesToggled(bool isActive);

		public abstract void OnMeasurementResultsUpdated();

		public abstract void OnMeasurementResultChanged(MeasurementResult mr);

		public abstract void ApplyStyle(VisualizationStyle style);

		public void Swiped(PointerInteraction pointerInteraction) {

			switch (pointerInteraction.Direction.y) {
				case > 0.20f:
					SwipedUp?.Invoke();
					break;
				case < -0.20f:
					SwipedDown?.Invoke();
					break;
			}

		}

	}

}