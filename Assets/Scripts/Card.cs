﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour {

    public string title;
    public string body;
    public Card leftNode;
    public Card rightNode;
    public Color color;
    public Texture2D image;
    public int spoonModifier;
    public int happinessModifier;

    public enum CardType { Action, Event, Activity, Day};

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
