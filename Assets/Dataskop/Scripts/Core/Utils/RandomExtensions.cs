using System;

namespace Dataskop.Utils {

	public static class RandomExtensions {

		/// <summary>
		///     Returns a random double between two doubles.
		/// </summary>
		/// <param name="random">Object of Type random</param>
		/// <param name="min">double value</param>
		/// <param name="max">double value</param>
		/// <returns></returns>
		public static double NextDoubleInRange(Random random, double min, double max) {
			return random.NextDouble() * (max - min) + min;
		}

	}

}