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

		public DataPoint DataPoint { get; set; }

		public VisualizationOption VisOption { get; set; }

		public bool IsSelected { get; protected set; }

		public bool IsInitialized { get; set; }

		public Transform VisOrigin { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; set; }

		public Vector3 Offset { get; set; }

		public float Scale { get; set; }

		public VisualizationType Type { get; protected set; }
		
		public void OnDataPointChanged();

		public void OnVisObjectHovered(int index);

		public void OnVisObjectSelected(int index);

		public void OnVisObjectDeselected(int index);

		public void Despawn();

		public void OnTimeSeriesToggled(bool isActive);

		public void OnMeasurementResultsUpdated();

		public void OnMeasurementResultChanged(MeasurementResult mr);

		public void ApplyStyle(VisualizationStyle style);

		/// <summary>
		/// Gets called when a swipe 
		/// </summary>
		/// <param name="pointerInteraction"></param>
		public void Swiped(PointerInteraction pointerInteraction);

	}

}