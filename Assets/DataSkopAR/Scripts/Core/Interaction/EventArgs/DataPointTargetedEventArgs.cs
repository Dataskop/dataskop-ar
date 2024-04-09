using System;
using DataskopAR.Data;
namespace DataskopAR.Interaction {

	public class DataPointTargetedEventArgs : EventArgs {

		public DataPointTargetedEventArgs(DataPoint d) {
			DataPoint = d;
		}

		public DataPoint DataPoint { get; }

	}

}