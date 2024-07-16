﻿using System;
using System.Linq;
using Dataskop.Data;
using Dataskop.Entities.Visualizations;
using Dataskop.Interaction;
using UnityEngine;

namespace Dataskop.Entities {

	public class DataPoint : MonoBehaviour {

		[Header("References")]
		[SerializeField] private SpriteRenderer mapIconBorder;
		[SerializeField] private SpriteRenderer visIcon;
		[SerializeField] private Sprite[] visIcons;

		[Header("Values")]
		[SerializeField] private Color mapSelectionColor;
		[SerializeField] private Color mapHoverColor;
		[SerializeField] private Color mapDefaultColor;

		public event Action<MeasurementResult> MeasurementResultChanged;

		public event Action<VisualizationType> VisualizationTypeChanged;

		public MeasurementDefinition MeasurementDefinition { get; set; }

		public int FocusedMeasurementIndex { get; set; }

		public MeasurementResult FocusedMeasurement => MeasurementDefinition.GetMeasurementResult(FocusedMeasurementIndex);

		public DataAttribute Attribute { get; set; }

		public Visualization Vis { get; private set; }

		public Device Device { get; set; }

		public AuthorRepository AuthorRepository { get; set; }

		private void Awake() {
			VisualizationTypeChanged += OnVisChanged;
		}

		/// <summary>
		///     Sets and replaces the current visualization form with another.
		/// </summary>
		/// <param name="visPrefab">The visualization to be used for this data point.</param>
		public void SetVis(GameObject visPrefab) {
			Vis = Instantiate(visPrefab, transform).GetComponent<Visualization>();
			Vis.DataPoint = this;
			Vis.SwipedUp += DecreaseMeasurementIndex;
			Vis.SwipedDown += IncreaseMeasurementIndex;
			VisualizationTypeChanged?.Invoke(Vis.Type);
		}

		public void RemoveVis() {

			if (Vis == null) return;

			Vis.SwipedUp -= DecreaseMeasurementIndex;
			Vis.SwipedDown -= IncreaseMeasurementIndex;
			Vis.OnTimeSeriesToggled(false);
			Vis.Despawn();

		}

		/// <summary>
		///     Sets the selection status of the data point.
		/// </summary>
		/// <param name="isSelected"></param>
		/// <param name="isHovered"></param>
		public void SetSelectionStatus(bool isSelected, bool isHovered) {

			if (isHovered) {
				//Vis.OnVisObjectHovered();
				SetMapIconColor(mapHoverColor);
				return;
			}

			if (isSelected) {
				//Vis.OnVisObjectSelected();
				SetMapIconColor(mapSelectionColor);
				return;
			}

			//Vis.OnVisObjectDeselected();
			SetMapIconColor(mapDefaultColor);

		}

		private void IncreaseMeasurementIndex() {

			if (MeasurementDefinition.MeasurementResults == null) {
				return;
			}

			if (FocusedMeasurementIndex == MeasurementDefinition.MeasurementResults.Count - 1) {
				return;
			}

			FocusedMeasurementIndex++;
			MeasurementResultChanged?.Invoke(MeasurementDefinition.GetMeasurementResult(FocusedMeasurementIndex));

		}

		private void DecreaseMeasurementIndex() {

			if (MeasurementDefinition.MeasurementResults == null) {
				return;
			}

			if (FocusedMeasurementIndex == 0) {
				return;
			}

			FocusedMeasurementIndex--;
			MeasurementResultChanged?.Invoke(MeasurementDefinition.GetMeasurementResult(FocusedMeasurementIndex));
		}

		private void SetMapIconColor(Color color) {
			mapIconBorder.color = color;
		}

		private void OnVisChanged(VisualizationType visType) {

			visIcon.sprite = visType switch {
				VisualizationType.Dot => visIcons[0],
				VisualizationType.Bubble => visIcons[1],
				VisualizationType.Bar => visIcons[2],
				_ => throw new ArgumentOutOfRangeException(nameof(visType), visType, null)
			};

		}

	}

}