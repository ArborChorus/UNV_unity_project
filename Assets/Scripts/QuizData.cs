using System;
using System.Collections.Generic;

public enum QuestionType
{
    Standard,       // 0
    DragAndDrop,    // 1
    MapSelection,   // 2
    HiddenObject    // 3
}

[Serializable]
public class DragItemData
{
    public string content;
    public bool shouldBeInZone;
    public string mistakeFeedback;
}

[Serializable]
public class AnswerOption
{
    public string answerText;
    public string feedbackText;
    public bool isCorrect;
}

[Serializable]
public class Question
{
    public string questionText;
    public QuestionType type;

    // --- NEW: HINT FIELD ---
    public string hintText;
    // -----------------------

    public AnswerOption[] options;
    public DragItemData[] dragItems;
}

[Serializable]
public class QuizData
{
    public string quizName;
    public List<Question> questions = new List<Question>();
}