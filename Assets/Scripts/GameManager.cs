using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public GameObject cardPanel;
    public RawImage cardImage;
    public Text cardTitle;
    public Text leftText;
    public Text rightText;
    public Text cardBody;

    public Card currentCard; //initialize this as the root node

    // Use this for initialization
    void Start()
    {
       
        EventTrigger trigger = cardPanel.GetComponent<EventTrigger>();
        Debug.Log(cardPanel.GetComponent<EventTrigger>().name);
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.EndDrag;
        entry.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
        trigger.triggers.Add(entry);

        cardTitle.text = currentCard.title;
        cardBody.text = currentCard.body;
        cardImage.texture = currentCard.image;
        leftText.text = currentCard.leftNode.title;
        rightText.text = currentCard.rightNode.title;
        

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

        //if swipe right, go to the right node of the current card
        if (direction == DraggedDirection.Right)
        {

            if (currentCard.rightNode != null)
            {

                currentCard = currentCard.rightNode; //set current node to right child
                cardTitle.text = currentCard.title;
                cardBody.text = currentCard.body;
                cardImage.texture = currentCard.image;

                if (currentCard.leftNode != null)
                    leftText.text = currentCard.leftNode.title;
                else
                    leftText.text = "";
                if (currentCard.rightNode != null)
                    rightText.text = currentCard.rightNode.title;
                else
                    rightText.text = "";
                
                
                Debug.Log("Moving to right node: " + currentCard.title);

            }

        }
        else if (direction == DraggedDirection.Left)
        {

            if (currentCard.leftNode != null)
            {
                Debug.Log("Moving to left node");
                currentCard = currentCard.leftNode; //set current node to right child
                cardTitle.text = currentCard.title;
                cardImage.texture = currentCard.image;
                Debug.Log("Moving to right node: " + currentCard.title);

            }
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
}
