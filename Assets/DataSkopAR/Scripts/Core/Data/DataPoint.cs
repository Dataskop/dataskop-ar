﻿using System;
using System.Linq;
using DataskopAR.Entities.Visualizations;
using UnityEngine;

namespace DataskopAR.Data {

	public class DataPoint : MonoBehaviour {

#region Event

		public event Action<MeasurementResult> MeasurementResultChanged;

#endregion

#region Fields

		private MeasurementResult mResult;

#endregion

#region Properties

		public MeasurementDefinition MeasurementDefinition { get; set; }

		public MeasurementResult CurrentMeasurementResult {
			get => mResult;
			set {
				mResult = value;
				MeasurementResultChanged?.Invoke(CurrentMeasurementResult);
			}
		}

		public int CurrentMeasurementResultIndex =>
			MeasurementDefinition.MeasurementResults.ToList().IndexOf(CurrentMeasurementResult);

		public DataAttribute Attribute { get; set; }
		public Visualization Vis { get; set; }
		public Device Device { get; set; }
		public AuthorRepository AuthorRepository { get; set; }

#endregion

#region Methods

		/// <summary>
		///     Sets and replaces the current visualization form with another.
		/// </summary>
		/// <param name="visPrefab">The visualization to be used for this data point.</param>
		public void SetVis(GameObject visPrefab) {
			RemoveVis();
			Vis = Instantiate(visPrefab, transform).GetComponent<Visualization>();
			Vis.Create(this);
			Vis.SwipedUp += NextMeasurementResult;
			Vis.SwipedDown += PreviousMeasurementResult;
		}

		public void RemoveVis() {

			if (Vis == null) return;

			Vis.SwipedUp -= NextMeasurementResult;
			Vis.SwipedDown -= PreviousMeasurementResult;
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
				Vis.Hover();
				return;
			}

			if (isSelected) {
				Vis.Select();
				return;
			}

			Vis.Deselect();

		}

		public void SetMeasurementResult(MeasurementResult mRes) {
			CurrentMeasurementResult = mRes;
		}

		public void NextMeasurementResult() {

			if (MeasurementDefinition.MeasurementResults != null) {
				int i = CurrentMeasurementResultIndex;

				if (i == 0)
					return;

				SetMeasurementResult(MeasurementDefinition.MeasurementResults.ToList()[i - 1]);
			}
		}

		public void PreviousMeasurementResult() {

			if (MeasurementDefinition.MeasurementResults != null) {
				int i = CurrentMeasurementResultIndex;

				if (i == MeasurementDefinition.MeasurementResults.Count - 1)
					return;

				SetMeasurementResult(MeasurementDefinition.MeasurementResults.ToList()[i + 1]);
			}

		}

#endregion

	}

}