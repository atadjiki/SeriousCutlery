using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour {

    public string title;
    public string body;
    public Card leftNode;
    public string leftNodeText;
    public Card rightNode;
    public string rightNodeText;
    public Color color;
    public Texture2D image;
    public int spoonModifier;
    public int happinessModifier;
    public ActionType action;
    public ActionType checkAction;
    public CardType type;

    public enum ActionType { None, ForgotGroceryList, Done };
    public enum CardType { None, Status };

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
