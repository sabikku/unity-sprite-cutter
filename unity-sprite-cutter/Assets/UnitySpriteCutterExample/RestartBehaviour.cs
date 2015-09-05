using UnityEngine;
using System.Collections;

public class RestartBehaviour : MonoBehaviour {

	/// <summary>
	/// Simple restart method triggered by UI.Button.
	/// </summary>
	public void RestartScene() {
		Application.LoadLevel( 0 );
	}

}
