using System;
using Dataskop.Data;

namespace Dataskop.Interaction {

	public class DataPointTargetedEventArgs : EventArgs {

		public DataPoint DataPoint { get; }

		public DataPointTargetedEventArgs(DataPoint d) {
			DataPoint = d;
		}

	}

}