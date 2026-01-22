using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizMenuController : MonoBehaviour
{
    [Header("Configuration")]
    public TextAsset[] quizFiles;

    [Header("UI References")]
    public GameObject quizSelectionPanel;
    public Button[] quizButtons;

    [Header("The Universal Button")]
    public UniversalButton universalButton;

    [Header("External Connections")]
    public MainMenuController mainMenu;
    public QuizGameplayController gameController;

    private List<QuizData> loadedQuizzes = new List<QuizData>();
    private bool isLoaded = false;

    // Start is empty because MainMenu handles the initial show/hide
    void Start() { }

    public void Show()
    {
        quizSelectionPanel.SetActive(true);

        // --- CONFIGURE UNIVERSAL BUTTON ---
        // On Level Select, this button goes back to Main Menu
        if (universalButton != null)
        {
            universalButton.Configure("ÍÀÇÀÄ", GoBack); // "BACK"
        }

        // Load data if needed
        if (!isLoaded)
        {
            LoadQuizzes();
            SetupButtons();
            isLoaded = true;
        }
    }

    public void Hide()
    {
        quizSelectionPanel.SetActive(false);
    }

    void GoBack()
    {
        Hide();
        mainMenu.Show();
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
                catch (System.Exception e) { Debug.LogError(e.Message); }
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
                TextMeshProUGUI btnText = quizButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = loadedQuizzes[i].quizName;

                int index = i;
                quizButtons[i].onClick.RemoveAllListeners();
                quizButtons[i].onClick.AddListener(() => OnLevelSelected(loadedQuizzes[index]));
            }
            else
            {
                quizButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnLevelSelected(QuizData data)
    {
        Hide();
        gameController.gameObject.SetActive(true);
        gameController.StartQuiz(data);
    }
}