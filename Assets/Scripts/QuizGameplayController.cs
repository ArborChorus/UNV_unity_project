using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;

public class QuizGameplayController : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button quitButton; // The "X" in top corner

    [Header("Result Popup")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitle;
    [SerializeField] private TextMeshProUGUI resultExplanation;
    [SerializeField] private Button popupActionButton; // The button on the popup
    [SerializeField] private TextMeshProUGUI popupBtnText; // The Text INSIDE that button

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
    // Link this to MenuController.ShowMenu() in Inspector!
    public UnityEvent onBackToMenu;

    // State
    private QuizData currentQuiz;
    private int questionIndex;
    private int score;
    private bool lastAnswerWasCorrect;
    private bool isQuizFinished = false;

    private List<GameObject> spawnedDraggables = new List<GameObject>();

    void Start()
    {
        if (popupActionButton != null) popupActionButton.onClick.AddListener(OnPopupBtnClicked);
        if (dragSubmitButton != null) dragSubmitButton.onClick.AddListener(OnDragSubmit);
        if (quitButton != null) quitButton.onClick.AddListener(QuitToMenu);
    }

    public void StartQuiz(QuizData quizToPlay)
    {
        currentQuiz = quizToPlay;
        questionIndex = 0;
        score = 0;
        isQuizFinished = false;

        gameplayPanel.SetActive(true);
        resultPanel.SetActive(false);

        LoadQuestion();
    }

    public void QuitToMenu()
    {
        // Cleanup
        ClearAllItems();
        gameplayPanel.SetActive(false);
        resultPanel.SetActive(false);

        // Tell MenuController to show up
        onBackToMenu?.Invoke();
    }

    void EndQuiz()
    {
        isQuizFinished = true;

        HideAllPanels();
        questionText.gameObject.SetActive(false);

        resultPanel.SetActive(true);
        resultTitle.text = "ВІКТОРИНУ ЗАВЕРШЕНО"; // QUIZ COMPLETED
        resultTitle.color = Color.white;
        resultExplanation.text = $"Ви завершили вікторину!\n\nФінальний рахунок: {score} / {currentQuiz.questions.Count}";

        // Change button to "Menu"
        if (popupBtnText != null) popupBtnText.text = "В меню";
    }

    void OnPopupBtnClicked()
    {
        if (isQuizFinished)
        {
            QuitToMenu();
        }
        else
        {
            // Next Question
            resultPanel.SetActive(false);
            if (popupBtnText != null) popupBtnText.text = "Далі";

            if (lastAnswerWasCorrect)
            {
                questionIndex++;
                LoadQuestion();
            }
            else
            {
                LoadQuestion(); // Retry logic
            }
        }
    }

    // --- GAMEPLAY LOGIC ---

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

        if (questionIndex >= currentQuiz.questions.Count)
        {
            EndQuiz();
            return;
        }

        Question q = currentQuiz.questions[questionIndex];

        // Reset UI
        if (questionText != null)
        {
            questionText.gameObject.SetActive(true);
            questionText.text = q.questionText;
        }

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

    void HideAllPanels()
    {
        standardPanel.SetActive(false);
        dragDropPanel.SetActive(false);
        mapPanel.SetActive(false);
        hiddenObjectPanel.SetActive(false);
    }

    public void ReportMapResult(bool isCorrect, string text) { OnResult(isCorrect, text); }

    void OnResult(bool isCorrect, string text)
    {
        resultExplanation.text = text;
        lastAnswerWasCorrect = isCorrect;

        if (isCorrect)
        {
            resultTitle.text = "ПРАВИЛЬНО";
            resultTitle.color = Color.green;
            score++;
        }
        else
        {
            resultTitle.text = "НЕПРАВИЛЬНО";
            resultTitle.color = Color.red;
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
            if (item.shouldBeInZone) totalRequired++;

        foreach (var item in itemsInZone)
        {
            if (item.data.shouldBeInZone) correctItemsFound++;
            else { isPerfect = false; errorMessages.Add($"{item.data.mistakeFeedback}"); }
        }

        if (correctItemsFound < totalRequired) { isPerfect = false; errorMessages.Add("Пропущено елементи."); }

        string finalFeedback = isPerfect ? "Чудово!" : "Помилки:\n" + string.Join("\n", errorMessages);
        OnResult(isPerfect, finalFeedback);
    }
}