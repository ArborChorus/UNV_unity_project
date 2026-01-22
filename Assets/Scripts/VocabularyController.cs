using UnityEngine;
using UnityEngine.UI;
using TMPro; // Essential for TextMeshPro
using System.Collections.Generic;

public class VocabularyController : MonoBehaviour
{
    [Header("Data")]
    public TextAsset jsonFile;

    [Header("Visual Settings")]
    public Color cardColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Light Gray
    public Sprite cardBackground; // Drag "Knob" or "UISprite" here for rounded corners
    public TMP_FontAsset fontAsset; // Drag your TMP Font here (e.g., LiberationSans)
    public Color textColor = Color.black;
    [Range(10, 50)] public float titleSize = 24f;
    [Range(10, 50)] public float bodySize = 18f;

    [Header("UI References")]
    public GameObject vocabularyPanel;
    public Transform contentContainer; // The "Content" object inside Scroll View

    [Header("The Universal Button")]
    public UniversalButton universalButton;

    [Header("External Connections")]
    public MainMenuController mainMenu;

    private bool isLoaded = false;

    // --- JSON CLASSES ---
    [System.Serializable]
    public class VocabTerm
    {
        public string word;
        [TextArea] public string definition;
    }

    [System.Serializable]
    public class VocabList
    {
        public List<VocabTerm> terms;
    }
    // --------------------

    void Start() { }

    public void Show()
    {
        vocabularyPanel.SetActive(true);

        if (universalButton != null)
        {
            universalButton.Configure("ÍÀÇÀÄ", GoBack);
        }

        if (!isLoaded && jsonFile != null)
        {
            GenerateList();
            isLoaded = true;
        }
    }

    public void Hide()
    {
        vocabularyPanel.SetActive(false);
    }

    void GoBack()
    {
        Hide();
        mainMenu.Show();
    }

    void GenerateList()
    {
        // 1. Clear existing items
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Parse Data
        VocabList data = JsonUtility.FromJson<VocabList>(jsonFile.text);
        if (data == null || data.terms == null) return;

        // 3. Create Objects via Code
        foreach (VocabTerm term in data.terms)
        {
            CreateVocabularyCard(term);
        }
    }

    void CreateVocabularyCard(VocabTerm term)
    {
        // A. Create the Main Card (The Background Image)
        GameObject cardObj = new GameObject("Card_" + term.word);
        cardObj.transform.SetParent(contentContainer, false);

        // Add Image
        Image img = cardObj.AddComponent<Image>();
        img.color = cardColor;
        if (cardBackground != null) img.sprite = cardBackground;
        img.type = Image.Type.Sliced; // Allows resizing without distortion if sprite supports it

        // Add Vertical Layout (To stack Title and Def inside)
        VerticalLayoutGroup layout = cardObj.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 10;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        // Add Content Size Fitter (So the card grows to fit text)
        ContentSizeFitter fitter = cardObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // B. Create Title Text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(cardObj.transform, false);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = term.word;
        titleText.font = fontAsset;
        titleText.fontSize = titleSize;
        titleText.color = textColor;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Left;

        // C. Create Definition Text
        GameObject defObj = new GameObject("Definition");
        defObj.transform.SetParent(cardObj.transform, false);

        TextMeshProUGUI defText = defObj.AddComponent<TextMeshProUGUI>();
        defText.text = term.definition;
        defText.font = fontAsset;
        defText.fontSize = bodySize;
        defText.color = textColor;
        defText.alignment = TextAlignmentOptions.Left;
        defText.enableWordWrapping = true; // Crucial for long text
    }
}