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
    public Text cardTitle;
    public Text leftText;
    public Text rightText;
    public Text cardBody;
    public Text spoonsText;
    public Text happinessText;

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


    //to test swiping with
    public void OnEndDrag(PointerEventData eventData)
    {

        Debug.Log("Press position + " + eventData.pressPosition);
        Debug.Log("End position + " + eventData.position);

        Vector3 dragVectorDirection = (eventData.position - eventData.pressPosition).normalized;
        DraggedDirection direction = GetDragDirection(dragVectorDirection);

        Debug.Log("Pre-Image Pos: " + cardImage.transform.position.ToString());

        if (moveToNextCard(direction)){

            if(direction == DraggedDirection.Left){
                iTween.MoveTo(cardImage.gameObject,
                      iTween.Hash("position", cardImage.transform.position += Vector3.left * 3,
                                  "easetype", iTween.EaseType.easeInOutSine, "time", .2f,
                                  "onComplete", "SwipeComplete", "onCompleteTarget", this.gameObject));
            } else if(direction == DraggedDirection.Right){
                iTween.MoveTo(cardImage.gameObject,
                              iTween.Hash("position", cardImage.transform.position += Vector3.right * 3,
                                  "easetype", iTween.EaseType.easeInOutSine, "time", .2f,
                                          "onComplete", "SwipeComplete", "onCompleteTarget", this.gameObject));
            }

        }
    }

    void SwipeComplete(){
        cardImage.transform.position = new Vector3(0f, 0f);
        Debug.Log("Post-Image Pos: " + cardImage.transform.position.ToString());
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
}
