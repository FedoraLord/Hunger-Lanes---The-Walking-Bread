using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {

    public GameObject titleScreen;
    public GameObject gameScreen;

    public Event[] events;

    public void PlayGame()
    {
        titleScreen.SetActive(false);
        gameScreen.SetActive(true);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
