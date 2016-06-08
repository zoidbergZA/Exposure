using UnityEngine;
using System.Collections;

public class DBconnection : MonoBehaviour {
	private Arguments argumentsScript;
	private string connectionURL;
	private string scoreURL = "insertScore.php?";

	void Awake() {
		argumentsScript = GameObject.Find("SceneManager").GetComponent<Arguments>();
	}
	void Start() {
		connectionURL = argumentsScript.getConURL();
	}

	public IEnumerator UploadScore(int userID, int gameID, int score) {
		string full_url = connectionURL + scoreURL + "userID=" + userID + "&gameID=" + gameID + "&score=" + score;
		WWW post = new WWW(full_url);
		yield return post;
		Application.Quit();
	}
}
