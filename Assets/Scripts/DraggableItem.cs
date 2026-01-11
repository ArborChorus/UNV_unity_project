using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Needed for Image

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    public Image itemImage; // Assign the Image component here
    public CanvasGroup canvasGroup;

    [HideInInspector] public DragItemData data;
    [HideInInspector] public Transform originalParent;

    public void Setup(DragItemData itemData)
    {
        data = itemData;

        // 1. Look for the image in the "Resources/Items" folder
        // The JSON "content" field should match the filename exactly (no extension)
        Sprite loadedSprite = Resources.Load<Sprite>("Items/" + data.content);

        if (loadedSprite != null)
        {
            itemImage.sprite = loadedSprite;
            itemImage.preserveAspect = true; // Keeps the image from stretching weirdly
        }
        else
        {
            Debug.LogError($"DraggableItem: Could not find image named '{data.content}' in Resources/Items folder!");
            // Optional: Set a default "Error" sprite here
            itemImage.color = Color.red;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root); // Move to root so it draws on top of everything
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;  // Allow raycasts to pass through to the DropZone
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // If it wasn't dropped into a zone, go back home
        if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }
}