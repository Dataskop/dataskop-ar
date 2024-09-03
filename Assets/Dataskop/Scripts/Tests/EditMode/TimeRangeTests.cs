using System;
using NUnit.Framework;

namespace Dataskop.EditMode.Tests {

	[TestFixture] [Category("Dataskop")]
	public class TimeRangeTests {

		private static readonly DateTime NOW = new DateTime(2024, 8, 12, 12, 0, 0);

		[Test]
		public void No_Missing_Time_Range_When_Search_Range_Is_Inside_Available_Time_Range() {

			// Arrange
			TimeRange[] available = {
				new TimeRange(new DateTime(2024, 8, 10, 12, 0, 0), NOW)
			};
			TimeRange searchRange = new TimeRange(new DateTime(2024, 8, 08, 12, 0, 0), NOW);

			// Act
			TimeRange[] foundGaps = TimeRangeExtensions.GetTimeRangeGaps(searchRange, available);

			// Assert
			Assert.That(foundGaps.Length, Is.EqualTo(0));

		}

		[Test]
		public void Found_Gap_Is_Between_Available_Ranges() {

			// Arrange
			TimeRange[] available = {
				new TimeRange(
					new DateTime(2024, 8, 10, 12, 0, 0),
					new DateTime(2024, 8, 12, 12, 0, 0)
				)
			};
			TimeRange searchRange = new TimeRange(
				new DateTime(2024, 8, 08, 12, 0, 0),
				new DateTime(2024, 8, 14, 12, 0, 0)
			);

			// Act
			TimeRange[] foundGaps = TimeRangeExtensions.GetTimeRangeGaps(searchRange, available);

			// Assert
			Assert.That(foundGaps.Length, Is.EqualTo(1));
			Assert.That(foundGaps[0].StartTime, Is.GreaterThanOrEqualTo(available[1].EndTime));
			Assert.That(foundGaps[0].EndTime, Is.LessThanOrEqualTo(available[0].StartTime));

		}

	}

}