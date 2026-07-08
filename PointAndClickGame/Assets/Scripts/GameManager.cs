using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public UIDocument mainUIDocument;
    public VisualTreeAsset tutorialLevelAsset; 

    private int score = 0;
    private int lives = 3;
    
    // UI Elements
    private Label scoreLabel;
    private Label livesLabel;
    private Label progressLabel;
    private Button safeButton;
    private VisualElement levelContainer;
    
    private VisualElement tooltipOverlay;
    private Label tooltipText;

    private VisualElement gameOverScreen;
    private Label finalScoreLabel;
    private Button restartButton;

    // Level state
    private int totalTrapsInLevel = 0;
    private int foundTrapsInLevel = 0;
    private bool isLevelOver = false;

    void Start()
    {
        InitializeUI();
        StartTutorialLevel();
    }

    private void InitializeUI()
    {
        var root = mainUIDocument.rootVisualElement;

        scoreLabel = root.Q<Label>("score-label");
        livesLabel = root.Q<Label>("lives-label");
        progressLabel = root.Q<Label>("progress-label");
        safeButton = root.Q<Button>("safe-button");
        levelContainer = root.Q<VisualElement>("level-container");
        
        tooltipOverlay = root.Q<VisualElement>("tooltip-overlay");
        tooltipText = root.Q<Label>("tooltip-text");

        gameOverScreen = root.Q<VisualElement>("game-over-screen");
        finalScoreLabel = root.Q<Label>("final-score-label");
        restartButton = root.Q<Button>("restart-button");

        safeButton.clicked += OnSafeButtonClicked;
        restartButton.clicked += RestartGame;

        UpdateScoreAndLives();
    }

    private void StartTutorialLevel()
    {
        isLevelOver = false;
        levelContainer.Clear();
        
        if (tutorialLevelAsset != null)
        {
            var tutorialUI = tutorialLevelAsset.Instantiate();
            tutorialUI.style.flexGrow = 1;
            levelContainer.Add(tutorialUI);
            
            SetupTraps(tutorialUI);
        }
        else
        {
            Debug.LogError("Tutorial Level Asset is not assigned!");
        }
    }

    private void SetupTraps(VisualElement rootElement)
    {
        // Find all elements marked as phishing targets
        var traps = rootElement.Query<VisualElement>(className: "phishing-target").ToList();
        totalTrapsInLevel = traps.Count;
        foundTrapsInLevel = 0;
        UpdateProgress();

        foreach (var trap in traps)
        {
            // Add click event
            trap.RegisterCallback<ClickEvent>(evt => OnTrapClicked(evt, trap));
        }

        // Add a general click listener to the whole webpage for miss-clicks
        var webpageContent = rootElement.Q<VisualElement>(className: "webpage-content");
        if(webpageContent != null)
        {
            webpageContent.RegisterCallback<ClickEvent>(OnMissClick);
        }
    }

    private void OnTrapClicked(ClickEvent evt, VisualElement trap)
    {
        if (isLevelOver) return;
        evt.StopPropagation(); // Prevent miss-click

        if (trap.ClassListContains("phishing-found")) return;

        trap.AddToClassList("phishing-found");
        foundTrapsInLevel++;
        score += 100;
        
        UpdateScoreAndLives();
        UpdateProgress();

        string reason = trap.tooltip;
        if (string.IsNullOrEmpty(reason)) reason = "Фишинговый элемент найден!";
        
        ShowTooltip(reason);

        if (foundTrapsInLevel >= totalTrapsInLevel)
        {
            LevelPassed();
        }
    }

    private void OnMissClick(ClickEvent evt)
    {
        if (isLevelOver) return;
        
        var target = evt.target as VisualElement;
        if (target != null && !target.ClassListContains("phishing-target"))
        {
            // Visual feedback for miss
            target.AddToClassList("miss-click");
            ShowTooltip("Промах! Это обычный элемент.");
            LoseLife();
        }
    }

    private void OnSafeButtonClicked()
    {
        if (isLevelOver) return;

        if (totalTrapsInLevel == 0)
        {
            score += 200; // Bonus for correctly identifying safe page
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
        UpdateScoreAndLives();
        if (lives <= 0)
        {
            GameOver();
        }
    }

    private void LevelPassed()
    {
        isLevelOver = true;
        ShowTooltip("Уровень пройден! Отличная работа.");
    }

    private void GameOver()
    {
        isLevelOver = true;
        gameOverScreen.RemoveFromClassList("hidden");
        finalScoreLabel.text = $"Итоговый счет: {score}";
    }

    private void RestartGame()
    {
        score = 0;
        lives = 3;
        gameOverScreen.AddToClassList("hidden");
        UpdateScoreAndLives();
        StartTutorialLevel();
    }

    private void UpdateScoreAndLives()
    {
        scoreLabel.text = $"Очки: {score}";
        
        string livesStr = "";
        for(int i = 0; i < lives; i++) livesStr += "❤️";
        livesLabel.text = $"Жизни: {livesStr}";
    }

    private void UpdateProgress()
    {
        progressLabel.text = $"Найдено уловок: {foundTrapsInLevel} / {totalTrapsInLevel}";
    }

    private Coroutine tooltipCoroutine;

    private void ShowTooltip(string text)
    {
        tooltipText.text = text;
        tooltipOverlay.RemoveFromClassList("hidden");
        
        if (tooltipCoroutine != null) StopCoroutine(tooltipCoroutine);
        tooltipCoroutine = StartCoroutine(HideTooltipAfterDelay(3.0f));
    }

    private IEnumerator HideTooltipAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        tooltipOverlay.AddToClassList("hidden");
    }
}
