using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

	public static MenuController instance;

	// Use this for initialization
	void Awake () {
		MakeInstance ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void MakeInstance(){
		if (instance == null) {
			instance = this;
		}
	}

	public void ClickPlayButton(){
		SceneManager.LoadScene (1);
	}

	public void CLickSoundButton(){
	
	}

	public void ClickHintButton(){
		
	}
}
