using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //UI variables
    public RawImage cardImage;
    public RawImage nextCardImage;
    public Text cardTitle;
    public Text nextCardTitle;
    public Text leftText;
    public Text rightText;
    public Text cardBody;
    public Text nextCardBody;
    public Text spoonsText;
    public Text happinessText;
    public bool allowDrag = true;

    //game variables
    public Card currentCard; //initialize this as the root node
    public int spoons;
    public int happiness;

    // Use this for initialization
    void Start()
    {

        EventTrigger trigger = cardImage.GetComponent<EventTrigger>();
        Debug.Log(cardImage.GetComponent<EventTrigger>().name);
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.EndDrag;
        entry.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
        trigger.triggers.Add(entry);

        cardTitle.text = currentCard.title;
        cardBody.text = currentCard.body;
        cardImage.texture = currentCard.image;
        leftText.text = currentCard.leftNode.title + getSpoonText(currentCard.leftNode);
        rightText.text = currentCard.rightNode.title + getSpoonText(currentCard.rightNode);

        spoonsText.text += spoons;
        happinessText.text += happiness;

    }

    // Update is called once per frame
    void Update()
    {

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
        Debug.Log("Post-Image Pos: " + cardImage.transform.position.ToString());
        Debug.Log("Post-Image Rot: " + cardImage.transform.rotation.ToString());

        allowDrag = true;

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

    private bool moveToNextCard(DraggedDirection direction)
    {

        bool validMove = false;

        //if swipe right, go to the right node of the current card
        if (direction == DraggedDirection.Right)
        {
            if (currentCard.rightNode != null)
            {
                currentCard = currentCard.rightNode; //set current node to right child

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
                leftText.text = currentCard.leftNode.title + getSpoonText(currentCard.leftNode);
            else
                leftText.text = "";
            if (currentCard.rightNode != null)
                rightText.text = currentCard.rightNode.title + getSpoonText(currentCard.rightNode);
            else
                rightText.text = "";
        }

        return validMove;
    }

    private string getSpoonText(Card card)
    {
        string spoonText = " (";
        if (card.spoonModifier > 0)
            spoonText += "+";
        spoonText += card.spoonModifier + ")";

        return spoonText;
    }

    private void processCurrentCard()
    {
        spoons += currentCard.spoonModifier;
        happiness += currentCard.happinessModifier;

        spoonsText.text = "Spoons: " + spoons;
        happinessText.text = "Happiness: " + happiness;


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
   
}
