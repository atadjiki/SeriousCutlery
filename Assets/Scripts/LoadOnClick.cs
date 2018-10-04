using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadOnClick : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void startGameScene(){

        SceneManager.LoadScene("Game");

    }

    public void goToWiki(){

        Application.OpenURL("https://en.wikipedia.org/wiki/Spoon_theory");
    }
}
