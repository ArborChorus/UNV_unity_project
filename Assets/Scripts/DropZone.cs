using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj != null)
        {
            // By setting the parent, we "capture" the item
            droppedObj.transform.SetParent(this.transform);
        }
    }
}