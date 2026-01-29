using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq; // Added for convenient list handling

public class QuizGameplayController : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private TextMeshProUGUI questionText;

    [Header("The Universal Button")]
    public UniversalButton universalButton;

    [Header("Hint UI")]
    [SerializeField] private Button hintButton;
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintTextInPanel;
    [SerializeField] private Button closeHintButton;

    [Header("Result Popup")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitle;
    [SerializeField] private TextMeshProUGUI resultExplanation;
    [SerializeField] private Button popupActionButton;
    [SerializeField] private TextMeshProUGUI popupBtnText;

    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite incorrectSprite;

    // --- NEW: GRAND FINALE UI ---
    [Header("Grand Finale (All 3 Quizzes Done)")]
    [SerializeField] private GameObject finalCongratsPanel;
    [SerializeField] private Button finalMenuButton;
    [SerializeField] private Button resetProgressButton; // Optional: for testing/resetting
    // ----------------------------

    [Header("Game Modes")]
    [SerializeField] private GameObject standardPanel;
    [SerializeField] private Button[] answerButtons;

    [SerializeField] private GameObject dragDropPanel;
    [SerializeField] private Transform itemSpawnContainer;
    [SerializeField] private Transform centerDropZone;
    [SerializeField] private GameObject draggablePrefab;
    [SerializeField] private Button dragSubmitButton;

    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject hiddenObjectPanel;

    [Header("Events")]
    public UnityEvent onBackToMenu;

    private QuizData currentQuiz;
    private int questionIndex;
    private int score;
    private bool lastAnswerWasCorrect;
    private bool isQuizFinished = false;
    private List<GameObject> spawnedDraggables = new List<GameObject>();

    private const string PREFS_COMPLETED_KEY = "CompletedQuizzes";
    private const int TOTAL_QUIZZES_TO_WIN = 3;

    void Start()
    {
        if (popupActionButton != null) popupActionButton.onClick.AddListener(OnPopupBtnClicked);
        if (dragSubmitButton != null) dragSubmitButton.onClick.AddListener(OnDragSubmit);

        if (hintButton != null) hintButton.onClick.AddListener(ShowHintPopup);
        if (closeHintButton != null) closeHintButton.onClick.AddListener(HideHintPopup);

        // --- NEW LISTENERS ---
        if (finalMenuButton != null) finalMenuButton.onClick.AddListener(QuitToMenu);
        if (resetProgressButton != null) resetProgressButton.onClick.AddListener(ResetProgress);
    }

    public void StartQuiz(QuizData quizToPlay)
    {
        currentQuiz = quizToPlay;
        questionIndex = 0;
        score = 0;
        isQuizFinished = false;

        gameplayPanel.SetActive(true);
        resultPanel.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);
        if (finalCongratsPanel != null) finalCongratsPanel.SetActive(false);

        if (universalButton != null)
        {
            universalButton.Configure("МЕНЮ", QuitToMenu);
        }

        LoadQuestion();
    }

    public void QuitToMenu()
    {
        ClearAllItems();
        gameplayPanel.SetActive(false);
        resultPanel.SetActive(false);
        if (hintPanel != null) hintPanel.SetActive(false);
        if (finalCongratsPanel != null) finalCongratsPanel.SetActive(false);

        onBackToMenu?.Invoke();
    }

    void EndQuiz()
    {
        isQuizFinished = true;
        HideAllPanels();
        if (questionText != null) questionText.gameObject.SetActive(false);
        if (hintButton != null) hintButton.gameObject.SetActive(false);

        // 1. Mark this specific quiz as complete in Save Data
        SaveQuizCompletion(currentQuiz.quizName);

        // 2. Check how many unique quizzes are finished
        int finishedCount = GetCompletedQuizCount();

        // 3. Logic: If 3 or more are done, show Final Panel. Otherwise, show normal Result.
        if (finishedCount >= TOTAL_QUIZZES_TO_WIN)
        {
            // --- SHOW GRAND FINALE ---
            if (finalCongratsPanel != null)
            {
                finalCongratsPanel.SetActive(true);
                // We do NOT show the normal result panel here
            }
            else
            {
                // Fallback if panel isn't assigned
                ShowStandardEndScreen();
            }
        }
        else
        {
            // --- SHOW STANDARD END SCREEN ---
            ShowStandardEndScreen();
        }
    }

    void ShowStandardEndScreen()
    {
        resultPanel.SetActive(true);

        if (resultTitle != null)
        {
            resultTitle.gameObject.SetActive(true);
            resultTitle.text = "";
            resultTitle.color = Color.white;
        }

        if (resultImage != null && correctSprite != null)
        {
            resultImage.gameObject.SetActive(true);
            resultImage.sprite = correctSprite;
        }

        resultExplanation.text = $"Ви завершили вікторину!\n\nФінальний рахунок: {score} / {currentQuiz.questions.Count}";
        if (popupBtnText != null) popupBtnText.text = "В меню";
    }

    // --- PROGRESS LOGIC ---

    void SaveQuizCompletion(string quizName)
    {
        // Get existing string (Format: "Quiz1|Quiz2|")
        string existing = PlayerPrefs.GetString(PREFS_COMPLETED_KEY, "");

        // If this quiz isn't already in the list, add it
        if (!existing.Contains(quizName + "|"))
        {
            existing += quizName + "|";
            PlayerPrefs.SetString(PREFS_COMPLETED_KEY, existing);
            PlayerPrefs.Save();
            Debug.Log($"Saved progress. Completed: {existing}");
        }
    }

    int GetCompletedQuizCount()
    {
        string existing = PlayerPrefs.GetString(PREFS_COMPLETED_KEY, "");
        // Split by '|' and remove empty entries to get count
        string[] quizzes = existing.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
        return quizzes.Length;
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(PREFS_COMPLETED_KEY);
        PlayerPrefs.Save();
        Debug.Log("Progress Reset!");
        QuitToMenu();
    }

    // ----------------------

    void OnPopupBtnClicked()
    {
        if (isQuizFinished) QuitToMenu();
        else
        {
            resultPanel.SetActive(false);
            if (popupBtnText != null) popupBtnText.text = "Далі";
            if (lastAnswerWasCorrect) { questionIndex++; LoadQuestion(); }
            else { LoadQuestion(); }
        }
    }

    // --- LOGIC ---

    void HideAllPanels()
    {
        if (standardPanel) standardPanel.SetActive(false);
        if (dragDropPanel) dragDropPanel.SetActive(false);
        if (mapPanel) mapPanel.SetActive(false);
        if (hiddenObjectPanel) hiddenObjectPanel.SetActive(false);
    }

    void ClearAllItems()
    {
        foreach (GameObject obj in spawnedDraggables) { if (obj != null) Destroy(obj); }
        spawnedDraggables.Clear();
        foreach (Transform child in itemSpawnContainer) Destroy(child.gameObject);
        foreach (Transform child in centerDropZone) Destroy(child.gameObject);
    }

    void LoadQuestion()
    {
        ClearAllItems();
        if (questionIndex >= currentQuiz.questions.Count) { EndQuiz(); return; }

        Question q = currentQuiz.questions[questionIndex];
        if (questionText != null) { questionText.gameObject.SetActive(true); questionText.text = q.questionText; }

        if (hintButton != null) hintButton.gameObject.SetActive(!string.IsNullOrEmpty(q.hintText));
        if (hintPanel != null) hintPanel.SetActive(false);

        if (q.type == QuestionType.Standard) SetupStandard(q);
        else if (q.type == QuestionType.DragAndDrop) SetupDragAndDrop(q);
        else if (q.type == QuestionType.MapSelection) SetupMapSelection();
        else if (q.type == QuestionType.HiddenObject) SetupHiddenObject();
    }

    void SetupStandard(Question q)
    {
        HideAllPanels();
        standardPanel.SetActive(true);
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i >= q.options.Length) { answerButtons[i].gameObject.SetActive(false); continue; }
            Button btn = answerButtons[i];
            btn.gameObject.SetActive(true);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = q.options[i].answerText;
            AnswerOption opt = q.options[i];
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnResult(opt.isCorrect, opt.feedbackText));
        }
    }

    void SetupDragAndDrop(Question q)
    {
        HideAllPanels();
        dragDropPanel.SetActive(true);
        foreach (var itemData in q.dragItems)
        {
            GameObject newObj = Instantiate(draggablePrefab, itemSpawnContainer);
            DraggableItem script = newObj.GetComponent<DraggableItem>();
            script.Setup(itemData);
            spawnedDraggables.Add(newObj);
        }
    }

    void SetupMapSelection() { HideAllPanels(); mapPanel.SetActive(true); }
    void SetupHiddenObject() { HideAllPanels(); hiddenObjectPanel.SetActive(true); }

    public void ReportMapResult(bool isCorrect, string text) { OnResult(isCorrect, text); }

    void OnResult(bool isCorrect, string text)
    {
        resultExplanation.text = text;
        lastAnswerWasCorrect = isCorrect;

        if (isCorrect)
        {
            score++;
        }

        if (resultTitle != null)
        {
            resultTitle.text = "";
            resultTitle.gameObject.SetActive(false);
        }

        if (resultImage != null)
        {
            resultImage.gameObject.SetActive(true);
            if (isCorrect)
            {
                if (correctSprite != null) resultImage.sprite = correctSprite;
            }
            else
            {
                if (incorrectSprite != null) resultImage.sprite = incorrectSprite;
            }
        }

        resultPanel.SetActive(true);
        if (popupBtnText != null) popupBtnText.text = isCorrect ? "Далі" : "Спробувати ще";
    }

    void OnDragSubmit()
    {
        DraggableItem[] itemsInZone = centerDropZone.GetComponentsInChildren<DraggableItem>();
        List<string> errorMessages = new List<string>();
        bool isPerfect = true;
        int correctItemsFound = 0;
        int totalRequired = 0;

        foreach (var item in currentQuiz.questions[questionIndex].dragItems)
        {
            if (item.content.ToLower().Contains("teddy")) continue;
            if (item.shouldBeInZone) totalRequired++;
        }

        foreach (var item in itemsInZone)
        {
            string itemName = item.data.content.ToLower();
            if (itemName.Contains("teddy")) continue;

            if (item.data.shouldBeInZone) correctItemsFound++;
            else
            {
                isPerfect = false;
                errorMessages.Add($"{item.data.mistakeFeedback}");
            }
        }

        if (correctItemsFound < totalRequired)
        {
            isPerfect = false;
            errorMessages.Add("Пропущено необхідні елементи.");
        }

        string finalFeedback = isPerfect ? "Чудово!" : "Помилки:\n" + string.Join("\n", errorMessages);
        OnResult(isPerfect, finalFeedback);
    }

    void ShowHintPopup() { if (hintPanel != null && hintTextInPanel != null) { hintTextInPanel.text = currentQuiz.questions[questionIndex].hintText; hintPanel.SetActive(true); } }
    void HideHintPopup() { if (hintPanel != null) hintPanel.SetActive(false); }
}