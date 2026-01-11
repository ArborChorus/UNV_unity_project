using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizMenuController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Drag your .json files here (Quiz1, Quiz2, Quiz3)")]
    public TextAsset[] quizFiles;

    [Header("UI References")]
    public GameObject menuPanel;
    public Button[] quizButtons; // Drag your 3 buttons here

    [Header("Connection to Game")]
    public QuizGameplayController gameController;

    private List<QuizData> loadedQuizzes = new List<QuizData>();

    void Start()
    {
        // 1. Load and Parse all JSON files
        LoadQuizzes();

        // 2. Setup Buttons
        SetupButtons();

        // 3. Ensure Menu is visible at start
        ShowMenu();
    }

    public void ShowMenu()
    {
        menuPanel.SetActive(true);

        // Hide game panel just in case
        if (gameController != null)
            gameController.gameObject.SetActive(false);
    }

    void LoadQuizzes()
    {
        loadedQuizzes.Clear();

        foreach (TextAsset file in quizFiles)
        {
            if (file != null)
            {
                try
                {
                    QuizData data = JsonUtility.FromJson<QuizData>(file.text);
                    if (data != null) loadedQuizzes.Add(data);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing {file.name}: {e.Message}");
                }
            }
        }
    }

    void SetupButtons()
    {
        for (int i = 0; i < quizButtons.Length; i++)
        {
            if (i < loadedQuizzes.Count)
            {
                quizButtons[i].gameObject.SetActive(true);

                // Set Button Text
                TextMeshProUGUI btnText = quizButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = loadedQuizzes[i].quizName;

                // Add Click Listener
                int index = i; // Local copy for closure
                quizButtons[i].onClick.RemoveAllListeners();
                quizButtons[i].onClick.AddListener(() => OnQuizSelected(loadedQuizzes[index]));
            }
            else
            {
                // Hide unused buttons
                quizButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnQuizSelected(QuizData data)
    {
        // Hide Menu
        menuPanel.SetActive(false);

        // Start Game
        gameController.gameObject.SetActive(true);
        gameController.StartQuiz(data);
    }
}