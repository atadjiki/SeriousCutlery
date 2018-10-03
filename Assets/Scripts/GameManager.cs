using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public Text energyText;
    public Text happinessText;
    public GameObject hudPanel;


    public bool displayModifiers;
    public bool displayHUD;
    private bool allowDrag = true;
    private int defaultEnergy;
    private int defaultHappiness;

    //game variables
    public Card currentCard; //initialize this as the root node
    public List<Card> chores;
    public int choreIndex;
    public int energy;
    public int happiness;
    

    //game bools
    private bool forgotGroceryList = false;

    // Use this for initialization
    void Start()
    {

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


        btnDog.onClick.AddListener(delegate { ChecklistButtonPress(btnDog);});
        btnGroceries.onClick.AddListener(delegate { ChecklistButtonPress(btnGroceries); });
        btnLaundry.onClick.AddListener(delegate { ChecklistButtonPress(btnLaundry); });
        btnMealPrep.onClick.AddListener(delegate { ChecklistButtonPress(btnMealPrep); });

        background.texture = backgrounds[backgroundIndex];
        backgroundIndex++;

        energyText.text += energy;
        happinessText.text += happiness;

        defaultEnergy = energy;
        defaultHappiness = happiness;

        if(!displayHUD){
            energyText.enabled = false;
            happinessText.enabled = false;
            hudPanel.SetActive(false);
        }
        initializeGame();

    }

    // Update is called once per frame
    void Update()
    {
        if(energy <= 0){

            currentCard = GameObject.Find("Lose").GetComponent<Card>();
            initializeGame();
            
        }
    }


    /*
     * Swipes to the next card
     */ 
    public void OnEndDrag(PointerEventData eventData)
    {
        if(allowDrag){

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

            } else{
                allowDrag = true;
            }
        }
    }

    private void setNextCard(Card nextCard)
    {
        if(nextCard != null){
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

    void CheckListSwipeComplete(){

        checkList.transform.position = new Vector3(0f, 0f);
        checkList.transform.rotation = Quaternion.Euler(0, 0, 0);
        checkList.gameObject.SetActive(false);
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

    void DoCheckListSwipe(PointerEventData eventData){

        if (!lockButtons) return;

        Debug.Log("Press position + " + eventData.pressPosition);
        Debug.Log("End position + " + eventData.position);

        Vector3 dragVectorDirection = (eventData.position - eventData.pressPosition).normalized;
        DraggedDirection direction = GetDragDirection(dragVectorDirection);

        Debug.Log("Pre-Image Pos: " + checkList.transform.position.ToString());

        if(direction == DraggedDirection.Left){

            float tiltAroundZ = 25f;
            Quaternion target = Quaternion.Euler(0, 0, tiltAroundZ);

            // Dampen towards the target rotation
            checkList.transform.rotation = Quaternion.Slerp(checkList.transform.rotation, target, 0.3f);

            iTween.MoveTo(checkList.gameObject,
                  iTween.Hash("x", -6,
                              "time", 0.9f,
                              "onComplete", "CheckListSwipeComplete", "onCompleteTarget", this.gameObject));

        } else if(direction == DraggedDirection.Right){

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
        if(displayModifiers){
            energyText = " (";
            if (card.spoonModifier > 0)
                energyText += "+";
            energyText += card.spoonModifier + ")";
        }

        return energyText;
    }

    private void processCurrentCard()
    {
        energy += currentCard.spoonModifier;
        happiness += currentCard.happinessModifier;

        energyText.text = "Energy: " + energy;
        happinessText.text = "Happiness: " + happiness;

        checkDone();                                                             
        checkForgotGroceryList();
        checkStatusCard();
        checkChecklistCard();

    }

    private void checkChecklistCard(){

        if(currentCard.type == Card.CardType.Checklist){

            showChecklist();
        }
    }

    private void checkStatusCard()
    {
        if(currentCard.type == Card.CardType.Status)
        {
            //generate status card
            currentCard = GameObject.Find("Status").GetComponent<Card>();
            cardTitle.text = currentCard.title;
            cardBody.text = currentCard.body;
            cardImage.texture = currentCard.image;
            if(chores.Count > choreIndex)
            {
                currentCard.rightNode = GameObject.Find("ChecklistCard").GetComponent<Card>();
                currentCard.rightNodeText = "Continue...";
                choreIndex++;
            }
            if(backgrounds.Count > backgroundIndex)
            {
                background.texture = backgrounds[backgroundIndex];
                backgroundIndex++;
            }
            
            
            
        }
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

    public void initializeGame(){
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

        energy = defaultEnergy;
        happiness = defaultHappiness;
        forgotGroceryList = false;
    }

    public void showChecklist(){

        checkList.gameObject.SetActive(true);
        lockButtons = false;
    }

    public void ChecklistButtonPress(Button button){

        if(lockButtons == false){
            if (button.Equals(btnGroceries))
            {
                Debug.Log("Groceries Pressed");
                lineGroceries.gameObject.SetActive(true);
                btnGroceries.enabled = false;
            }
            else if (button.Equals(btnLaundry))
            {
                Debug.Log("Laundry Pressed");
                lineLaundry.gameObject.SetActive(true);
                btnLaundry.enabled = false;
            }
            else if (button.Equals(btnMealPrep))
            {
                Debug.Log("Meal Prep Pressed");
                lineMealPrep.gameObject.SetActive(true);
                btnMealPrep.enabled = false;
            }
            else if (button.Equals(btnDog))
            {
                Debug.Log("Dog Pressed");
                lineDog.gameObject.SetActive(true);
                btnDog.enabled = false;
            }

            lockButtons = true;
        }
       
    }
   
}
