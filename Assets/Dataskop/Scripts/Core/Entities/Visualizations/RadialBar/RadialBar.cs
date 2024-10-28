using System;
using System.Collections;
using System.Collections.Generic;
using Dataskop.Data;
using Dataskop.Entities;
using Dataskop.Entities.Visualizations;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop {

	public class RadialBar : MonoBehaviour, IVisualization {

		[Header("References")]
		[SerializeField] private GameObject visObjectPrefab;
		[SerializeField] private Transform visObjectsContainer;

		[Header("Vis Values")]
		[SerializeField] private Vector3 offset;
		[SerializeField] private float scaleFactor;

		private float Scale { get; set; }

		public IVisObject[] VisObjects { get; set; }

		public IVisObject FocusedVisObject { get; }

		public DataPoint DataPoint { get; }

		public VisualizationOption VisOption { get; set; }

		public VisHistoryConfiguration VisHistoryConfiguration { get; set; }

		public bool IsSelected { get; }

		public bool HasHistoryEnabled { get; }

		public Transform VisOrigin { get; set; }

		public MeasurementType[] AllowedMeasurementTypes { get; set; }

		public Vector3 Offset { get; }

		public VisualizationType Type { get; set; }

		public event Action SwipedDown;

		public event Action SwipedUp;

		public event Action<int> VisObjectHovered;

		public event Action<int> VisObjectSelected;

		public event Action<int> VisObjectDeselected;

		public event Action<IVisObject> FocusedVisObjectChanged;

		public void Initialize(DataPoint dp) {
			throw new NotImplementedException();
		}

		public void OnTimeSeriesToggled(bool isActive) {
			throw new NotImplementedException();
		}

		public void OnFocusedIndexChanged(int index) {
			throw new NotImplementedException();
		}

		public void OnSwipeInteraction(PointerInteraction pointerInteraction) {
			throw new NotImplementedException();
		}

		public void OnMeasurementResultRangeUpdated() {
			throw new NotImplementedException();
		}

		public void OnMeasurementResultsUpdated(int newIndex) {
			throw new NotImplementedException();
		}

		public void ApplyStyle(VisualizationStyle style) {
			throw new NotImplementedException();
		}

		public void Despawn() {
			throw new NotImplementedException();
		}

	}

}