using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public UIDocument mainUIDocument;
    public VisualTreeAsset tutorialLevelAsset;
    public ProceduralLevelGenerator levelGenerator;

    private VisualElement startScreen;
    private VisualElement mainMenuScreen;
    private Button btnTutorial;
    private Button btnTraining;

    private VisualElement gameScreen;
    private Label livesLabel;
    private Label progressLabel;
    private Label timerLabel;
    private Button safeButton;
    private VisualElement levelContainer;

    private Label scoreLabel;
    private Label finalScoreLabel;
    private VisualElement newRecordContainer;
    private TextField playerNameInput;
    private Button btnSaveScore;
    private readonly List<Label> leaderBoardRows = new List<Label>();

    private int score = 0;

    private const string ScoreNameKey = "HighScoreName_";
    private const string ScoreValueKey = "HighScoreValue_";

    private VisualElement tooltipOverlay;
    private Label tooltipText;

    private VisualElement gameOverScreen;
    private Button restartButton;

    private Button adviceButton;
    private Button magnifierButton;

    private int lives = 3;
    private int adviceUses = 2;
    private int totalTrapsInLevel = 0;
    private int foundTrapsInLevel = 0;

    private float timeLeft = 30f;
    private bool timerActive = false;
    private bool isLevelOver = false;
    private bool isTutorial = false;
    private bool isMagnifierActive = false;

    private readonly Dictionary<Label, string> originalLabelTexts = new Dictionary<Label, string>();
    private readonly WaitForSeconds tooltipDelay = new WaitForSeconds(4.0f);
    private readonly WaitForSeconds levelTransitionDelay = new WaitForSeconds(2.0f);

    void Start()
    {
        if (levelGenerator == null) levelGenerator = GetComponent<ProceduralLevelGenerator>();
        InitializeUI();
        ShowStartScreen();
    }

    private void InitializeUI()
    {
        var root = mainUIDocument.rootVisualElement;

        startScreen = root.Q<VisualElement>("start-screen");
        mainMenuScreen = root.Q<VisualElement>("main-menu-screen");
        btnTutorial = root.Q<Button>("btn-tutorial");
        btnTraining = root.Q<Button>("btn-training");

        gameScreen = root.Q<VisualElement>("game-screen");
        livesLabel = root.Q<Label>("lives-label");
        progressLabel = root.Q<Label>("progress-label");
        timerLabel = root.Q<Label>("timer-label");
        safeButton = root.Q<Button>("safe-button");
        levelContainer = root.Q<VisualElement>("level-container");

        tooltipOverlay = root.Q<VisualElement>("tooltip-overlay");
        tooltipText = root.Q<Label>("tooltip-text");

        gameOverScreen = root.Q<VisualElement>("game-over-screen");
        restartButton = root.Q<Button>("restart-button");

        Button btnQuit = root.Q<Button>("btn-quit");
        adviceButton = root.Q<Button>("advice-button");
        magnifierButton = root.Q<Button>("magnifier-button");

        scoreLabel = root.Q<Label>("score-label");
        finalScoreLabel = root.Q<Label>("final-score-label");
        newRecordContainer = root.Q<VisualElement>("new-record-container");
        playerNameInput = root.Q<TextField>("player-name-input");
        btnSaveScore = root.Q<Button>("btn-save-score");

        leaderBoardRows.Clear();
        for (int i = 0; i < 5; i++)
        {
            var row = root.Q<Label>($"leader-row-{i}");
            if (row != null) leaderBoardRows.Add(row);
        }

        if (startScreen != null) startScreen.RegisterCallback<ClickEvent>(evt => ShowMainMenu());
        if (btnTutorial != null) btnTutorial.clicked += StartTutorialLevel;
        if (btnTraining != null) btnTraining.clicked += StartProceduralGame;
        if (btnQuit != null) btnQuit.clicked += QuitGame;
        if (safeButton != null) safeButton.clicked += OnSafeButtonClicked;
        if (adviceButton != null) adviceButton.clicked += UseAdvice;
        if (magnifierButton != null) magnifierButton.clicked += ToggleMagnifier;
        if (restartButton != null) restartButton.clicked += ShowMainMenu;
        if (btnSaveScore != null) btnSaveScore.clicked += SaveNewRecord;

        ApplyDynamicGradients(root);
    }

    private void ApplyDynamicGradients(VisualElement root)
    {
        if (root == null) return;

        Texture2D primaryGrad = CreateGradientTexture(new Color(0.0f, 0.32f, 1.0f), new Color(0.0f, 0.79f, 1.0f));
        foreach (var el in root.Query(className: "btn-primary-gradient").ToList())
        {
            el.style.backgroundImage = primaryGrad;
        }

        Texture2D actionGrad = CreateGradientTexture(new Color(0.0f, 0.69f, 0.45f), new Color(0.02f, 0.77f, 0.42f));
        foreach (var el in root.Query(className: "action-button").ToList())
        {
            el.style.backgroundImage = actionGrad;
        }

        Texture2D indigoGlow = CreateGradientTexture(new Color(0.18f, 0.15f, 0.44f, 0.08f), new Color(0.18f, 0.15f, 0.44f, 0.0f));
        foreach (var el in root.Query(className: "bg-glow-indigo").ToList())
        {
            el.style.backgroundImage = indigoGlow;
        }

        Texture2D cyanGlow = CreateGradientTexture(new Color(0.0f, 0.79f, 1.0f, 0.08f), new Color(0.0f, 0.79f, 1.0f, 0.0f));
        foreach (var el in root.Query(className: "bg-glow-cyan").ToList())
        {
            el.style.backgroundImage = cyanGlow;
        }
    }

    private Texture2D CreateGradientTexture(Color startColor, Color endColor)
    {
        Texture2D tex = new Texture2D(1, 32);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 32; y++)
        {
            float t = y / 31f;
            tex.SetPixel(0, y, Color.Lerp(startColor, endColor, t));
        }
        tex.Apply();
        return tex;
    }    private void ShowStartScreen()
    {
        if (startScreen != null) startScreen.RemoveFromClassList("hidden");
        mainMenuScreen.AddToClassList("hidden");
        gameScreen.AddToClassList("hidden");
        gameOverScreen.AddToClassList("hidden");
        tooltipOverlay.AddToClassList("hidden");
        isLevelOver = true;
    }

    private void ShowMainMenu()
    {
        if (startScreen != null) startScreen.AddToClassList("hidden");
        mainMenuScreen.RemoveFromClassList("hidden");
        gameScreen.AddToClassList("hidden");
        gameOverScreen.AddToClassList("hidden");
        tooltipOverlay.AddToClassList("hidden");
        isLevelOver = true;
    }

    private void StartProceduralGame()
    {
        lives = 3;
        adviceUses = 2;
        score = 0;
        isTutorial = false;
        isMagnifierActive = false;

        originalLabelTexts.Clear();
        mainMenuScreen.AddToClassList("hidden");
        gameOverScreen.AddToClassList("hidden");
        gameScreen.RemoveFromClassList("hidden");

        UpdateLivesUI();
        UpdateAdviceButtonUI();
        UpdateMagnifierUI();
        UpdateScoreUI();

        StartNextProceduralLevel();
    }

    private void StartTutorialLevel()
    {
        lives = 3;
        adviceUses = 2;
        score = 0;
        isTutorial = true;
        isLevelOver = false;
        timerActive = false;
        isMagnifierActive = false;

        originalLabelTexts.Clear();
        levelContainer.Clear();
        mainMenuScreen.AddToClassList("hidden");
        gameOverScreen.AddToClassList("hidden");
        gameScreen.RemoveFromClassList("hidden");

        UpdateLivesUI();
        UpdateAdviceButtonUI();
        UpdateMagnifierUI();
        UpdateScoreUI();

        if (timerLabel != null) timerLabel.text = "Время: ∞";
        if (tutorialLevelAsset == null) { Debug.LogError("Tutorial Level Asset is not assigned!"); return; }

        var tutorialUI = tutorialLevelAsset.Instantiate();
        tutorialUI.style.flexGrow = 1;
        levelContainer.Add(tutorialUI);
        SetupTraps(tutorialUI);
        UpdateProgressUI();
    }

    private void StartNextProceduralLevel()
    {
        isLevelOver = false;
        levelContainer.Clear();
        
        if (levelGenerator != null)
        {
            levelGenerator.GenerateLevel(levelContainer, 0);
            SetupTraps(levelContainer);
            UpdateProgressUI();
            
            timeLeft = 30f;
            timerActive = true;
        }
        else
        {
            Debug.LogError("ProceduralLevelGenerator is missing!");
        }
    }

    private void SetupTraps(VisualElement rootElement)
    {
        var traps = rootElement.Query<VisualElement>(className: "phishing-target").ToList();
        totalTrapsInLevel = traps.Count;
        foundTrapsInLevel = 0;

        Debug.Log($"<color=#FF9900>Traps count: {totalTrapsInLevel}</color>");
        foreach (var trap in traps)
        {
            Debug.Log($"<color=#00FFFF>Trap tooltip: {trap.tooltip}</color>");
        }

        rootElement.RegisterCallback<ClickEvent>(OnLevelClicked);

        originalLabelTexts.Clear();
        List<Label> labels = rootElement.Query<Label>().ToList();
        foreach (var label in labels)
        {
            label.RegisterCallback<MouseEnterEvent>(evt => OnLabelEnter(label));
            label.RegisterCallback<MouseLeaveEvent>(evt => OnLabelLeave(label));
        }
    }

    private void OnLevelClicked(ClickEvent evt)
    {
        if (isLevelOver) return;

        VisualElement target = evt.target as VisualElement;
        VisualElement current = target;
        VisualElement trap = null;
        VisualElement levelRoot = evt.currentTarget as VisualElement;

        while (current != null)
        {
            if (current.ClassListContains("phishing-target"))
            {
                trap = current;
                break;
            }
            if (current == levelRoot)
            {
                break;
            }
            current = current.parent;
        }

        if (trap != null)
        {
            if (trap.ClassListContains("phishing-found")) return;

            trap.AddToClassList("phishing-found");
            trap.style.borderBottomWidth = 4;
            trap.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.8f, 0.2f));

            if (trap.ClassListContains("phishing-hide-on-found"))
            {
                if (trap.parent != null) trap.parent.style.display = DisplayStyle.None;
                else trap.style.display = DisplayStyle.None;
            }

            foundTrapsInLevel++;
            UpdateProgressUI();

            score += 100;
            UpdateScoreUI();

            string reason = trap.tooltip;
            if (string.IsNullOrEmpty(reason)) reason = "Фишинговый элемент найден!";

            AudioManager.Instance?.PlaySuccess();
            ShowTooltip(reason);
        }
        else
        {
            score = Mathf.Max(0, score - 50);
            UpdateScoreUI();

            AudioManager.Instance?.PlayError();
            ShowTooltip("Промах! Это обычный элемент.");
            LoseLife();
        }
    }

    private void OnSafeButtonClicked()
    {
        if (isLevelOver) return;

        AudioManager.Instance?.PlayClick();

        if (foundTrapsInLevel == totalTrapsInLevel)
        {
            if (totalTrapsInLevel == 0) score += 250;
            if (!isTutorial) score += Mathf.RoundToInt(timeLeft * 10f);
            UpdateScoreUI();

            AudioManager.Instance?.PlaySuccess();
            LevelPassed();
        }
        else
        {
            score = Mathf.Max(0, score - 50);
            UpdateScoreUI();

            AudioManager.Instance?.PlayError();
            ShowTooltip("На странице еще есть уловки! Ищите внимательнее.");
            LoseLife();
        }
    }

    private void LoseLife()
    {
        lives--;
        UpdateLivesUI();
        if (lives <= 0)
        {
            GameOver();
        }
    }

    private void LevelPassed()
    {
        isLevelOver = true;
        timerActive = false;
        ShowTooltip("Отличная работа! Переходим к следующему сайту...");
        
        if (isTutorial)
        {
            StartCoroutine(ReturnToMenuAfterDelay());
        }
        else
        {
            StartCoroutine(LoadNextLevelAfterDelay());
        }
    }

    private IEnumerator LoadNextLevelAfterDelay()
    {
        yield return levelTransitionDelay;
        StartNextProceduralLevel();
    }

    private IEnumerator ReturnToMenuAfterDelay()
    {
        yield return levelTransitionDelay;
        ShowMainMenu();
    }

    private void GameOver()
    {
        isLevelOver = true;
        timerActive = false;
        AudioManager.Instance?.PlayGameOver();

        if (finalScoreLabel != null) finalScoreLabel.text = $"Ваш результат: {score} очков";

        LoadAndShowLeaderboard();

        if (CheckNewRecord())
        {
            if (newRecordContainer != null) newRecordContainer.RemoveFromClassList("hidden");
            if (playerNameInput != null) playerNameInput.value = "";
        }
        else
        {
            if (newRecordContainer != null) newRecordContainer.AddToClassList("hidden");
        }

        if (gameOverScreen != null) gameOverScreen.RemoveFromClassList("hidden");
    }

    private void Update()
    {
        if (timerActive && !isLevelOver && !isTutorial)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                timeLeft = 0;
                timerActive = false;
                AudioManager.Instance?.PlayError();
                ShowTooltip("Время вышло! Мошенники успели украсть данные.");
                LoseLife();
                if (lives > 0)
                {
                    isLevelOver = true;
                    StartCoroutine(LoadNextLevelAfterDelay());
                }
            }
            UpdateTimerUI();
        }
    }
    
    private void UpdateTimerUI()
    {
        if (timerLabel != null)
        {
            timerLabel.text = $"Время: {Mathf.CeilToInt(timeLeft)}";
            if (timeLeft <= 5f)
                timerLabel.style.color = new StyleColor(Color.red);
            else
                timerLabel.style.color = new StyleColor(Color.white);
        }
    }

    private void UpdateLivesUI()
    {
        livesLabel.text = $"Жизни: {string.Concat(Enumerable.Repeat("❤️", lives))}";
    }

    private void UpdateProgressUI()
    {
        progressLabel.text = $"Найдено уловок: {foundTrapsInLevel}";
    }

    private Coroutine tooltipCoroutine;

    private void ShowTooltip(string text)
    {
        var title = tooltipOverlay.Q<Label>("tooltip-title");
        if (title != null)
        {
            title.text = text.Contains("Промах") || text.Contains("еще есть уловки") ? "Внимание!" : "Фишинг найден!";
            title.style.color = text.Contains("Промах") || text.Contains("еще есть уловки") ? new StyleColor(new Color(1f, 0.3f, 0.3f)) : new StyleColor(Color.white);
        }

        tooltipText.text = text;
        tooltipOverlay.RemoveFromClassList("hidden");
        
        if (tooltipCoroutine != null) StopCoroutine(tooltipCoroutine);
        tooltipCoroutine = StartCoroutine(HideTooltipAfterDelay());
    }

    private IEnumerator HideTooltipAfterDelay()
    {
        yield return tooltipDelay;
        tooltipOverlay.AddToClassList("hidden");
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void UseAdvice()
    {
        if (isLevelOver || adviceUses <= 0) return;

        var traps = levelContainer.Query<VisualElement>(className: "phishing-target").ToList();
        var activeTraps = traps.Where(t => !t.ClassListContains("phishing-found")).ToList();

        adviceUses--;
        UpdateAdviceButtonUI();

        if (activeTraps.Count > 0)
        {
            VisualElement randomTrap = activeTraps[Random.Range(0, activeTraps.Count)];
            StartCoroutine(HighlightTrapRoutine(randomTrap));
        }
        else
        {
            ShowTooltip("Старший аналитик подтверждает: на этой странице всё чисто!");
        }
    }

    private IEnumerator HighlightTrapRoutine(VisualElement trap)
    {
        trap.style.borderTopWidth = 3;
        trap.style.borderBottomWidth = 3;
        trap.style.borderLeftWidth = 3;
        trap.style.borderRightWidth = 3;

        Color orangeColor = new Color(1.0f, 0.6f, 0.0f);
        trap.style.borderTopColor = new StyleColor(orangeColor);
        trap.style.borderBottomColor = new StyleColor(orangeColor);
        trap.style.borderLeftColor = new StyleColor(orangeColor);
        trap.style.borderRightColor = new StyleColor(orangeColor);

        yield return new WaitForSeconds(3.0f);

        trap.style.borderTopWidth = StyleKeyword.Null;
        trap.style.borderBottomWidth = StyleKeyword.Null;
        trap.style.borderLeftWidth = StyleKeyword.Null;
        trap.style.borderRightWidth = StyleKeyword.Null;

        trap.style.borderTopColor = StyleKeyword.Null;
        trap.style.borderBottomColor = StyleKeyword.Null;
        trap.style.borderLeftColor = StyleKeyword.Null;
        trap.style.borderRightColor = StyleKeyword.Null;
    }

    private void UpdateAdviceButtonUI()
    {
        if (adviceButton == null) return;

        adviceButton.text = $"СОВЕТ АНАЛИТИКА ({adviceUses})";
        if (adviceUses <= 0)
        {
            adviceButton.SetEnabled(false);
        }
        else
        {
            adviceButton.SetEnabled(true);
        }
    }

    private void ToggleMagnifier()
    {
        if (isLevelOver) return;
        isMagnifierActive = !isMagnifierActive;
        UpdateMagnifierUI();
    }

    private void UpdateMagnifierUI()
    {
        if (magnifierButton == null) return;

        magnifierButton.text = isMagnifierActive ? "ЛУПА: ВКЛ" : "ЛУПА: ВЫКЛ";
        magnifierButton.style.backgroundColor = isMagnifierActive ? new StyleColor(new Color(0.0f, 0.32f, 1.0f, 0.15f)) : StyleKeyword.Null;
    }

    private void OnLabelEnter(Label label)
    {
        if (!isMagnifierActive || isLevelOver) return;
        if (!originalLabelTexts.ContainsKey(label)) originalLabelTexts[label] = label.text;

        label.style.scale = new Scale(new Vector2(1.25f, 1.3f));
        label.text = HighlightHomoglyphs(originalLabelTexts[label]);
    }

    private void OnLabelLeave(Label label)
    {
        label.style.scale = StyleKeyword.Null;
        if (originalLabelTexts.TryGetValue(label, out string originalText)) label.text = originalText;
    }

    private string HighlightHomoglyphs(string originalText)
    {
        if (string.IsNullOrEmpty(originalText)) return originalText;

        bool hasCyrillic = false;
        bool hasLatin = false;

        foreach (char c in originalText)
        {
            if ((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё') hasCyrillic = true;
            else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) hasLatin = true;
        }

        if (!hasCyrillic || !hasLatin) return originalText;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in originalText)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) sb.Append($"<color=#FF3B30>{c}</color>");
            else sb.Append(c);
        }

        return sb.ToString();
    }

    private void UpdateScoreUI()
    {
        if (scoreLabel != null) scoreLabel.text = $"Очки: {score}";
    }

    private void LoadAndShowLeaderboard()
    {
        for (int i = 0; i < 5; i++)
        {
            string name = PlayerPrefs.GetString(ScoreNameKey + i, "Пусто");
            int val = PlayerPrefs.GetInt(ScoreValueKey + i, 0);
            if (i < leaderBoardRows.Count && leaderBoardRows[i] != null)
            {
                leaderBoardRows[i].text = $"{i + 1}. {name} — {val}";
            }
        }
    }

    private bool CheckNewRecord()
    {
        if (score <= 0) return false;
        int lowestScore = PlayerPrefs.GetInt(ScoreValueKey + 4, 0);
        return score > lowestScore;
    }

    private void SaveNewRecord()
    {
        if (!CheckNewRecord()) return;

        string newName = playerNameInput != null ? playerNameInput.value : "СТАЖЕР";
        if (string.IsNullOrEmpty(newName)) newName = "СТАЖЕР";

        List<string> names = new List<string>();
        List<int> values = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            names.Add(PlayerPrefs.GetString(ScoreNameKey + i, "Пусто"));
            values.Add(PlayerPrefs.GetInt(ScoreValueKey + i, 0));
        }

        int insertIndex = -1;
        for (int i = 0; i < 5; i++)
        {
            if (score > values[i])
            {
                insertIndex = i;
                break;
            }
        }

        if (insertIndex != -1)
        {
            names.Insert(insertIndex, newName);
            values.Insert(insertIndex, score);

            for (int i = 0; i < 5; i++)
            {
                PlayerPrefs.SetString(ScoreNameKey + i, names[i]);
                PlayerPrefs.SetInt(ScoreValueKey + i, values[i]);
            }
            PlayerPrefs.Save();
        }

        if (newRecordContainer != null) newRecordContainer.AddToClassList("hidden");
        LoadAndShowLeaderboard();
    }
}
