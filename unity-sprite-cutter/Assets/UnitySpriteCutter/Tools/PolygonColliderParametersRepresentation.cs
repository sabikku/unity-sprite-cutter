using UnityEngine;
using System.Collections.Generic;

namespace UnitySpriteCutter.Tools {
	internal class PolygonColliderParametersRepresentation {
		
		public List<Vector2[]> paths = new List<Vector2[]>();
		Vector2 offset;
		bool isTrigger;
		PhysicsMaterial2D sharedMaterial;
		bool usedByEffector;
		bool enabled;
		
		public void CopyParametersFrom( Collider2D origin ) {
			isTrigger = origin.isTrigger;
			offset = origin.offset;
			sharedMaterial = origin.sharedMaterial;
			usedByEffector = origin.usedByEffector;
			enabled = origin.enabled;
		}
		
		public void PasteTo( PolygonCollider2D polygonCollider ) {
			polygonCollider.pathCount = paths.Count;
			for ( int i = 0; i < paths.Count; i++ ) {
				polygonCollider.SetPath( i, paths[ i ] );
			}
			
			polygonCollider.isTrigger = isTrigger;
			polygonCollider.offset = offset;
			polygonCollider.sharedMaterial = sharedMaterial;
			polygonCollider.usedByEffector = usedByEffector;
			polygonCollider.enabled = enabled;
		}
	}
}
