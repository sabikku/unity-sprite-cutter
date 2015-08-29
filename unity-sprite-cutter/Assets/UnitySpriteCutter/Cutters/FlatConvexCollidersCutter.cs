using UnityEngine;
using System.Collections.Generic;
using UnitySpriteCutter.Tools;

namespace UnitySpriteCutter.Cutters {
	
	internal static class FlatConvexCollidersCutter {

		public class CutResult {
			public List<PolygonColliderParametersRepresentation> firstSideColliderRepresentations;
			public List<PolygonColliderParametersRepresentation> secondSideColliderRepresentations;

			public bool DidNotCut() {
				return ( firstSideColliderRepresentations.Count == 0 || secondSideColliderRepresentations.Count == 0 );
			}
		}

		public static CutResult Cut( Vector2 lineStart, Vector2 lineEnd, Collider2D[] colliders ) {
			CutResult result = new CutResult();
			result.firstSideColliderRepresentations = new List<PolygonColliderParametersRepresentation>();
			result.secondSideColliderRepresentations = new List<PolygonColliderParametersRepresentation>();

			foreach ( Collider2D collider in colliders ) {

				List<Vector2[]> paths = ColliderPathsCreator.GetPolygonColliderPathsFrom( collider );
				foreach ( Vector2[] path in paths ) {
					ShapeCutter.CutResult cutResult = ShapeCutter.CutShapeIntoTwo( lineStart, lineEnd, path );

					if ( cutResult.firstSidePoints.Length > 0 ) {
						PolygonColliderParametersRepresentation repr = new PolygonColliderParametersRepresentation();
						repr.CopyParametersFrom( collider );
						repr.paths.Add( cutResult.firstSidePoints );
						result.firstSideColliderRepresentations.Add( repr );
					}
					if ( cutResult.secondSidePoints.Length > 0 ) {
						PolygonColliderParametersRepresentation repr = new PolygonColliderParametersRepresentation();
						repr.CopyParametersFrom( collider );
						repr.paths.Add( cutResult.secondSidePoints );
						result.secondSideColliderRepresentations.Add( repr );
					}

				}
			}

			return result;
		}

	}

}