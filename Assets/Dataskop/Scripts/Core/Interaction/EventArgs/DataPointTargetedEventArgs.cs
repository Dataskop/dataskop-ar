using System;
using Dataskop.Data;

namespace Dataskop.Interaction {

	public class DataPointTargetedEventArgs : EventArgs {

		public DataPointTargetedEventArgs(DataPoint d) {
			DataPoint = d;
		}

		public DataPoint DataPoint { get; }

	}

}