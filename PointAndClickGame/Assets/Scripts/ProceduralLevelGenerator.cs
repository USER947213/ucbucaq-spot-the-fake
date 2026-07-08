using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Tooltip("UXML шаблон чистого сайта")]
    public VisualTreeAsset cleanSiteTemplate;

    [Tooltip("Список возможных уловок для внедрения")]
    public List<AnomalyDefinition> possibleAnomalies = new List<AnomalyDefinition>();

    private void Awake()
    {
        // For MVP, if anomalies list is empty, we populate it with defaults
        if (possibleAnomalies.Count == 0)
        {
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "url-text",
                anomalyType = AnomalyType.ChangeText,
                replacementText = "https://sberbank-login-secure.xyz/login",
                tooltipMessage = "Официальный адрес банка короткий. Этот адрес длинный, странный и имеет домен .xyz."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "lock-icon",
                anomalyType = AnomalyType.ChangeText,
                replacementText = "🔓", // fake open lock instead of closed
                tooltipMessage = "Замок открыт или выглядит подозрительно! У безопасного сайта он закрыт."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "bank-logo",
                anomalyType = AnomalyType.ChangeText,
                replacementText = "СБЕР БАHK Онлайн", // replacing Н with English H, barely noticeable
                tooltipMessage = "Опечатка в логотипе! Мошенники часто меняют одну букву, надеясь на невнимательность."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "cvv-container",
                anomalyType = AnomalyType.RemoveClass, // Wait, we can use an anomaly to show the hidden field
                cssClassModifier = "", 
                tooltipMessage = "Ни один банк никогда не просит CVV-код при входе в личный кабинет! Это обман."
            });
        }
    }

    /// <summary>
    /// Generates a level by instantiating the clean template and injecting traps.
    /// Returns the number of traps injected.
    /// </summary>
    public int GenerateLevel(VisualElement container, int currentLevel)
    {
        container.Clear();
        
        if (cleanSiteTemplate == null)
        {
            Debug.LogError("Clean Site Template is not assigned to ProceduralLevelGenerator!");
            return 0;
        }

        // 1. Load clean template
        var ui = cleanSiteTemplate.Instantiate();
        ui.style.flexGrow = 1;
        container.Add(ui);

        // 2. Determine how many traps to inject based on level
        // Randomly 0 to 3 traps
        int trapsCount = Random.Range(0, 4); 
        
        // 3. Pick random traps
        List<AnomalyDefinition> selectedTraps = new List<AnomalyDefinition>();
        List<AnomalyDefinition> availableTraps = new List<AnomalyDefinition>(possibleAnomalies);
        
        for (int i = 0; i < trapsCount && availableTraps.Count > 0; i++)
        {
            int index = Random.Range(0, availableTraps.Count);
            selectedTraps.Add(availableTraps[index]);
            availableTraps.RemoveAt(index);
        }

        // 4. Apply traps to DOM
        int appliedTraps = 0;
        foreach (var trap in selectedTraps)
        {
            VisualElement targetElement = null;
            
            if (!string.IsNullOrEmpty(trap.targetElementName))
            {
                targetElement = ui.Q<VisualElement>(trap.targetElementName);
            }
            else if (!string.IsNullOrEmpty(trap.targetElementClass))
            {
                targetElement = ui.Q<VisualElement>(className: trap.targetElementClass);
            }

            if (targetElement != null)
            {
                ApplyTrap(targetElement, trap);
                appliedTraps++;
            }
            else
            {
                Debug.LogWarning($"Could not find target for trap: {trap.targetElementName} / {trap.targetElementClass}");
            }
        }

        return appliedTraps;
    }

    private void ApplyTrap(VisualElement element, AnomalyDefinition trap)
    {
        // Special case for CVV trap (we need to make it visible and mark the input as phishing-target)
        if (trap.targetElementName == "cvv-container")
        {
            element.style.display = DisplayStyle.Flex; // Show it
            var inputField = element.Q<VisualElement>("cvv-trap");
            if (inputField != null)
            {
                inputField.AddToClassList("phishing-target");
                inputField.tooltip = trap.tooltipMessage;
            }
            return;
        }

        // Standard traps
        element.AddToClassList("phishing-target");
        element.tooltip = trap.tooltipMessage;

        switch (trap.anomalyType)
        {
            case AnomalyType.ChangeText:
                if (element is Label label)
                {
                    label.text = trap.replacementText;
                }
                break;
            case AnomalyType.HideElement:
                element.style.display = DisplayStyle.None;
                break;
            case AnomalyType.AddClass:
                element.AddToClassList(trap.cssClassModifier);
                break;
            case AnomalyType.RemoveClass:
                element.RemoveFromClassList(trap.cssClassModifier);
                break;
        }
    }
}
