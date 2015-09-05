using UnityEngine;
using System.Collections.Generic;
using UnitySpriteCutter;

public class LinecastCutterBehaviour : MonoBehaviour {

	public LayerMask layerMask;

	Vector2 mouseStart;
	void Update() {

		if ( Input.GetMouseButtonDown( 0 ) ) {
			mouseStart = Camera.main.ScreenToWorldPoint( Input.mousePosition );
		}

		Vector2 mouseEnd = Camera.main.ScreenToWorldPoint( Input.mousePosition );

		if ( Input.GetMouseButtonUp( 0 ) ) {
			LinecastCut( mouseStart, mouseEnd, layerMask.value );
		}
	}
	
	void LinecastCut( Vector2 lineStart, Vector2 lineEnd, int layerMask = Physics2D.AllLayers ) {
		List<GameObject> gameObjectsToCut = new List<GameObject>();
		RaycastHit2D[] hits = Physics2D.LinecastAll( lineStart, lineEnd, layerMask );
		foreach ( RaycastHit2D hit in hits ) {
			if ( HitCounts( hit ) ) {
				gameObjectsToCut.Add( hit.transform.gameObject );
			}
		}
		
		foreach ( GameObject go in gameObjectsToCut ) {
			SpriteCutterOutput output = SpriteCutter.Cut( new SpriteCutterInput() {
				lineStart = lineStart,
				lineEnd = lineEnd,
				gameObject = go,
				gameObjectCreationMode = SpriteCutterInput.GameObjectCreationMode.CUT_OFF_COPY,
			} );
		}
	}

	bool HitCounts( RaycastHit2D hit ) {
		return ( hit.transform.GetComponent<SpriteRenderer>() != null ||
		         hit.transform.GetComponent<MeshRenderer>() != null );
	}

}
