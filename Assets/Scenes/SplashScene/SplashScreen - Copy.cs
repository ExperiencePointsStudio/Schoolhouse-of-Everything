using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
	public float delay = 3f;

	private void Start()
	{
		Invoke("LoadNextScene", delay);
	}

	private void LoadNextScene()
	{
		SceneManager.LoadScene("MenuScene");
	}
}
