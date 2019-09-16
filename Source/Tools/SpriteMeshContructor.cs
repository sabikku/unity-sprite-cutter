using UnityEngine;
using System.Collections.Generic;

namespace UnitySpriteCutter.Tools {
	
	public static class SpriteMeshConstructor {

		// For only one texture
		public static Mesh ConstructFromRendererBounds( SpriteRenderer renderer ) {

			if ( renderer.sprite == null ) {
				throw new System.Exception( "Cannot cut from null sprite!" );
			}

			Mesh result = new Mesh();
			
			Vector2 min = renderer.sprite.bounds.min;
			Vector2 max = renderer.sprite.bounds.max;

			Vector3[] vertices = new Vector3[ 4 ];
			vertices[ 0 ] = new Vector2( min.x, max.y );
			vertices[ 1 ] = max;
			vertices[ 2 ] = new Vector2( max.x, min.y );
			vertices[ 3 ] = min;

			int[] triangles = new int[ 6 ];
			triangles[ 0 ] = 0;
			triangles[ 1 ] = 1;
			triangles[ 2 ] = 2;
			triangles[ 3 ] = 0;
			triangles[ 4 ] = 2;
			triangles[ 5 ] = 3;

			Vector2[] uv = new Vector2[ 4 ];
			uv[ 0 ] = new Vector2( 0, 1 );
			uv[ 1 ] = new Vector2( 1, 1 );
			uv[ 2 ] = new Vector2( 1, 0 );
			uv[ 3 ] = new Vector2( 0, 0 );

			result.vertices = vertices;
			result.triangles = triangles;
			result.uv = uv;
			result.Optimize();
			result.RecalculateNormals();

			return result;
		}

		// For any texture 
		public static Mesh ConstructFromRendererBounds_General( SpriteRenderer renderer ) {

			if ( renderer.sprite == null ) {
				throw new System.Exception( "Cannot cut from null sprite!" );
			}

			Mesh result = new Mesh();
			
			Vector2 min = renderer.sprite.bounds.min;
			Vector2 max = renderer.sprite.bounds.max;

			Vector3[] vertices = new Vector3[ 4 ];
			vertices[ 0 ] = new Vector2( min.x, max.y );
			vertices[ 1 ] = max;
			vertices[ 2 ] = new Vector2( max.x, min.y );
			vertices[ 3 ] = min;

			int[] triangles = new int[ 6 ];
			triangles[ 0 ] = 0;
			triangles[ 1 ] = 1;
			triangles[ 2 ] = 2;
			triangles[ 3 ] = 0;
			triangles[ 4 ] = 2;
			triangles[ 5 ] = 3;

			result.vertices = vertices;
			result.triangles = triangles;
			result.uv = convertUVs(renderer.sprite);
			result.Optimize();
			result.RecalculateNormals();

			return result;
		}

		private static Vector2[] convertUVs(Sprite sprite) {
			Vector2[] uv = new Vector2[ 4 ];

			Rect textureRect = sprite.textureRect;

			float minX = textureRect.min.x / sprite.texture.width;
			float maxX = textureRect.max.x / sprite.texture.width;
			float minY = textureRect.min.y / sprite.texture.height;
			float maxY = textureRect.max.y / sprite.texture.height;

			uv[ 0 ] = new Vector2( minX, maxY );
			uv[ 1 ] = new Vector2( maxX, maxY );
			uv[ 2 ] = new Vector2( maxX, minY );
			uv[ 3 ] = new Vector2( minX, minY );

			return uv;
		}

	}

}