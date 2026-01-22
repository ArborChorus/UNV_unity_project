using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;

    [Header("Navigation Buttons (Internal)")]
    public Button btnGoToQuiz;
    public Button btnGoToVocab;

    [Header("The Universal Button")]
    public UniversalButton universalButton; // Drag the button from hierarchy here

    [Header("Other Controllers")]
    public QuizMenuController quizMenuController;
    public VocabularyController vocabularyController;
    public QuizGameplayController gameController;

    void Start()
    {
        // Setup internal menu buttons
        btnGoToQuiz.onClick.AddListener(OpenQuizMenu);
        btnGoToVocab.onClick.AddListener(OpenVocabulary);

        // Initial State
        Show();
    }

    public void Show()
    {
        mainMenuPanel.SetActive(true);

        // Ensure other panels are closed
        if (quizMenuController) quizMenuController.Hide();
        if (vocabularyController) vocabularyController.Hide();
        if (gameController != null) gameController.gameObject.SetActive(false);

        // --- CONFIGURE UNIVERSAL BUTTON ---
        // On Main Menu, this button quits the app
        if (universalButton != null)
        {
            universalButton.Configure("ÂÈÕ²Ä", QuitGame); // "EXIT"
        }
    }

    public void Hide()
    {
        mainMenuPanel.SetActive(false);
    }

    void OpenQuizMenu()
    {
        Hide();
        quizMenuController.Show();
    }

    void OpenVocabulary()
    {
        Hide();
        vocabularyController.Show();
    }

    void QuitGame()
    {
        Debug.Log("Quitting Application...");
        Application.Quit();
    }
}