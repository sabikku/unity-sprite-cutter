using UnityEngine;
using System.Collections;

namespace UnitySpriteCutter.Tools {

	/// <summary>
	/// Removes SpriteRenderer component without player noticing it.
	/// 
	/// You can't destroy one Renderer and add another within one frame, so here's the workaround utility to do it:
	///  - it shows a duplicated sprite over the original
	///  - removes the original SpriteRenderer
	///  - waits for one frame
	///  - removes the duplicated sprite and executes callback delegate
	/// </summary>
	public class SafeSpriteRendererRemoverBehaviour : MonoBehaviour {

		static SafeSpriteRendererRemoverBehaviour instance = null;

		public static SafeSpriteRendererRemoverBehaviour get {
			get {
				if ( instance == null ) {
					GameObject go = new GameObject( "SpriteRendererConverter" );
					GameObject.DontDestroyOnLoad( go );
					instance = go.AddComponent<SafeSpriteRendererRemoverBehaviour>();
				}
				return instance;
			}
		}

		public delegate void OnFinish();

		public void RemoveAndWaitOneFrame( SpriteRenderer spriteRenderer, OnFinish onFinish = null ) {
			
			SpriteRenderer duplicatedSpriteRenderer = CreateDuplicatedSpriteRenderer( spriteRenderer );
			
			GameObject gameObject = spriteRenderer.gameObject;
			spriteRenderer.enabled = false;
			SpriteRenderer.Destroy( spriteRenderer );

			StartCoroutine( EndRemovalAfterOneFrame( gameObject, duplicatedSpriteRenderer, onFinish ) );
		}

		SpriteRenderer CreateDuplicatedSpriteRenderer( SpriteRenderer originalSpriteRenderer ) {
			SpriteRenderer result = new GameObject( "temporaryDuplicatedSpriteRenderer" ).AddComponent<SpriteRenderer>();
			result.transform.position = originalSpriteRenderer.transform.position;
			result.transform.rotation = originalSpriteRenderer.transform.rotation;
			result.transform.localScale = originalSpriteRenderer.transform.localScale;

			result.sprite = originalSpriteRenderer.sprite;
			result.color = originalSpriteRenderer.color;
			result.hideFlags = originalSpriteRenderer.hideFlags;
			result.sortingLayerID = originalSpriteRenderer.sortingLayerID;
			result.sortingOrder = originalSpriteRenderer.sortingOrder;
			return result;
		}

		IEnumerator EndRemovalAfterOneFrame( GameObject gameObject, SpriteRenderer duplicatedSpriteRenderer, OnFinish onFinish ) {
			/*
			 * We assume the http://docs.unity3d.com/ScriptReference/Object.Destroy.html is correct
			 * and that destroying objects will be done right after Update() loop, but before rendering.
			 * Also, we assume that WaitForEndOfFrame() waits till just after the rendering loop.
			 */
			yield return new WaitForEndOfFrame();

			if ( onFinish != null ) {
				onFinish();
			}

			GameObject.Destroy( duplicatedSpriteRenderer.gameObject );
		}
	}

}