using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Dataskop.EditMode.Tests {

	[TestFixture] [Category("Dataskop")]
	public class TimeRangeTests {

		private static readonly DateTime NOW = new DateTime(2024, 8, 12);

		[Test]
		public void No_Missing_Time_Range_When_Search_Range_Is_Inside_Available_Time_Range() {

			// Arrange
			TimeRange[] available = {
				new TimeRange(new DateTime(2024, 8, 10), NOW)
			};
			TimeRange searchRange = new TimeRange(new DateTime(2024, 8, 11), NOW);

			// Act
			TimeRange[] foundGaps = TimeRangeExtensions.GetTimeRangeGaps(searchRange, available);

			// Assert
			Assert.That(foundGaps.Length, Is.EqualTo(0));

		}

		[Test]
		public void End_Time_Of_Gap_Is_Smaller_Or_Equal_Than_Start_Of_Overlapping_Range() {

			// Arrange
			TimeRange[] available = {
				new TimeRange(
					new DateTime(2024, 8, 10),
					new DateTime(2024, 8, 12)
				)
			};
			TimeRange searchRange = new TimeRange(
				new DateTime(2024, 8, 09),
				new DateTime(2024, 8, 11)
			);

			// Act
			TimeRange[] foundGaps = TimeRangeExtensions.GetTimeRangeGaps(searchRange, available);

			// Assert
			Assert.That(foundGaps.Length, Is.EqualTo(1));
			Assert.That(foundGaps[0].EndTime, Is.LessThanOrEqualTo(available[0].StartTime));

		}

		[Test]
		public void Found_Gap_Is_Between_Available_Ranges() {

			// Arrange
			TimeRange[] available = {
				new TimeRange(
					new DateTime(2024, 8, 10),
					new DateTime(2024, 8, 12)
				),
				new TimeRange(
					new DateTime(2024, 8, 03),
					new DateTime(2024, 8, 05)
				)
			};

			TimeRange searchRange = new TimeRange(
				new DateTime(2024, 8, 06),
				new DateTime(2024, 8, 08)
			);

			// Act
			TimeRange[] foundGaps = TimeRangeExtensions.GetTimeRangeGaps(searchRange, available);

			// Assert
			Assert.That(foundGaps.Length, Is.EqualTo(1));
			Assert.That(foundGaps[0].StartTime, Is.GreaterThanOrEqualTo(available[1].EndTime));
			Assert.That(foundGaps[0].EndTime, Is.LessThanOrEqualTo(available[0].StartTime));

		}

		[Test]
		public void Found_Multiple_Gaps_When_There_Is_A_Search_Across_Multiple_Ranges() {

			// Arrange
			TimeRange[] available = {
				new TimeRange(
					new DateTime(2024, 8, 23),
					new DateTime(2024, 8, 25)
				),
				new TimeRange(
					new DateTime(2024, 8, 18),
					new DateTime(2024, 8, 20)
				),
				new TimeRange(
					new DateTime(2024, 8, 09),
					new DateTime(2024, 8, 15)
				),
				new TimeRange(
					new DateTime(2024, 8, 04),
					new DateTime(2024, 8, 05)
				)
			};

			TimeRange searchRange = new TimeRange(
				new DateTime(2024, 8, 01),
				new DateTime(2024, 8, 24)
			);

			// Act
			TimeRange[] foundGaps = TimeRangeExtensions.GetTimeRangeGaps(searchRange, available);

			// Assert
			Assert.That(foundGaps.Length, Is.EqualTo(4));

		}

		[Test]
		public void No_Gap_Is_Overlapping_With_Existing_Time_Ranges() {

			// Arrange
			TimeRange[] available = {
				new TimeRange(
					new DateTime(2024, 8, 23),
					new DateTime(2024, 8, 25)
				),
				new TimeRange(
					new DateTime(2024, 8, 18),
					new DateTime(2024, 8, 20)
				),
				new TimeRange(
					new DateTime(2024, 8, 09),
					new DateTime(2024, 8, 15)
				)
			};

			TimeRange searchRange = new TimeRange(
				new DateTime(2024, 8, 01),
				new DateTime(2024, 8, 24)
			);

			// Act
			TimeRange[] foundGaps = TimeRangeExtensions.GetTimeRangeGaps(searchRange, available);
			bool hasOverlap = foundGaps.Any(gap => available.Any(avail =>
				(gap.StartTime < avail.EndTime && gap.EndTime > avail.StartTime)));

			// Assert
			Assert.That(hasOverlap, Is.False);

		}

	}

}