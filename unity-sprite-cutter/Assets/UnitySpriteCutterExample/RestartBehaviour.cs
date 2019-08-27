using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartBehaviour : MonoBehaviour {

	/// <summary>
	/// Simple restart method triggered by UI.Button.
	/// </summary>
	public void RestartScene() {
		SceneManager.LoadScene(0);
	}

}
