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
    public Texture2D image;
    public EnergyMod energyModifier;
    public HappinessMod happinessModifier;
    public ActionType action;
    public ActionType checkAction;
    public CardType type;
    public bool EnergyBonus;

    public enum EnergyMod { None, Low, Medium, High, Extreme };

    public enum HappinessMod { None, Decrease, Increase };
    public enum ActionType { None, ForgotGroceryList, Done };
    public enum CardType { None, Status, Feedback, Checklist, InitialStatus };

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
