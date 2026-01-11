using UnityEngine;
using UnityEngine.UI;

public class MapMinigame : MonoBehaviour
{
    [Header("Controller Reference")]
    public QuizGameplayController gameplayController;

    [Header("The Zones (Drag Buttons Here)")]
    public Button zone1_Wrong;    // e.g. Window
    public Button zone2_Almost;   // e.g. Middle of room
    public Button zone3_Correct;  // e.g. Corridor
    public Button zone4_Bad;      // <--- NEW: e.g. Balcony or Kitchen

    [Header("Feedback Text")]
    [TextArea] public string feedback1_Wrong = "Wrong! Windows are dangerous.";
    [TextArea] public string feedback2_Almost = "Not safe enough. This is too exposed.";
    [TextArea] public string feedback3_Correct = "Correct! The two-wall rule saves lives.";
    [TextArea] public string feedback4_Bad = "Very dangerous! Do not go here."; // <--- NEW

    void Start()
    {
        // 1. Wrong Zone (Window)
        if (zone1_Wrong)
            zone1_Wrong.onClick.AddListener(() => OnZoneClicked(false, feedback1_Wrong));

        // 2. Almost Zone (Center)
        if (zone2_Almost)
            zone2_Almost.onClick.AddListener(() => OnZoneClicked(false, feedback2_Almost));

        // 3. Correct Zone (Corridor)
        if (zone3_Correct)
            zone3_Correct.onClick.AddListener(() => OnZoneClicked(true, feedback3_Correct));

        // 4. New Bad Zone (e.g. Balcony)
        if (zone4_Bad)
            zone4_Bad.onClick.AddListener(() => OnZoneClicked(false, feedback4_Bad));
    }

    void OnZoneClicked(bool isCorrect, string feedback)
    {
        // Report result to main controller
        gameplayController.ReportMapResult(isCorrect, feedback);
    }
}