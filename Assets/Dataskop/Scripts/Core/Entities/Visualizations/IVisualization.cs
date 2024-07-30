using System;
using Dataskop.Data;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public interface IVisualization {

		public IVisObject[] VisObjects { get; set; }

		public IVisObject FocusedVisObject { get; }

		/// <summary>
		///     The DataPoint this visualization belongs to.
		/// </summary>
		public DataPoint DataPoint { get; set; }

		public VisualizationOption VisOption { get; set; }

		public VisHistoryConfiguration VisHistoryConfiguration { get; set; }

		public bool IsSelected { get; set; }

		public bool IsInitialized { get; set; }

		public bool HasHistoryEnabled { get; set; }

		/// <summary>
		///     The Transform of the whole Visualization.
		/// </summary>
		public Transform VisOrigin { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; set; }

		public Vector3 Offset { get; set; }

		public float Scale { get; set; }

		public VisualizationType Type { get; set; }

		public int PreviousIndex { get; set; }

		public IVisObjectStyle VisObjectStyle { get; set; }

		public event Action SwipedDown;

		public event Action SwipedUp;

		public event Action<int> VisObjectHovered;

		public event Action<int> VisObjectSelected;

		public event Action<int> VisObjectDeselected;

		public event Action<IVisObject> FocusedVisObjectChanged;

		/// <summary>
		///     Creates a Visualization for a given Data Point.
		/// </summary>
		public void Initialize(DataPoint dp);

		public void OnVisObjectHovered(int index);

		public void OnVisObjectSelected(int index);

		public void OnVisObjectDeselected(int index);

		public void OnTimeSeriesToggled(bool isActive);

		public void OnFocusedIndexChanged(MeasurementDefinition def, int index);

		public void OnSwipeInteraction(PointerInteraction pointerInteraction);

		public void ApplyStyle(VisualizationStyle style);

		/// <summary>
		///     Destroys and cleans up this Visualization.
		/// </summary>
		public void Despawn();

	}

}