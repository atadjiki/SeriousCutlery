﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    //UI variables
    public GameObject checkList;
    public Image lineGroceries;
    public Image lineLaundry;
    public Image lineMealPrep;
    public Image lineDog;
    public Button btnGroceries;
    public Button btnLaundry;
    public Button btnMealPrep;
    public Button btnDog;
    private bool lockButtons = true;

    public List<Texture2D> backgrounds;
    public int backgroundIndex;
    public RawImage background;
    public RawImage cardImage;
    public RawImage nextCardImage;
    public Text cardTitle;
    public Text nextCardTitle;
    public Text leftText;
    public Text rightText;
    public Text cardBody;
    public Text nextCardBody;

    public bool displayModifiers;
    public bool displayHUD;
    private bool allowDrag = true;
    public int defaultEnergy;
    public int defaultHappiness;

    //game variables
    public Card currentCard; //initialize this as the root node
    public int energy;
    public int happiness;
    public Card dogChore;
    public Card groceryChore;
    public Card laundryChore;
    public Card mealPrepChore;

    public enum EndStatus { NightOut, ComeOver, NightAlone, BedEarly, End };

    public bool skipToEnd;

    //game bools
    private bool forgotGroceryList = false;

    // Use this for initialization
    void Start()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();

        EventTrigger cardImageTrigger = cardImage.GetComponent<EventTrigger>();
        Debug.Log(cardImage.GetComponent<EventTrigger>().name);
        EventTrigger.Entry cardEntry = new EventTrigger.Entry();
        cardEntry.eventID = EventTriggerType.EndDrag;
        cardEntry.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
        cardImageTrigger.triggers.Add(cardEntry);

        EventTrigger checkListTrigger = checkList.GetComponent<EventTrigger>();
        Debug.Log(checkList.GetComponent<EventTrigger>().name);
        EventTrigger.Entry checklistEntry = new EventTrigger.Entry();
        checklistEntry.eventID = EventTriggerType.EndDrag;
        checklistEntry.callback.AddListener((data) => { DoCheckListSwipe((PointerEventData)data); });
        checkListTrigger.triggers.Add(checklistEntry);


        btnDog.onClick.AddListener(delegate { ChecklistButtonPress(btnDog); });
        btnGroceries.onClick.AddListener(delegate { ChecklistButtonPress(btnGroceries); });
        btnLaundry.onClick.AddListener(delegate { ChecklistButtonPress(btnLaundry); });
        btnMealPrep.onClick.AddListener(delegate { ChecklistButtonPress(btnMealPrep); });

        background.texture = backgrounds[backgroundIndex];
        backgroundIndex++;

        initializeGame();

        if (skipToEnd)
        {
            lineDog.gameObject.SetActive(true);
            lineLaundry.gameObject.SetActive(true);
            lineMealPrep.gameObject.SetActive(true);
            lineGroceries.gameObject.SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (energy <= 0)
        {

            currentCard = GameObject.Find("Lose").GetComponent<Card>();
            initializeGame();
            currentCard.body = generateStatusText();

        }
    }


    /*
     * Swipes to the next card
     */
    public void OnEndDrag(PointerEventData eventData)
    {
        if (allowDrag)
        {

            allowDrag = false;
            Debug.Log("Press position + " + eventData.pressPosition);
            Debug.Log("End position + " + eventData.position);

            Vector3 dragVectorDirection = (eventData.position - eventData.pressPosition).normalized;
            DraggedDirection direction = GetDragDirection(dragVectorDirection);

            Debug.Log("Pre-Image Pos: " + cardImage.transform.position.ToString());

            //set the next card image to the nove we are moving to
            if (direction == DraggedDirection.Left)
                setNextCard(currentCard.leftNode);
            else if (direction == DraggedDirection.Right)
                setNextCard(currentCard.rightNode);


            if (moveToNextCard(direction))
            {
                //move and rotate the card
                DoTinderSwipe(direction);

            }
            else
            {
                allowDrag = true;
            }
        }
    }

    private void setNextCard(Card nextCard)
    {
        if (nextCard != null)
        {
            nextCardImage.texture = nextCard.image;
            nextCardBody.text = nextCard.body;
            nextCardTitle.text = nextCard.title;
        }

    }

    /*
     * Callback function for tween
     */
    void SwipeComplete()
    {
        cardImage.transform.position = new Vector3(0f, 0f);
        cardImage.transform.rotation = Quaternion.Euler(0, 0, 0);
        nextCardImage.transform.position = new Vector3(0f, 0f);
        Debug.Log("Post-Image Pos: " + cardImage.transform.position.ToString());
        Debug.Log("Post-Image Rot: " + cardImage.transform.rotation.ToString());

        allowDrag = true;
    }

    void CheckListSwipeComplete()
    {

        checkList.transform.position = new Vector3(0f, 0f);
        checkList.transform.rotation = Quaternion.Euler(0, 0, 0);
        checkList.gameObject.SetActive(false);

        if (currentCard.leftNode != null)
            leftText.text = currentCard.leftNodeText + getEnergyText(currentCard.leftNode);
        else
            leftText.text = "";
        if (currentCard.rightNode != null)
            rightText.text = currentCard.rightNodeText + getEnergyText(currentCard.rightNode);
        else
            rightText.text = "";
    }

    /*
     * Animates the current card and moves it out of the screen
     */
    void DoTinderSwipe(DraggedDirection direction)
    {
        if (direction == DraggedDirection.Left)
        {

            float tiltAroundZ = 25f;
            Quaternion target = Quaternion.Euler(0, 0, tiltAroundZ);

            // Dampen towards the target rotation
            cardImage.transform.rotation = Quaternion.Slerp(cardImage.transform.rotation, target, 0.3f);

            iTween.MoveTo(cardImage.gameObject,
                  iTween.Hash("x", -6,
                              "time", 0.9f,
                              "onComplete", "SwipeComplete", "onCompleteTarget", this.gameObject));
        }
        else if (direction == DraggedDirection.Right)
        {

            float tiltAroundZ = -25f;
            Quaternion target = Quaternion.Euler(0, 0, tiltAroundZ);

            // Dampen towards the target rotation
            cardImage.transform.rotation = Quaternion.Slerp(cardImage.transform.rotation, target, 0.3f);

            iTween.MoveTo(cardImage.gameObject,
                          iTween.Hash("x", 6,
                                     "time", 0.9f,
                                      "onComplete", "SwipeComplete", "onCompleteTarget", this.gameObject));
        }

    }

    void DoCheckListSwipe(PointerEventData eventData)
    {

        if (!lockButtons) return;

        Debug.Log("Press position + " + eventData.pressPosition);
        Debug.Log("End position + " + eventData.position);

        Vector3 dragVectorDirection = (eventData.position - eventData.pressPosition).normalized;
        DraggedDirection direction = GetDragDirection(dragVectorDirection);

        Debug.Log("Pre-Image Pos: " + checkList.transform.position.ToString());

        if (direction == DraggedDirection.Left)
        {

            float tiltAroundZ = 25f;
            Quaternion target = Quaternion.Euler(0, 0, tiltAroundZ);

            // Dampen towards the target rotation
            checkList.transform.rotation = Quaternion.Slerp(checkList.transform.rotation, target, 0.3f);

            iTween.MoveTo(checkList.gameObject,
                  iTween.Hash("x", -6,
                              "time", 0.9f,
                              "onComplete", "CheckListSwipeComplete", "onCompleteTarget", this.gameObject));

        }
        else if (direction == DraggedDirection.Right)
        {

            float tiltAroundZ = -25f;
            Quaternion target = Quaternion.Euler(0, 0, tiltAroundZ);

            // Dampen towards the target rotation
            checkList.transform.rotation = Quaternion.Slerp(checkList.transform.rotation, target, 0.3f);

            iTween.MoveTo(checkList.gameObject,
                          iTween.Hash("x", 6,
                                     "time", 0.9f,
                                      "onComplete", "CheckListSwipeComplete", "onCompleteTarget", this.gameObject));
        }
    }

    private bool moveToNextCard(DraggedDirection direction)
    {

        bool validMove = false;
        GameObject.Find("Status").GetComponent<Card>().body = "...";

        //if swipe right, go to the right node of the current card
        if (direction == DraggedDirection.Right)
        {
            if (currentCard.rightNode != null)
            {
                currentCard = currentCard.rightNode; //set current node to right child
                Debug.Log("Moving to right node: " + currentCard.title);
                processCurrentCard();
                validMove = true;
            }

        }
        else if (direction == DraggedDirection.Left)
        {
            if (currentCard.leftNode != null)
            {
                currentCard = currentCard.leftNode; //set current node to right child
                Debug.Log("Moving to left node: " + currentCard.title);
                processCurrentCard();
                validMove = true;
            }
        }
        if (direction == DraggedDirection.Left || direction == DraggedDirection.Right)
        {

            cardTitle.text = currentCard.title;
            cardBody.text = currentCard.body;
            cardImage.texture = currentCard.image;

            if (currentCard.leftNode != null)
                leftText.text = currentCard.leftNodeText + getEnergyText(currentCard.leftNode);
            else
                leftText.text = "";
            if (currentCard.rightNode != null)
                rightText.text = currentCard.rightNodeText + getEnergyText(currentCard.rightNode);
            else
                rightText.text = "";
        }

        return validMove;
    }

    private string getEnergyText(Card card)
    {
        string energyText = "";
        if (displayModifiers)
        {
            energyText = " (";
            if (card.energyModifier > 0)
                energyText += "+";
            energyText += card.energyModifier + ")";
        }

        return energyText;
    }

    private void processCurrentCard()
    {
        energy += GetEnergyAmount(currentCard.energyModifier);
        if (currentCard.EnergyBonus) energy++;
        happiness += GetHappinessAmount(currentCard.happinessModifier);

        Debug.Log("Energy: " + energy + ", Happiness: " + happiness);

        checkChecklistCard();
        checkDone();
        checkForgotGroceryList();
        checkFeedbackCard();
        checkStatusCard();
        checkInitialStatusCard();


    }

    private int GetHappinessAmount(Card.HappinessMod happinessModifier)
    {
        if (happinessModifier == Card.HappinessMod.Increase)
        {
            return 1;
        }
        else if (happinessModifier == Card.HappinessMod.Decrease)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    private int GetEnergyAmount(Card.EnergyMod energyModifier)
    {
        if (energyModifier == Card.EnergyMod.Extreme)
        {
            return -4;
        }
        else if (energyModifier == Card.EnergyMod.High)
        {
            return -3;
        }
        else if (energyModifier == Card.EnergyMod.Medium)
        {
            return -2;
        }
        else if (energyModifier == Card.EnergyMod.Low)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    private void checkFeedbackCard()
    {
        if (currentCard.type == Card.CardType.Feedback)
        {

            //generate feedback card
            cardTitle.text = currentCard.title;
            currentCard.body = generateFeedbackText();
            Debug.Log("Feedback Text: " + generateFeedbackText());
            cardImage.texture = currentCard.image;
        }
    }



    private void checkChecklistCard()
    {

        if (currentCard.type == Card.CardType.Checklist)
        {
            rightText.text = "Select a chore";
            Debug.Log("Right text says: " + rightText.text);
            showChecklist();
        }
    }

    private void checkInitialStatusCard()
    {
        if (currentCard.type == Card.CardType.InitialStatus)
        {
            //generate status card
            cardTitle.text = currentCard.title;
            currentCard.body = generateStatusText();
            Debug.Log("Initial Status Text: " + generateStatusText());
            cardImage.texture = currentCard.image;

            if (backgrounds.Count > backgroundIndex)
            {
                background.texture = backgrounds[backgroundIndex];
                backgroundIndex++;
            }
        }
    }

    private void checkStatusCard()
    {
        if (currentCard.type == Card.CardType.Status)
        {
            //generate status card
            currentCard = GameObject.Find("Status").GetComponent<Card>();
            cardTitle.text = currentCard.title;
            currentCard.body = generateStatusText();
            Debug.Log("Status Text: " + generateStatusText());
            cardImage.texture = currentCard.image;
            if (completedAllChores())
            {

                currentCard.rightNode = generateEndCard();
            }
            else
            {
                currentCard.rightNode = GameObject.Find("ChecklistCard").GetComponent<Card>();
            }

            if (backgrounds.Count > backgroundIndex)
            {
                background.texture = backgrounds[backgroundIndex];
                backgroundIndex++;
            }
        }
    }

    private Card generateEndCard()
    {
        EndStatus endStatus = CalculateEndStatus();
        Card endCard = GameObject.Find("Done").GetComponent<Card>();

        if (endStatus == EndStatus.NightOut)
        {

            endCard = GameObject.Find("NightOut").GetComponent<Card>();

        }
        else if (endStatus == EndStatus.ComeOver)
        {

            endCard = GameObject.Find("ComeOver").GetComponent<Card>();

        }
        else if (endStatus == EndStatus.NightAlone)
        {

            endCard = GameObject.Find("NightAlone").GetComponent<Card>();

        }
        else if (endStatus == EndStatus.BedEarly)
        {

            endCard = GameObject.Find("BedEarly").GetComponent<Card>();

        }
        else if (endStatus == EndStatus.End)
        {

            endCard = GameObject.Find("Lose").GetComponent<Card>();

        }

        endCard.body += generateStatusText();
        endCard.rightNodeText = "Start Over?";
        return endCard;
    }

    private bool completedAllChores()
    {
        if (lineDog.gameObject.activeSelf && lineLaundry.gameObject.activeSelf
           && lineMealPrep.gameObject.activeSelf && lineGroceries.gameObject.activeSelf)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private EndStatus CalculateEndStatus()
    {
        float energyPercentage = ((float)energy / defaultEnergy);
        float happyPercentage = ((float)happiness / defaultHappiness);

        Debug.Log("Energy/Default: " + energyPercentage);
        Debug.Log("Happiness/Default: " + happyPercentage);
        //if energy is high
        if (energyPercentage >= 0.75f)
        {

            if (happyPercentage >= 0.75f)
            {

                return EndStatus.NightOut;

            }
            else if (happyPercentage > 0.5f)
            {

                return EndStatus.NightOut;

            }
            else if (happyPercentage > 0f)
            {

                return EndStatus.ComeOver;
            }
            else
            {
                return EndStatus.BedEarly;
            }
        }
        else if (energyPercentage > 0.5f)
        {

            if (happyPercentage > 0.75f)
            {

                return EndStatus.NightOut;

            }
            else if (happyPercentage > 0.5f)
            {

                return EndStatus.ComeOver;

            }
            else if (happyPercentage > 0f)
            {

                return EndStatus.NightAlone;
            }
            else
            {
                return EndStatus.BedEarly;
            }
        }
        else if (energyPercentage > 0f)
        {

            if (happyPercentage > 0.75f)
            {

                return EndStatus.ComeOver;

            }
            else if (happyPercentage > 0.5f)
            {

                return EndStatus.NightAlone;

            }
            else 
            {
                return EndStatus.BedEarly;
            } 

        }
        else
        {

            return EndStatus.End;

        }

    }

    private string generateStatusText()
    {
        float energyPercentage = ((float)energy / defaultEnergy);
        float happyPercentage = ((float)happiness / defaultHappiness);

        Debug.Log("Energy/Default: " + energyPercentage);
        Debug.Log("Happiness/Default: " + happyPercentage);
        //if energy is high
        if (energyPercentage > 0.75f)
        {

            if (happyPercentage > 0.75f)
            {

                return "You're having a great day.";

            }
            else if (happyPercentage > 0.5f)
            {

                return "Your feet are fine and you feel okay.";

            }
            else
            {

                return "Your feet are fine but you're feeling upset";
            }
        }
        else if (energyPercentage > 0.5f)
        {

            if (happyPercentage > 0.75f)
            {

                return "Your feet are sore, but you're feeling great.";

            }
            else if (happyPercentage > 0.5f)
            {

                return "Your feet are sore, but you feel okay.";

            }
            else
            {

                return "Your feet are sore, and you feel awful.";
            }

        }
        else if (energyPercentage > 0f)
        {

            if (happyPercentage > 0.75f)
            {

                return "Your feet hurt, but you're feeling accomplished.";

            }
            else if (happyPercentage > 0.5f)
            {

                return "Your feet hurt, but you're still feeling okay";

            }
            else
            {

                return "Your feet hurt, and you're starting to wish you'd stayed in bed today.";
            }

        }
        else
        {

            if (happyPercentage > 0.75f)
            {

                return "The pain in your feet is unbearable, you wish you could keep going but you're going to have to call it a day.";

            }
            else if (happyPercentage > 0.5f)
            {

                return "The pain in your feet is unbearable, you'll have to call it a day and start again tomorrow.";

            }
            else
            {

                return "The pain in your feet is unbearable, today was not a good day";
            }

        }

    }

    private string generateFeedbackText()
    {
        string result = "";

        if (currentCard.energyModifier == Card.EnergyMod.Low)
        {
            result += "It doesn't take much effort. ";
        }
        else if (currentCard.energyModifier == Card.EnergyMod.Medium)
        {
            result += "It takes a little effort. ";
        }
        else if (currentCard.energyModifier == Card.EnergyMod.High)
        {
            result += "It takes a lot of effort. ";
        }
        else if (currentCard.energyModifier == Card.EnergyMod.Extreme)
        {
            result += "It takes tremendous effort. ";

        }

        if (currentCard.happinessModifier == Card.HappinessMod.Increase)
        {
            result += "You feel a little happier.";
        }
        else if (currentCard.happinessModifier == Card.HappinessMod.Decrease)
        {
            result += "You feel a little more upset.";
        }
        return result;
    }

    private void checkDone()
    {
        if (currentCard.checkAction == Card.ActionType.Done)
        {
            initializeGame();
        }
    }

    private void checkForgotGroceryList()
    {
        if (currentCard.action == Card.ActionType.ForgotGroceryList)
        {
            forgotGroceryList = true;
        }

        if (currentCard.checkAction == Card.ActionType.ForgotGroceryList && forgotGroceryList)
        {

            currentCard = GameObject.Find("Remember You Forgot Something").GetComponent<Card>();
            cardTitle.text = currentCard.title;
            cardBody.text = currentCard.body;
            cardImage.texture = currentCard.image;
            if (currentCard.leftNode != null)
                leftText.text = currentCard.leftNodeText + getEnergyText(currentCard.leftNode);
            else
                leftText.text = "";
            if (currentCard.rightNode != null)
                rightText.text = currentCard.rightNodeText + getEnergyText(currentCard.rightNode);
            else
                rightText.text = "";

        }
    }

    /* Following code from: 
     * http://gamedevelopertips.com/how-detect-swipe-direction-unity/
     */
    private enum DraggedDirection
    {
        Up,
        Down,
        Right,
        Left
    }
    private DraggedDirection GetDragDirection(Vector3 dragVector)
    {
        float positiveX = Mathf.Abs(dragVector.x);
        float positiveY = Mathf.Abs(dragVector.y);
        DraggedDirection draggedDir;
        if (positiveX > positiveY)
        {
            draggedDir = (dragVector.x > 0) ? DraggedDirection.Right : DraggedDirection.Left;
        }
        else
        {
            draggedDir = (dragVector.y > 0) ? DraggedDirection.Up : DraggedDirection.Down;
        }
        Debug.Log(draggedDir);
        return draggedDir;
    }

    public void initializeGame()
    {

        cardTitle.text = currentCard.title;
        cardBody.text = currentCard.body;
        cardImage.texture = currentCard.image;
        if (currentCard.leftNode != null)
            leftText.text = currentCard.leftNodeText + getEnergyText(currentCard.leftNode);
        else
            leftText.text = "";
        if (currentCard.rightNode != null)
            rightText.text = currentCard.rightNodeText + getEnergyText(currentCard.rightNode);
        else
            rightText.text = "";

        energy = Random.Range(defaultEnergy / 2, defaultEnergy);
        happiness = Random.Range(defaultHappiness / 2, defaultHappiness);
        forgotGroceryList = false;
    }

    public void showChecklist()
    {
        
        rightText.text = "Select a chore";
        checkList.gameObject.SetActive(true);
        lockButtons = false;
    }

    public void ChecklistButtonPress(Button button)
    {

        if (lockButtons == false)
        {
            if (button.Equals(btnGroceries))
            {
                Debug.Log("Groceries Pressed");
                lineGroceries.gameObject.SetActive(true);
                btnGroceries.enabled = false;
                currentCard = groceryChore;

            }
            else if (button.Equals(btnLaundry))
            {
                Debug.Log("Laundry Pressed");
                lineLaundry.gameObject.SetActive(true);
                btnLaundry.enabled = false;
                currentCard = laundryChore;
            }
            else if (button.Equals(btnMealPrep))
            {
                Debug.Log("Meal Prep Pressed");
                lineMealPrep.gameObject.SetActive(true);
                btnMealPrep.enabled = false;
                currentCard = mealPrepChore;
            }
            else if (button.Equals(btnDog))
            {
                Debug.Log("Dog Pressed");
                lineDog.gameObject.SetActive(true);
                btnDog.enabled = false;
                currentCard = dogChore;
            }

            lockButtons = true;
            rightText.text = "Continue...";
            cardTitle.text = currentCard.title;
            cardBody.text = currentCard.body;
            cardImage.texture = currentCard.image;

        }

    }

}
