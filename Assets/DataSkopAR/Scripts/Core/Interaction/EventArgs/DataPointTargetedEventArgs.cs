using System;
using DataSkopAR.Data;

namespace DataSkopAR.Interaction {

	public class DataPointTargetedEventArgs : EventArgs {

		public DataPointTargetedEventArgs(DataPoint d) {
			DataPoint = d;
		}

		public DataPoint DataPoint { get; }

	}

}