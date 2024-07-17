using System;
using Dataskop.Data;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public interface IVisualization {

		public event Action SwipedDown;

		public event Action SwipedUp;

		public event Action<int> VisObjectHovered;

		public event Action<int> VisObjectSelected;

		public event Action<int> VisObjectDeselected;

		public IVisObject[] VisObjects { get; set; }

		/// <summary>
		/// The DataPoint this visualization belongs to.
		/// </summary>
		public DataPoint DataPoint { get; set; }

		public VisualizationOption VisOption { get; set; }

		public TimeSeriesConfig TimeSeriesConfig { get; set; }

		public bool IsSelected { get; set; }

		public bool IsInitialized { get; set; }

		public bool HasHistoryEnabled { get; set; }

		/// <summary>
		/// The Transform of the whole Visualization.
		/// </summary>
		public Transform VisOrigin { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; set; }

		public Vector3 Offset { get; set; }

		public float Scale { get; set; }

		public VisualizationType Type { get; set; }

		/// <summary>
		/// Creates a Visualization for a given Data Point.
		/// </summary>
		public void OnDataPointChanged();

		public void OnVisObjectHovered(int index);

		public void OnVisObjectSelected(int index);

		public void OnVisObjectDeselected(int index);

		public void OnTimeSeriesToggled(bool isActive);

		public void OnMeasurementResultsUpdated();

		public void OnMeasurementResultChanged(MeasurementResult mr);

		public void ApplyStyle(VisualizationStyle style);

		public void Swiped(PointerInteraction pointerInteraction);

		/// <summary>
		/// Destroys and cleans up this Visualization.
		/// </summary>
		public void Despawn();

	}

}