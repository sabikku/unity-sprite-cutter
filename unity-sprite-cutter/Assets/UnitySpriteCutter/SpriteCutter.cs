using UnityEngine;
using System.Collections.Generic;
using UnitySpriteCutter.Cutters;
using UnitySpriteCutter.Tools;

namespace UnitySpriteCutter {

	public class SpriteCutterInput {

		public Vector2 lineStart;
		public Vector2 lineEnd;

		/// <summary>
		/// GameObject to cut. Has to have SpriteRenderer or MeshRenderer or cut wont take place.
		/// </summary>
		public GameObject gameObject;

		/// <summary>
		/// By default, objects are cut only if their colliders can be cut with given line.
		/// If set to true, colliders doesn't matter and aren't cut.
		/// </summary>
		public bool dontCutColliders;

		public enum GameObjectCreationMode {
			/// <summary>
			/// The original gameObject is converted into the "firstSide" gameObject.
			/// Use this, if you want to cut off a new gameObject from an existing one. Default value.
			/// </summary>
			CUT_OFF_ONE = 0,
			/// <summary>
			/// The original gameObject stays as it was before cutting.
			/// Instead, SpriteCutter creates two new gameObjects with first and second side of the cut.
			/// </summary>
			CUT_INTO_TWO = 1,
		}
		public GameObjectCreationMode gameObjectCreationMode;

	}

	public class SpriteCutterOutput {
		public GameObject firstSideGameObject;
		public GameObject secondSideGameObject;
	}

	public static class SpriteCutter {

		/// <summary>
		/// Returns null, if cutting didn't took place.
		/// </summary>
		public static SpriteCutterOutput Cut( SpriteCutterInput input ) {

			if ( input.gameObject == null ) {
				Debug.LogWarning( "SpriteCutter.Cut exceuted with null gameObject!" );
				return null;
			}
			
			Vector3 localLineStart = input.gameObject.transform.InverseTransformPoint( input.lineStart );
			Vector3 localLineEnd = input.gameObject.transform.InverseTransformPoint( input.lineEnd );

			SpriteRenderer spriteRenderer = input.gameObject.GetComponent<SpriteRenderer>();
			MeshRenderer meshRenderer = input.gameObject.GetComponent<MeshRenderer>();

			FlatConvexPolygonMeshCutter.CutResult meshCutResult =
				CutSpriteOrMeshRenderer( localLineStart, localLineEnd, spriteRenderer, meshRenderer );
			if ( meshCutResult.DidNotCut() ) {
				return null;
			}

			FlatConvexCollidersCutter.CutResult collidersCutResults;
			if ( input.dontCutColliders ) {
				collidersCutResults = new FlatConvexCollidersCutter.CutResult();
			} else {
				collidersCutResults =
					CutColliders( localLineStart, localLineEnd, input.gameObject.GetComponents<Collider2D>() );

				if ( collidersCutResults.DidNotCut() ) {
					return null;
				}
			}

			SpriteCutterGameObject secondSideResult = SpriteCutterGameObject.CreateAsCopyOf( input.gameObject, true );
			PrepareResultGameObject( secondSideResult, spriteRenderer, meshRenderer,
			                         meshCutResult.secondSideMesh, collidersCutResults.secondSideColliderRepresentations );

			SpriteCutterGameObject firstSideResult = null;
			if ( input.gameObjectCreationMode == SpriteCutterInput.GameObjectCreationMode.CUT_INTO_TWO ) {
				firstSideResult = SpriteCutterGameObject.CreateAsCopyOf( input.gameObject, true );
				PrepareResultGameObject( firstSideResult, spriteRenderer, meshRenderer,
				                         meshCutResult.firstSideMesh, collidersCutResults.firstSideColliderRepresentations );

			} else if ( input.gameObjectCreationMode == SpriteCutterInput.GameObjectCreationMode.CUT_OFF_ONE ) {
				firstSideResult = SpriteCutterGameObject.CreateAs( input.gameObject );
				if ( spriteRenderer != null ) {
					RendererParametersRepresentation tempParameters = new RendererParametersRepresentation();
					tempParameters.CopyFrom( spriteRenderer );
					SafeSpriteRendererRemoverBehaviour.get.RemoveAndWaitOneFrame( spriteRenderer, onFinish: () => {
						PrepareResultGameObject( firstSideResult, tempParameters,
						                         meshCutResult.firstSideMesh, collidersCutResults.firstSideColliderRepresentations );
					} );

				} else {
					PrepareResultGameObject( firstSideResult, spriteRenderer, meshRenderer,
					                         meshCutResult.firstSideMesh, collidersCutResults.firstSideColliderRepresentations );
				}
			}

			return new SpriteCutterOutput() {
				firstSideGameObject = firstSideResult.gameObject,
				secondSideGameObject = secondSideResult.gameObject,
			};
		}

		static FlatConvexPolygonMeshCutter.CutResult CutSpriteOrMeshRenderer( Vector2 lineStart, Vector2 lineEnd, SpriteRenderer spriteRenderer, MeshRenderer meshRenderer ) {
			Mesh originMesh = GetOriginMeshFrom( spriteRenderer, meshRenderer );
			return FlatConvexPolygonMeshCutter.Cut( lineStart, lineEnd, originMesh );
		}

		static Mesh GetOriginMeshFrom( SpriteRenderer spriteRenderer, MeshRenderer meshRenderer ) {
			if ( spriteRenderer != null ) {
				return SpriteMeshConstructor.ConstructFromRendererBounds( spriteRenderer );
			} else {
				return meshRenderer.GetComponent<MeshFilter>().mesh;
			}
		}

		static FlatConvexCollidersCutter.CutResult CutColliders( Vector2 lineStart, Vector2 lineEnd, Collider2D[] colliders ) {
			return FlatConvexCollidersCutter.Cut( lineStart, lineEnd, colliders );
		}
		
		static void PrepareResultGameObject( SpriteCutterGameObject resultGameObject, SpriteRenderer spriteRenderer,
		                                            MeshRenderer meshRenderer, Mesh mesh, List<PolygonColliderParametersRepresentation> colliderRepresentations ) {
			resultGameObject.AssignMeshFilter( mesh );
			if ( spriteRenderer != null ) {
				resultGameObject.AssignMeshRendererFrom( spriteRenderer );
			} else {
				resultGameObject.AssignMeshRendererFrom( meshRenderer );
			}

			if ( colliderRepresentations != null ) {
				resultGameObject.BuildCollidersFrom( colliderRepresentations );
			}
		}
		
		static void PrepareResultGameObject( SpriteCutterGameObject resultGameObject, RendererParametersRepresentation tempParameters,
		                                            Mesh mesh, List<PolygonColliderParametersRepresentation> colliderRepresentations ) {
			resultGameObject.AssignMeshFilter( mesh );
			resultGameObject.AssignMeshRendererFrom( tempParameters );
			
			if ( colliderRepresentations != null ) {
				resultGameObject.BuildCollidersFrom( colliderRepresentations );
			}
		}

	}

}