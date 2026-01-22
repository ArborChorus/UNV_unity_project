using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UniversalButton : MonoBehaviour
{
    [Header("References")]
    public Button myButton;
    public TextMeshProUGUI buttonLabel; // Optional, if you want text to change
    public Image buttonIcon;            // Optional, if you want icon to change

    /// <summary>
    /// Call this method to change what the button does and looks like.
    /// </summary>
    /// <param name="text">Text to display (e.g. "Back", "Exit")</param>
    /// <param name="newAction">The function to run when clicked</param>
    public void Configure(string text, UnityAction newAction)
    {
        // 1. Update Text (if available)
        if (buttonLabel != null)
            buttonLabel.text = text;

        // 2. Clear old clicks and add new one
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(newAction);

        // 3. Make sure it's visible
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}