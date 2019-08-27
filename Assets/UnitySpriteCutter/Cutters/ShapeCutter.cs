using UnityEngine;
using System.Collections.Generic;

namespace UnitySpriteCutter.Cutters {
	
	internal static class ShapeCutter {

		public class CutResult {
			public Vector2[] firstSidePoints;
			public Vector2[] secondSidePoints;
		}

		class InfiniteLine {
			
			public float a;
			public float b;

			public InfiniteLine( Vector2 segmentRepresentationStart, Vector2 segmentRepresentationEnd ) {
				Vector2 offset = segmentRepresentationEnd - segmentRepresentationStart;
				Vector2 sum = segmentRepresentationStart + segmentRepresentationEnd;
				if ( offset.y == 0 ) {
					a = 0;
					b = segmentRepresentationStart.y;
				} else {
					if ( offset.x == 0 ) {
						// It isn't a mathematical function - let's fake it!
						offset.x = 0.01f;
					}
					a = offset.y / offset.x;
					b = ( sum.y - ( a * sum.x ) ) / 2;
				}
			}
			
			public bool PointBelowLine( Vector2 point ) {
				return ( point.y < ( a * point.x + b )  );
			}
			
			public bool IntersectsWithSegment( Vector2 start, Vector2 end ) {
				bool firstPointUnder = PointBelowLine( start );
				bool secondPointUnder = PointBelowLine( end );
				return ( firstPointUnder != secondPointUnder );
			}

			public Vector2 IntersectionWithOtherLine( InfiniteLine other ) {
				Vector2 result = new Vector2();
				result.x = ( other.b - b ) / ( a - other.a );
				result.y = a * result.x + b;
				return result;
			}
		}

		public static CutResult CutShapeIntoTwo( Vector2 lineStart, Vector2 lineEnd, Vector2[] shape ) {
			List<Vector2> firstSide = new List<Vector2>();
			List<Vector2> secondSide = new List<Vector2>();

			InfiniteLine cuttingLine = new InfiniteLine( lineStart, lineEnd );

			int intersectionsFound = 0;

			for ( int i = 0; i < shape.Length; i++ ) {
				Vector2 point = shape[ i ];

				Vector2 previousPoint;
				if ( i == 0 ) {
					previousPoint = shape[ shape.Length - 1 ];
				} else {
					previousPoint = shape[ i - 1 ];
				}

				if ( cuttingLine.IntersectsWithSegment( previousPoint, point ) ) {
					InfiniteLine lastTwoPointsLine = new InfiniteLine( previousPoint, point );
					Vector2 intersectionPoint = cuttingLine.IntersectionWithOtherLine( lastTwoPointsLine );
					firstSide.Add( intersectionPoint );
					secondSide.Add( intersectionPoint );
					intersectionsFound++;
				}

				if ( cuttingLine.PointBelowLine( point ) ) {
					firstSide.Add( point );
				} else {
					secondSide.Add( point );
				}
			}

			if ( intersectionsFound > 2 ) {
				throw new System.Exception( "SpriteCutter cannot cut through non-convex shapes! Adjust your colliders shapes to be convex!" );
			}
			
			CutResult result = new CutResult();
			result.firstSidePoints = firstSide.ToArray();
			result.secondSidePoints = secondSide.ToArray();
			return result;
		}
		
	}
	
}