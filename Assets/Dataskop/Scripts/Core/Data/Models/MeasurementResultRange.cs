using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dataskop.Data;

namespace Dataskop {

	public class MeasurementResultRange : IList<MeasurementResult> {

		private readonly IList<MeasurementResult> list = new List<MeasurementResult>();

		private DateTime StartTime { get; set; }

		private DateTime EndTime { get; set; }

		public MeasurementResultRange(IEnumerable<MeasurementResult> newList) {
			list = newList.ToList();
		}

		#region IList<MeasurementResult> Members

		public MeasurementResult this[int index]
		{
			get => list[index];
			set => list[index] = value;
		}

		public int Count => list.Count;

		public bool IsReadOnly => list.IsReadOnly;

		public IEnumerator<MeasurementResult> GetEnumerator() {
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Add(MeasurementResult item) {
			list.Add(item);
		}

		public void Clear() {
			list.Clear();
		}

		public bool Contains(MeasurementResult item) {
			return list.Contains(item);
		}

		public void CopyTo(MeasurementResult[] array, int arrayIndex) {
			list.CopyTo(array, arrayIndex);
		}

		public bool Remove(MeasurementResult item) {
			return list.Remove(item);
		}

		public int IndexOf(MeasurementResult item) {
			return list.IndexOf(item);
		}

		public void Insert(int index, MeasurementResult item) {
			list.Insert(index, item);
		}

		public void RemoveAt(int index) {
			list.RemoveAt(index);
		}

		#endregion

		public TimeRange GetTimeRange() {
			return new TimeRange(StartTime, EndTime);
		}

		public void SetTimeRange(TimeRange newRange) {
			StartTime = newRange.StartTime;
			EndTime = newRange.EndTime;
		}

	}

}