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
    public string content;        // Filename in Resources/Items
    public bool shouldBeInZone;
    public string mistakeFeedback;
}

[Serializable]
public class AnswerOption
{
    public string answerText;   // For Standard Buttons
    public string feedbackText; // Popup Text
    public bool isCorrect;
}

[Serializable]
public class Question
{
    public string questionText;
    public QuestionType type;

    // For Standard Questions
    public AnswerOption[] options;

    // For Drag and Drop Questions
    public DragItemData[] dragItems;
}

[Serializable]
public class QuizData
{
    public string quizName;
    public List<Question> questions = new List<Question>();
}