using Dataskop.Entities.Visualizations;
using NUnit.Framework;

namespace Dataskop.EditMode.Tests {

	[TestFixture] [Category("Dataskop")]
	public class BubbleUtilsTests {

		[Test]
		public void Radius_Is_At_Minimum_When_Value_Is_Less_Than_Lower_Input_Limit() {

			// Arrange
			float inputValue = 0;
			float minValue = 1;
			float maxValue = 2;
			float minSize = 5;
			float maxSize = 10;

			// Act
			float radius = BubbleUtils.CalculateRadius(inputValue, minValue, maxValue, minSize, maxSize);

			// Assert
			Assert.That(radius, Is.EqualTo(minSize));

		}

		[Test]
		public void Radius_Is_At_Maximum_When_Value_Is_More_Than_Upper_Input_Limit() {

			// Arrange
			float inputValue = 3;
			float minValue = 1;
			float maxValue = 2;
			float minSize = 5;
			float maxSize = 10;

			// Act
			float radius = BubbleUtils.CalculateRadius(inputValue, minValue, maxValue, minSize, maxSize);

			// Assert
			Assert.That(radius, Is.EqualTo(maxSize));

		}

		/*
		[Test]
		[TestCase(2.5f, 5)]
		public void Radius_Does_Not_Scale_Linearly(float value1, float expectedRadius) {

			// Arrange
			BubbleUtils sut = new();

			float minValue = 0;
			float maxValue = 5;
			float minSize = 0;
			float maxSize = 10;

			// Act
			float radius1 = sut.CalculateRadius(value1, minValue, maxValue, minSize, maxSize);

			// Assert
			Assert.That(radius1, !Is.EqualTo(expectedRadius));

		}
		*/

	}

}