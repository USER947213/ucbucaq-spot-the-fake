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

    private int lives = 3;
    
    // UI Elements
    private VisualElement startScreen;
    private VisualElement mainMenuScreen;
    private Button btnTutorial;
    private Button btnTraining;

    private VisualElement gameScreen;
    private Label livesLabel;
    private Label progressLabel;
    private Button safeButton;
    private VisualElement levelContainer;
    
    private VisualElement tooltipOverlay;
    private Label tooltipText;

    private VisualElement gameOverScreen;
    private Button restartButton;

    // Level state
    private int totalTrapsInLevel = 0;
    private int foundTrapsInLevel = 0;
    private bool isLevelOver = false;
    private bool isTutorial = false;
    
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

        // Menu Elements
        startScreen = root.Q<VisualElement>("start-screen");
        mainMenuScreen = root.Q<VisualElement>("main-menu-screen");
        btnTutorial = root.Q<Button>("btn-tutorial");
        btnTraining = root.Q<Button>("btn-training");

        // Game Elements
        gameScreen = root.Q<VisualElement>("game-screen");
        livesLabel = root.Q<Label>("lives-label");
        progressLabel = root.Q<Label>("progress-label");
        safeButton = root.Q<Button>("safe-button");
        levelContainer = root.Q<VisualElement>("level-container");
        
        tooltipOverlay = root.Q<VisualElement>("tooltip-overlay");
        tooltipText = root.Q<Label>("tooltip-text");

        gameOverScreen = root.Q<VisualElement>("game-over-screen");
        restartButton = root.Q<Button>("restart-button");

        // Bindings
        if (startScreen != null) startScreen.RegisterCallback<ClickEvent>(evt => ShowMainMenu());
        btnTutorial.clicked += StartTutorialLevel;
        btnTraining.clicked += StartProceduralGame;
        safeButton.clicked += OnSafeButtonClicked;
        restartButton.clicked += ShowMainMenu;
    }

    private void ShowStartScreen()
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
        isTutorial = false;
        mainMenuScreen.AddToClassList("hidden");
        gameOverScreen.AddToClassList("hidden");
        gameScreen.RemoveFromClassList("hidden");
        UpdateLivesUI();
        
        StartNextProceduralLevel();
    }

    private void StartTutorialLevel()
    {
        lives = 3;
        isTutorial = true;
        isLevelOver = false;
        mainMenuScreen.AddToClassList("hidden");
        gameOverScreen.AddToClassList("hidden");
        gameScreen.RemoveFromClassList("hidden");
        levelContainer.Clear();
        UpdateLivesUI();
        
        if (tutorialLevelAsset != null)
        {
            var tutorialUI = tutorialLevelAsset.Instantiate();
            tutorialUI.style.flexGrow = 1;
            levelContainer.Add(tutorialUI);
            SetupTraps(tutorialUI);
            UpdateProgressUI();
        }
        else
        {
            Debug.LogError("Tutorial Level Asset is not assigned!");
        }
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

        foreach (var trap in traps)
        {
            trap.RegisterCallback<ClickEvent>(evt => OnTrapClicked(evt, trap));
        }

        // Add a general click listener to the whole webpage for miss-clicks
        rootElement.RegisterCallback<ClickEvent>(OnMissClick);
    }

    private void OnTrapClicked(ClickEvent evt, VisualElement trap)
    {
        if (isLevelOver) return;
        evt.StopPropagation();

        if (trap.ClassListContains("phishing-found")) return;

        trap.AddToClassList("phishing-found");
        trap.style.borderBottomWidth = 4;
        trap.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.8f, 0.2f)); // Green highlight

        foundTrapsInLevel++;
        UpdateProgressUI();

        string reason = trap.tooltip;
        if (string.IsNullOrEmpty(reason)) reason = "Фишинговый элемент найден!";
        
        ShowTooltip(reason);

        // We DO NOT auto-pass the level here anymore!
        // The user must click "Safe" to confirm they think there are no more traps.
        // Exception: For tutorial, we might want to auto-pass or keep the same logic. Let's keep the same logic.
    }

    private void OnMissClick(ClickEvent evt)
    {
        if (isLevelOver) return;
        
        var target = evt.target as VisualElement;
        // Don't punish for clicking the safe button or other UI outside the level
        if (target != null && !target.ClassListContains("phishing-target"))
        {
            ShowTooltip("Промах! Это обычный элемент.");
            LoseLife();
        }
    }

    private void OnSafeButtonClicked()
    {
        if (isLevelOver) return;

        if (foundTrapsInLevel == totalTrapsInLevel)
        {
            LevelPassed();
        }
        else
        {
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
        gameOverScreen.RemoveFromClassList("hidden");
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
}
