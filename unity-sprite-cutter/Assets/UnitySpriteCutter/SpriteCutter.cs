using UnityEngine;
using System.Collections.Generic;
using UnitySpriteCutter.Cutters;
using UnitySpriteCutter.Tools;
using GameObjectCreationMode = UnitySpriteCutter.SpriteCutterInput.GameObjectCreationMode;

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
			/// Use this, if you want to intantiate new "cutted" gameObject from an existing one. Default value.
			/// </summary>
			CUT_OFF_NEW = 0,
			/// <summary>
			/// Works as CUT_OFF_ONE, but instead of intantiating an entirelynew GameObject,
			/// it instantiates a "cutted" copy of the original gameObject.
			/// </summary>
			CUT_OFF_COPY = 1,
			/// <summary>
			/// The original gameObject stays as it was before cutting.
			/// Instead, SpriteCutter instantiates two new gameObjects with first and second side of the cut.
			/// </summary>
			CUT_INTO_TWO = 2,
		}
		public GameObjectCreationMode gameObjectCreationMode;

	}

	public class SpriteCutterOutput {
		public GameObject firstSideGameObject;
		public GameObject secondSideGameObject;
	}

	public static class SpriteCutter {

		struct SideCutData {
			public Mesh cuttedMesh;
			public List<PolygonColliderParametersRepresentation> cuttedCollidersRepresentations;
		}

		/// <summary>
		/// Returns null, if cutting didn't took place.
		/// </summary>
		public static SpriteCutterOutput Cut( SpriteCutterInput input ) {

			if ( input.gameObject == null ) {
				Debug.LogWarning( "SpriteCutter.Cut exceuted with null gameObject!" );
				return null;
			}

			/* Cutting  mesh and collider */

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
			
			SideCutData firstCutData = new SideCutData() {
				cuttedMesh = meshCutResult.firstSideMesh,
				cuttedCollidersRepresentations = collidersCutResults.firstSideColliderRepresentations
			};
			SideCutData secondCutData = new SideCutData() {
				cuttedMesh = meshCutResult.secondSideMesh,
				cuttedCollidersRepresentations = collidersCutResults.secondSideColliderRepresentations
			};

			/* Second side result - created as new GameObject or instantiated copy of the original GameObject. */

			SpriteCutterGameObject secondSideResult = null;
			switch ( input.gameObjectCreationMode ) {
				case GameObjectCreationMode.CUT_OFF_NEW:
				case GameObjectCreationMode.CUT_INTO_TWO:
					secondSideResult = SpriteCutterGameObject.CreateNew( input.gameObject, true );
					PrepareResultGameObject( secondSideResult, spriteRenderer, meshRenderer, secondCutData );
					break;

				case GameObjectCreationMode.CUT_OFF_COPY:
					secondSideResult = SpriteCutterGameObject.CreateAsInstantiatedCopyOf( input.gameObject, true );
					SpriteRenderer copiedSpriteRenderer = secondSideResult.gameObject.GetComponent<SpriteRenderer>();
				    MeshRenderer copiedMeshRenderer = secondSideResult.gameObject.GetComponent<MeshRenderer>();
					if ( copiedSpriteRenderer != null ) {
						RendererParametersRepresentation tempParameters = new RendererParametersRepresentation();
						tempParameters.CopyFrom( copiedSpriteRenderer );
						SafeSpriteRendererRemoverBehaviour.get.RemoveAndWaitOneFrame( copiedSpriteRenderer, onFinish: () => {
								PrepareResultGameObject( secondSideResult, tempParameters, secondCutData );
						} );
						
					} else {
						PrepareResultGameObject( secondSideResult, copiedSpriteRenderer, copiedMeshRenderer, secondCutData );
					}
					break;
			}


			/* First side result */

			SpriteCutterGameObject firstSideResult = null;
			switch ( input.gameObjectCreationMode ) {
				case GameObjectCreationMode.CUT_OFF_NEW:
				case GameObjectCreationMode.CUT_OFF_COPY:
					firstSideResult = SpriteCutterGameObject.CreateAs( input.gameObject );
					if ( spriteRenderer != null ) {
						RendererParametersRepresentation tempParameters = new RendererParametersRepresentation();
						tempParameters.CopyFrom( spriteRenderer );
						SafeSpriteRendererRemoverBehaviour.get.RemoveAndWaitOneFrame( spriteRenderer, onFinish: () => {
							PrepareResultGameObject( firstSideResult, tempParameters, firstCutData );
						} );
						
					} else {
						PrepareResultGameObject( firstSideResult, spriteRenderer, meshRenderer, firstCutData );
					}
					break;

				case GameObjectCreationMode.CUT_INTO_TWO:
					firstSideResult = SpriteCutterGameObject.CreateNew( input.gameObject, true );
					PrepareResultGameObject( firstSideResult, spriteRenderer, meshRenderer, firstCutData );
					break;
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
		                                     MeshRenderer meshRenderer, SideCutData cutData ) {
			resultGameObject.AssignMeshFilter( cutData.cuttedMesh );
			if ( spriteRenderer != null ) {
				resultGameObject.AssignMeshRendererFrom( spriteRenderer );
			} else {
				resultGameObject.AssignMeshRendererFrom( meshRenderer );
			}

			if ( cutData.cuttedCollidersRepresentations != null ) {
				resultGameObject.BuildCollidersFrom( cutData.cuttedCollidersRepresentations );
			}
		}
		
		static void PrepareResultGameObject( SpriteCutterGameObject resultGameObject, RendererParametersRepresentation tempParameters,
		                                     SideCutData cutData ) {
			resultGameObject.AssignMeshFilter( cutData.cuttedMesh );
			resultGameObject.AssignMeshRendererFrom( tempParameters );
			
			if ( cutData.cuttedCollidersRepresentations != null ) {
				resultGameObject.BuildCollidersFrom( cutData.cuttedCollidersRepresentations );
			}
		}

	}

}