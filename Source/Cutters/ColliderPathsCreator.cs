using UnityEngine;
using System.Collections.Generic;

namespace UnitySpriteCutter.Cutters {

	/// <summary>
	/// It converts simple colliders - BoxCollider2D and CircleCollider2D - to paths,
	/// that can be later cut through and applied to PolygonColliders.
	/// </summary>
	public static class ColliderPathsCreator {

		/// <summary>
		/// The circle collider sides. Use less for more efficency.
		/// </summary>
		public static uint circleColliderSides = 64;

		public static List<Vector2[]> GetPolygonColliderPathsFrom( Collider2D collider ) {

			PolygonCollider2D polygonCollider = collider as PolygonCollider2D;

			if ( polygonCollider != null ) {
				List<Vector2[]> result = new List<Vector2[]>();
				for ( int i = 0; i < polygonCollider.pathCount; i++ ) {
					result.Add( polygonCollider.GetPath( i ) );
				}
				return result;
			}

			if ( collider is EdgeCollider2D ) {
				return null;
			}

			if ( collider is CircleCollider2D ) {
				return CreatePolygonColliderPathsFromCircle( collider as CircleCollider2D );

			} else if ( collider is BoxCollider2D ) {
				return CreatePolygonColliderPathsFromBox( collider as BoxCollider2D );

			} else {
				throw new System.Exception( "Unrecognized Collider2D in gameObject " + collider.gameObject.name );
			}

		}
		
		static List<Vector2[]> CreatePolygonColliderPathsFromCircle( CircleCollider2D circleCollider ) {

			int sides = Mathf.Max( 3, (int)circleColliderSides );
			Vector2[] path = new Vector2[ sides ];
			float angle = 0;
			float delta = ( 2 * Mathf.PI ) / sides;
			
			for ( int i = 0; i < sides; i++ ) {
				path[ i ] = new Vector2(
					Mathf.Cos( angle ) * circleCollider.radius,
					Mathf.Sin( angle ) * circleCollider.radius
				);
				angle += delta;
			}
			
			List<Vector2[]> result = new List<Vector2[]>();
			result.Add( path );
			return result;

		}
		
		static List<Vector2[]> CreatePolygonColliderPathsFromBox( BoxCollider2D boxCollider ) {

			Vector2[] path = new Vector2[ 4 ];
			Vector2 halfSize = boxCollider.size / 2;

			path[ 0 ] = new Vector2( -halfSize.x,
			                         +halfSize.y );
			path[ 1 ] = new Vector2( +halfSize.x,
			                         +halfSize.y );
			path[ 2 ] = new Vector2( +halfSize.x,
			                         -halfSize.y );
			path[ 3 ] = new Vector2( -halfSize.x,
			                         -halfSize.y );
			
			List<Vector2[]> result = new List<Vector2[]>();
			result.Add( path );
			return result;

		}
		
	}
	
}
