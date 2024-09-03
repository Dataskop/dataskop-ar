using System;
using NUnit.Framework;

namespace Dataskop.EditMode.Tests {

	[TestFixture] [Category("Dataskop")]
	public class TimeRangeTests {

		[Test]
		public void No_Missing_Time_Range_When_Search_Range_Is_Inside_Available_Time_Range() {

			// Arrange
			TimeRange[] available = {
				new(DateTime.Now.Subtract(new TimeSpan(3, 0, 0)), DateTime.Now)
			};
			TimeRange searchRange = new(DateTime.Now.Subtract(new TimeSpan(2, 0, 0)), DateTime.Now);

			// Act
			TimeRange[] foundGaps = TimeRangeExtensions.GetTimeRangeGaps(searchRange, available);

			// Assert
			Assert.That(foundGaps.Length, Is.EqualTo(0));

		}

	}

}