using UnityEngine;
using System.Collections.Generic;
using UnitySpriteCutter.Cutters;
using UnitySpriteCutter.Tools;

namespace UnitySpriteCutter {

	/// <summary>
	/// Holds gameObject destined to cut and performs all operations that modifies its parameters / components.
	/// </summary>
	internal class SpriteCutterGameObject {

		public GameObject gameObject {
			get;
			private set;
		}

		private SpriteCutterGameObject() {
		}

		public static SpriteCutterGameObject CreateAs( GameObject origin ) {
			SpriteCutterGameObject result = new SpriteCutterGameObject();
			result.gameObject = origin;
			return result;
		}

		public static SpriteCutterGameObject CreateAsCopyOf( GameObject origin, bool secondSide ) {
			SpriteCutterGameObject result = new SpriteCutterGameObject();
			result.gameObject = new GameObject( origin.name + ( !secondSide ? "_firstSide" : "_secondSide" ) );
			result.CopyGameObjectParametersFrom( origin );
			result.CopyTransformFrom( origin.transform );
			return result;
		}
		
		void CopyGameObjectParametersFrom( GameObject other ) {
			gameObject.isStatic = other.isStatic;
			gameObject.layer = other.layer;
			gameObject.tag = other.tag;
		}
		
		void CopyTransformFrom( Transform transform ) {
			gameObject.transform.position = transform.position;
			gameObject.transform.rotation = transform.rotation;
			gameObject.transform.localScale = transform.localScale;
		}
		
		public void AssignMeshFilter( Mesh mesh ) {
			MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
			if ( meshFilter == null ) {
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}
			meshFilter.mesh = mesh;
		}
		
		public void AssignMeshRendererFrom( SpriteRenderer spriteRenderer ) {
			RendererParametersRepresentation tempParameters = new RendererParametersRepresentation();
			tempParameters.CopyFrom( spriteRenderer );
			AssignMeshRendererFrom( tempParameters );
		}
		
		public void AssignMeshRendererFrom( MeshRenderer meshRenderer ) {
			RendererParametersRepresentation tempParameters = new RendererParametersRepresentation();
			tempParameters.CopyFrom( meshRenderer );
			AssignMeshRendererFrom( tempParameters );
		}

		public void AssignMeshRendererFrom( RendererParametersRepresentation tempParameters ) {
			MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if ( meshRenderer == null ) {
				meshRenderer = gameObject.AddComponent<MeshRenderer>();
			}
			tempParameters.PasteTo( meshRenderer );
		}

		public void BuildCollidersFrom( List<PolygonColliderParametersRepresentation> representations ) {
			foreach ( Collider2D collider in gameObject.GetComponents<Collider2D>() ) {
				Collider2D.Destroy( collider );
			}
			foreach ( PolygonColliderParametersRepresentation representation in representations ) {
				PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
				representation.PasteTo( collider );
			}
		}
	}

}