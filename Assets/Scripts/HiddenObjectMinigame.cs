using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HiddenObjectMinigame : MonoBehaviour
{
    [Header("Controller Reference")]
    public QuizGameplayController gameplayController;

    [Header("UI References")]
    public Button submitButton; // <--- NEW: Drag a button here (e.g., "Check" or "Done")

    [Header("Dangerous Items (Select all 3)")]
    public Button danger1;
    public Button danger2;
    public Button danger3;

    [Header("Safe Items (Do not select)")]
    public Button safe1;
    public Button safe2;
    public Button safe3;

    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow; // Color when player clicks an item

    [Header("Feedback Messages")]
    [TextArea] public string msgSuccess = "Молодець. Ти правильно розпізнав небезпеку. Тримай дистанцію та повідом дорослих";
    [TextArea] public string msgSelectedSafe = "Ці предмети виглядають безпечними. Зверни увагу на незнайомі або дивні об’єкти";
    [TextArea] public string msgNotAllFound = "Ти помітив не всі небезпечні предмети. Подивись уважніше";

    // Internal State
    private HashSet<Button> selectedButtons = new HashSet<Button>();
    private HashSet<Button> dangerousButtons = new HashSet<Button>();
    private HashSet<Button> safeButtons = new HashSet<Button>();

    void OnEnable()
    {
        ResetGame();
    }

    void Start()
    {
        if (gameplayController == null)
            gameplayController = FindObjectOfType<QuizGameplayController>();

        if (submitButton != null)
            submitButton.onClick.AddListener(CheckAnswer);

        // Group buttons for easy checking
        dangerousButtons.Add(danger1);
        dangerousButtons.Add(danger2);
        dangerousButtons.Add(danger3);

        safeButtons.Add(safe1);
        safeButtons.Add(safe2);
        safeButtons.Add(safe3);

        // Setup Listeners
        SetupButton(danger1);
        SetupButton(danger2);
        SetupButton(danger3);
        SetupButton(safe1);
        SetupButton(safe2);
        SetupButton(safe3);
    }

    void SetupButton(Button btn)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ToggleSelection(btn));
    }

    void ResetGame()
    {
        selectedButtons.Clear();
        ResetButtonVisuals(danger1); ResetButtonVisuals(danger2); ResetButtonVisuals(danger3);
        ResetButtonVisuals(safe1); ResetButtonVisuals(safe2); ResetButtonVisuals(safe3);
    }

    void ResetButtonVisuals(Button btn)
    {
        if (btn == null) return;
        btn.image.color = normalColor;
    }

    void ToggleSelection(Button btn)
    {
        if (selectedButtons.Contains(btn))
        {
            // Deselect
            selectedButtons.Remove(btn);
            btn.image.color = normalColor;
        }
        else
        {
            // Select
            selectedButtons.Add(btn);
            btn.image.color = selectedColor;
        }
    }

    void CheckAnswer()
    {
        bool hasSafeItem = false;
        int foundDangerousCount = 0;

        foreach (Button btn in selectedButtons)
        {
            if (safeButtons.Contains(btn)) hasSafeItem = true;
            if (dangerousButtons.Contains(btn)) foundDangerousCount++;
        }

        // --- CONDITION 1: Player selected a Safe Item (Variant B) ---
        if (hasSafeItem)
        {
            gameplayController.ReportMapResult(false, msgSelectedSafe);
            return;
        }

        // --- CONDITION 2: Player didn't find all Dangerous Items (Variant C) ---
        if (foundDangerousCount < 3)
        {
            gameplayController.ReportMapResult(false, msgNotAllFound);
            return;
        }

        // --- CONDITION 3: All 3 Dangerous, No Safe (Variant A - Win) ---
        if (foundDangerousCount == 3 && !hasSafeItem)
        {
            gameplayController.ReportMapResult(true, msgSuccess);
        }
    }
}