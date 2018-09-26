using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnSwipe : MonoBehaviour {

    Image card;

    private List<Color> colors;
    private int index;

	// Use this for initialization
	void Start () {

        colors = new List<Color>() { Color.red, Color.blue, Color.cyan, Color.yellow, Color.green };
        index = 0;

        card = this.gameObject.GetComponent<Image>();

        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.EndDrag;
        entry.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
        trigger.triggers.Add(entry);

    }
	
	// Update is called once per frame
	void Update () {
		
	}

   
    //to test swiping with
    public void OnEndDrag(PointerEventData eventData)
    {

        Debug.Log("Press position + " + eventData.pressPosition);
        Debug.Log("End position + " + eventData.position);
        Vector3 dragVectorDirection = (eventData.position - eventData.pressPosition).normalized;
        Debug.Log("norm + " + dragVectorDirection);
        DraggedDirection direction = GetDragDirection(dragVectorDirection);

        //if swipe right, go to next color
        if(direction == DraggedDirection.Right){
            if (index < colors.Count-1)
            {
                index++;
                card.color = colors[index];

            }
            else if (index == colors.Count-1)
            {
                index = 0;
                card.color = colors[index];


            }
        } else if(direction == DraggedDirection.Left){
            if (index > 0)
            {
                index--;
                card.color = colors[index];


            }
            else if (index == 0)
            {
                index = colors.Count - 1;
                card.color = colors[index];

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
