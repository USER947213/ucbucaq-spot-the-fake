using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Tooltip("Модули хедеров")]
    public List<VisualTreeAsset> headerModules = new List<VisualTreeAsset>();
    
    [Tooltip("Модули контента (тела)")]
    public List<VisualTreeAsset> bodyModules = new List<VisualTreeAsset>();
    
    [Tooltip("Модули футеров")]
    public List<VisualTreeAsset> footerModules = new List<VisualTreeAsset>();

    [Tooltip("Список возможных уловок")]
    public List<AnomalyDefinition> possibleAnomalies = new List<AnomalyDefinition>();

    [Tooltip("UXML для инжекта таймера (подгружается из кода, если список пуст)")]
    public VisualTreeAsset urgentTimerAsset;

    [Tooltip("UXML для инжекта фальшивого чата")]
    public VisualTreeAsset fakeChatAsset;

    [Tooltip("UXML для инжекта системного окна")]
    public VisualTreeAsset systemModalAsset;

    private void Awake()
    {
        // Populate default genius anomalies if empty
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
                replacementText = "🔓",
                tooltipMessage = "Замок открыт или выглядит подозрительно! У безопасного сайта он закрыт."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "social-logo",
                anomalyType = AnomalyType.ChangeText,
                replacementText = "ВKонтакте",
                tooltipMessage = "Опечатка в логотипе ВКонтакте! Мошенники часто меняют буквы на похожие из другого алфавита."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "bank-logo",
                anomalyType = AnomalyType.ChangeText,
                replacementText = "СБЕР БAHK",
                tooltipMessage = "Опечатка в логотипе банка! Буква 'Н' заменена на английскую 'H'."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "cvv-container",
                anomalyType = AnomalyType.RemoveClass,
                tooltipMessage = "Ни один банк никогда не просит ПИН-код или CVV при входе! Это фишинг."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "copyright-text",
                anomalyType = AnomalyType.ChangeText,
                replacementText = "© 2018 Все права защищены.",
                tooltipMessage = "Устаревший год в копирайте. Мошенники часто копируют старые версии сайтов и забывают обновлять футер."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "security-badge",
                anomalyType = AnomalyType.ChangeText,
                replacementText = "✅ Secured by MacAfee",
                tooltipMessage = "Опечатка в названии известного антивируса (McAfee -> MacAfee). Фейковый бейдж."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "article-ad-container",
                anomalyType = AnomalyType.InjectElement,
                elementToInject = urgentTimerAsset,
                tooltipMessage = "Внедрен фейковый таймер."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "download-button",
                anomalyType = AnomalyType.HoverSpoof,
                replacementText = "Скачать Вирус.exe",
                tooltipMessage = "Кнопка меняет свое назначение или текст при наведении. Это попытка обмануть вас перед кликом."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "buy-btn",
                anomalyType = AnomalyType.ClickjackingOverlay,
                tooltipMessage = "Невидимый слой поверх кнопки (Clickjacking)! Мошенники перехватывают клик."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "pay-btn",
                anomalyType = AnomalyType.ClickjackingOverlay,
                tooltipMessage = "Невидимый слой (Clickjacking)! Ваши деньги ушли бы злоумышленнику."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "download-link",
                anomalyType = AnomalyType.HoverSpoof,
                replacementText = "http://malware-server.xyz/stealer.exe",
                tooltipMessage = "При наведении ссылка меняется на вредоносную!"
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "level-container",
                anomalyType = AnomalyType.SpawnFakeChat,
                tooltipMessage = "Сайт открыл фальшивый чат поддержки, чтобы выведать ваши данные."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "level-container",
                anomalyType = AnomalyType.SpawnSystemModal,
                tooltipMessage = "Сайт показывает системное окно браузера, чтобы запугать вас вирусами!"
            });
        }
    }

    public int GenerateLevel(VisualElement container, int currentLevel)
    {
        container.Clear();

        // 1. Pick random modules
        if (headerModules.Count > 0)
        {
            var h = headerModules[Random.Range(0, headerModules.Count)].Instantiate();
            container.Add(h);
        }
        if (bodyModules.Count > 0)
        {
            var b = bodyModules[Random.Range(0, bodyModules.Count)].Instantiate();
            b.style.flexGrow = 1;
            container.Add(b);
        }
        if (footerModules.Count > 0)
        {
            var f = footerModules[Random.Range(0, footerModules.Count)].Instantiate();
            container.Add(f);
        }

        // 2. Find all applicable anomalies (targets exist in the generated DOM)
        List<AnomalyDefinition> applicableAnomalies = new List<AnomalyDefinition>();
        foreach (var anomaly in possibleAnomalies)
        {
            VisualElement target = null;
            if (!string.IsNullOrEmpty(anomaly.targetElementName))
                target = container.Q<VisualElement>(anomaly.targetElementName);
            else if (!string.IsNullOrEmpty(anomaly.targetElementClass))
                target = container.Q<VisualElement>(className: anomaly.targetElementClass);

            if (target != null)
            {
                applicableAnomalies.Add(anomaly);
            }
        }

        // 3. Determine how many traps to inject (0 to 10)
        // Max traps bounded by how many applicable we found
        int maxTraps = Mathf.Min(10, applicableAnomalies.Count);
        int trapsCount = Random.Range(0, maxTraps + 1); 

        // 4. Randomly pick trapsCount anomalies
        var selectedTraps = applicableAnomalies.OrderBy(_ => Random.value).Take(trapsCount).ToList();

        // 5. Apply traps
        int appliedTraps = 0;
        foreach (var trap in selectedTraps)
        {
            VisualElement targetElement = null;
            if (!string.IsNullOrEmpty(trap.targetElementName))
                targetElement = container.Q<VisualElement>(trap.targetElementName);
            else if (!string.IsNullOrEmpty(trap.targetElementClass))
                targetElement = container.Q<VisualElement>(className: trap.targetElementClass);

            if (targetElement != null)
            {
                ApplyTrap(targetElement, trap);
                appliedTraps++;
            }
        }

        return appliedTraps; // For injects, the actual count might differ if the injected block has multiple targets, but we'll recalculate in GameManager anyway.
    }

    private void ApplyTrap(VisualElement element, AnomalyDefinition trap)
    {
        if (trap.anomalyType == AnomalyType.SpawnFakeChat)
        {
            if (fakeChatAsset != null)
            {
                var instance = fakeChatAsset.Instantiate();
                instance.style.position = Position.Absolute;
                instance.style.left = 0;
                instance.style.top = 0;
                instance.style.right = 0;
                instance.style.bottom = 0;
                instance.pickingMode = PickingMode.Ignore;

                var target = instance.Children().FirstOrDefault() ?? instance;
                target.AddToClassList("phishing-target");
                target.AddToClassList("phishing-hide-on-found");
                target.tooltip = trap.tooltipMessage;
                target.pickingMode = PickingMode.Position;
                element.Add(instance);
            }
            return;
        }

        if (trap.anomalyType == AnomalyType.SpawnSystemModal)
        {
            if (systemModalAsset != null)
            {
                var instance = systemModalAsset.Instantiate();
                instance.style.position = Position.Absolute;
                instance.style.left = 0;
                instance.style.top = 0;
                instance.style.right = 0;
                instance.style.bottom = 0;
                instance.pickingMode = PickingMode.Ignore;

                var target = instance.Children().FirstOrDefault() ?? instance;
                target.AddToClassList("phishing-target");
                target.AddToClassList("phishing-hide-on-found");
                target.tooltip = trap.tooltipMessage;
                target.pickingMode = PickingMode.Position;
                element.Add(instance);
            }
            return;
        }

        if (trap.targetElementName == "cvv-container")
        {
            element.style.display = DisplayStyle.Flex;
            var inputField = element.Q<VisualElement>("cvv-trap");
            if (inputField != null)
            {
                inputField.AddToClassList("phishing-target");
                inputField.tooltip = trap.tooltipMessage;
            }
            return;
        }

        if (trap.anomalyType == AnomalyType.InjectElement)
        {
            if (trap.elementToInject != null)
            {
                var injected = trap.elementToInject.Instantiate();
                element.Add(injected);
            }
            return;
        }

        element.AddToClassList("phishing-target");
        element.tooltip = trap.tooltipMessage;

        switch (trap.anomalyType)
        {
            case AnomalyType.ChangeText:
                if (element is Label label)
                    label.text = trap.replacementText;
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
            case AnomalyType.HoverSpoof:
                if (element is Label spoofLabel)
                {
                    string originalText = spoofLabel.text;
                    element.RegisterCallback<MouseEnterEvent>(e => spoofLabel.text = trap.replacementText);
                    element.RegisterCallback<MouseLeaveEvent>(e => spoofLabel.text = originalText);
                }
                else if (element is Button spoofBtn)
                {
                    string originalText = spoofBtn.text;
                    element.RegisterCallback<MouseEnterEvent>(e => spoofBtn.text = trap.replacementText);
                    element.RegisterCallback<MouseLeaveEvent>(e => spoofBtn.text = originalText);
                }
                break;
            case AnomalyType.ClickjackingOverlay:
                break;
        }
    }
}
