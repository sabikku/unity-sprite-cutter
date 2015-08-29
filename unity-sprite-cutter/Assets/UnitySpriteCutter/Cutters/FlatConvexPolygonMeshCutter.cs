using UnityEngine;
using System.Collections.Generic;

namespace UnitySpriteCutter.Cutters {
	
	internal static class FlatConvexPolygonMeshCutter {

		public class CutResult {
			public Mesh firstSideMesh;
			public Mesh secondSideMesh;

			public bool DidNotCut() {
				return firstSideMesh == null || secondSideMesh == null;
			}
		}

		public static CutResult Cut( Vector2 lineStart, Vector2 lineEnd, Mesh mesh ) {
			CutResult result = new CutResult();

			Vector2[] shape = ConvertVerticesToShape( mesh.vertices );
			ShapeCutter.CutResult shapeCutResult = ShapeCutter.CutShapeIntoTwo( lineStart, lineEnd, shape );
			if ( shapeCutResult.firstSidePoints.Length < 3 ||
			     shapeCutResult.secondSidePoints.Length < 3 ) {
				return result;
			}

			result.firstSideMesh  = GenerateHalfMeshFrom( mesh, shapeCutResult.firstSidePoints );
			result.secondSideMesh = GenerateHalfMeshFrom( mesh, shapeCutResult.secondSidePoints );

			return result;
		}

		static Vector2[] ConvertVerticesToShape( Vector3[] vertices ) {
			Vector2[] shape = new Vector2[ vertices.Length ];
			float z = vertices[ 0 ].z;
			for ( int i = 0; i < vertices.Length; i++ ) {
				if ( vertices[ i ].z != z ) {
					throw new System.Exception( "Given mesh isn't flat! " + z + " vs " + vertices[ i ].z );
				}
				shape[ i ] = vertices[ i ];
			}
			return shape;
		}

		static Mesh GenerateHalfMeshFrom( Mesh original, Vector2[] flatVertices ) {
			Vector3[] newVertices = new Vector3[ flatVertices.Length ];
			for ( int i = 0; i < flatVertices.Length; i++ ) {
				newVertices[ i ] = (Vector3)flatVertices[ i ];
			}

			Mesh result = new Mesh();

			if ( newVertices.Length < 3 ) {
				throw new System.Exception( "Cannot generate mesh from less than 3 vertices!" );
			}

			result.vertices = newVertices;
			result.triangles = GenerateConvexPolygonTrianglesFromVertices( newVertices );
			result.uv = GenerateProportionalUVs( newVertices, original );

			result.Optimize();
			result.RecalculateNormals();

			return result;
		}

		static int[] GenerateConvexPolygonTrianglesFromVertices( Vector3[] vertices ) {
			if ( vertices.Length == 3 ) {
				return new int[] { 0, 1, 2 };
			}

			List<int> result = new List<int>();
			for ( int i = 2; i < vertices.Length; i++ ) {
				result.Add( 0 );
				result.Add( i - 1 );
				result.Add( i );
			}

			return result.ToArray();
		}

		static Vector2[] GenerateProportionalUVs( Vector3[] vertices, Mesh original ) {
			Vector2[] result = new Vector2[ vertices.Length ];

			int vertexIndexToCalculateDiff = 0;
			for ( int i = 1; i < original.vertexCount; i++ ) {
				if ( original.vertices[ 0 ].x != original.vertices[ i ].x &&
				     original.vertices[ 0 ].y != original.vertices[ i ].y ) {
					vertexIndexToCalculateDiff = i;
					break;
				}
			}
			if ( vertexIndexToCalculateDiff == 0 ) {
				throw new System.Exception( "Couldn't find vertexes with different x and y coordinates!" );
			}

			Vector3 twoFirstVerticesDiff = original.vertices[ vertexIndexToCalculateDiff ] - original.vertices[ 0 ];
			Vector2 twoFirstUVsDiff = original.uv[ vertexIndexToCalculateDiff ] - original.uv[ 0 ];
			Vector2 distanceToUVMap = new Vector2();
			distanceToUVMap.x = twoFirstUVsDiff.x / twoFirstVerticesDiff.x;
			distanceToUVMap.y = twoFirstUVsDiff.y / twoFirstVerticesDiff.y;

			for ( int i = 0; i < vertices.Length; i++ ) {
				result[ i ] = ( vertices[ i ] - original.vertices[ 0 ] );
				result[ i ] = new Vector2( result[ i ].x * distanceToUVMap.x,
				                           result[ i ].y * distanceToUVMap.y );
				result[ i ] += original.uv[ 0 ];
			}

			return result;
		}

	}

}