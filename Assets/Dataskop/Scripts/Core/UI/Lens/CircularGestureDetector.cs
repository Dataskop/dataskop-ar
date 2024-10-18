using System.Collections.Generic;
using UnityEngine;

namespace Dataskop.Dataskop.Scripts.Core.UI.Lens
{
    public class NewBehaviourScript : MonoBehaviour {
	    public RectTransform circleTransform;	// Reference to the circle image
		public int minPointsForGesture = 10;	// The minimum number of touch points for a gesture
		public float minRadius = 50f;			// The minimum radius for a circular gesture
		public float maxRadiusDeviation = 20f;	// The maximum deviation from the circular gesture to still be detected as circle
		
		private readonly List<Vector2> touchPositions = new();
		

        // Update is called once per frame
        private void Update() {
	        // If no touch input was performed we dont need to do anything
	        if (Input.touchCount <= 0)
		        return;
	        
	        var touch = Input.GetTouch(0).position;
	        
	        // Convert touch position to local pos
	        Vector2 localPoint;
	        RectTransformUtility.ScreenPointToLocalPointInRectangle(circleTransform, touch, null, out localPoint);
	        
	        // Check if we are in the circle
	        if (IsTouchInsideCircle(localPoint)) {
		        touchPositions.Add(touch);

		        // Ensure there are enough points for a gesture
		        if (touchPositions.Count < minPointsForGesture)
			        return;
		        
		        // Fit a circle to the touch points
		        var circleFitResult = FitCircle(touchPositions);
		        
		        // Check if the circle meets the criteria
		        if (circleFitResult.Radius > minRadius && IsCircleGesture(circleFitResult)) {
			        Debug.Log("Circular gesture was performed!");
			        touchPositions.Clear(); // Reset positions after detection
		        }
	        }
        }

        bool IsTouchInsideCircle(Vector2 localPoint) {
	        // Grab width and height of circle
	        float width = circleTransform.rect.width;
	        float height = circleTransform.rect.height;
	        
	        // Calc the radius
	        float radius = width * 0.5f;
	        
	        // Calc distance from center
	        Vector2 center = Vector2.zero;		// Locally the center should be (0,0)
	        float distanceFromCenter = Vector2.Distance(center, localPoint);
	        
	        return distanceFromCenter <= radius;
        }

        private static CircleFitResult FitCircle(List<Vector2> points) {
	        // Calculate centroid or average position
	        var centroid = Vector2.zero;
	        foreach (var point in points) {
		        centroid += point;
	        }
	        
	        centroid /= points.Count;
	        
	        // Estimate circle parameters like center and radius
	        float radiusSum = 0f;
	        foreach (var point in points) {
		        radiusSum += Vector2.Distance(point, centroid);
	        }
	        float estimatedRadius = radiusSum / points.Count;
	        
	        return new CircleFitResult(centroid, estimatedRadius);
        }

        bool IsCircleGesture(CircleFitResult circleFit) {
	        // Check if the points roughly form a circular shape
	        foreach (var point in touchPositions) {
		        float distance = Vector2.Distance(point, circleFit.Center);
		        if (Mathf.Abs(distance - circleFit.Radius) > maxRadiusDeviation) {
			        return false;
		        }
	        }
	        return true;
        }
    }

    public struct CircleFitResult {
	    public Vector2 Center;
	    public float Radius;

	    public CircleFitResult(Vector2 center, float radius) {
		    this.Center = center;
		    this.Radius = radius;
	    }
    }
}
