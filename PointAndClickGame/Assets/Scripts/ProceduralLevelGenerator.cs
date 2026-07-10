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
                replacementText = "ВKонтакте", // English K
                tooltipMessage = "Опечатка в логотипе ВКонтакте! Мошенники часто меняют буквы на похожие из другого алфавита."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "bank-logo",
                anomalyType = AnomalyType.ChangeText,
                replacementText = "СБЕР БAHK", // English H
                tooltipMessage = "Опечатка в логотипе банка! Буква 'Н' заменена на английскую 'H'."
            });
            possibleAnomalies.Add(new AnomalyDefinition {
                targetElementName = "cvv-container",
                anomalyType = AnomalyType.RemoveClass, // Makes it visible if it was hidden by class, or we just handle it specially
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
                elementToInject = urgentTimerAsset, // We'll assume this gets assigned in SetupGameScene
                tooltipMessage = "Внедрен фейковый таймер." // This won't be used directly since the injected UXML has its own tooltip
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
        // Special hardcoded case for CVV (unhiding it)
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
            return; // We don't mark the container itself as a target
        }

        // Standard traps - mark them
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
                var overlay = new VisualElement();
                overlay.style.position = Position.Absolute;
                overlay.style.left = 0;
                overlay.style.top = 0;
                overlay.style.right = 0;
                overlay.style.bottom = 0;
                overlay.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.01f)); // Almost invisible
                overlay.AddToClassList("phishing-target");
                overlay.tooltip = trap.tooltipMessage;
                element.Add(overlay);
                break;
            case AnomalyType.SpawnFakeChat:
                var chat = new VisualElement();
                chat.style.position = Position.Absolute;
                chat.style.bottom = 20;
                chat.style.right = 20;
                chat.style.width = 250;
                chat.style.backgroundColor = Color.white;
                chat.style.borderTopLeftRadius = 12;
                chat.style.borderTopRightRadius = 12;
                chat.style.borderTopWidth = 2;
                chat.style.borderBottomWidth = 2;
                chat.style.borderLeftWidth = 2;
                chat.style.borderRightWidth = 2;
                chat.style.borderTopColor = new StyleColor(Color.black);
                chat.style.borderBottomColor = new StyleColor(Color.black);
                chat.style.borderLeftColor = new StyleColor(Color.black);
                chat.style.borderRightColor = new StyleColor(Color.black);
                
                var chatTitle = new Label("Поддержка 24/7");
                chatTitle.style.backgroundColor = new StyleColor(Color.blue);
                chatTitle.style.color = Color.white;
                chatTitle.style.paddingBottom = 10;
                chatTitle.style.paddingTop = 10;
                chatTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
                chat.Add(chatTitle);

                var chatMsg = new Label("Ваша карта заблокирована. Введите CVV для разблокировки:");
                chatMsg.style.whiteSpace = WhiteSpace.Normal;
                chatMsg.style.paddingTop = 10;
                chatMsg.style.paddingBottom = 10;
                chatMsg.style.paddingLeft = 5;
                chatMsg.style.paddingRight = 5;
                chatMsg.style.color = Color.black;
                chat.Add(chatMsg);

                var chatInput = new TextField();
                chatInput.style.marginBottom = 10;
                chat.Add(chatInput);

                chat.AddToClassList("phishing-target");
                chat.tooltip = trap.tooltipMessage;
                element.Add(chat);
                break;
            case AnomalyType.SpawnSystemModal:
                var modal = new VisualElement();
                modal.style.position = Position.Absolute;
                modal.style.top = 50;
                modal.style.left = 150;
                modal.style.width = 400;
                modal.style.backgroundColor = new StyleColor(new Color(0.9f, 0.9f, 0.9f, 1f));
                modal.style.borderTopWidth = 1;
                modal.style.borderBottomWidth = 1;
                modal.style.borderLeftWidth = 1;
                modal.style.borderRightWidth = 1;
                modal.style.borderTopColor = Color.gray;
                modal.style.borderBottomColor = Color.gray;
                modal.style.borderLeftColor = Color.gray;
                modal.style.borderRightColor = Color.gray;
                
                var modalTitle = new Label("Windows Security Alert");
                modalTitle.style.backgroundColor = Color.red;
                modalTitle.style.color = Color.white;
                modalTitle.style.paddingBottom = 5;
                modalTitle.style.paddingTop = 5;
                modalTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
                modal.Add(modalTitle);

                var modalMsg = new Label("ОБНАРУЖЕН ВИРУС! Нажмите ОК, чтобы очистить ПК.");
                modalMsg.style.whiteSpace = WhiteSpace.Normal;
                modalMsg.style.paddingTop = 20;
                modalMsg.style.paddingBottom = 20;
                modalMsg.style.paddingLeft = 10;
                modalMsg.style.color = Color.black;
                modal.Add(modalMsg);

                var btn = new Button { text = "OK" };
                btn.style.width = 100;
                btn.style.alignSelf = Align.Center;
                btn.style.marginBottom = 10;
                modal.Add(btn);

                modal.AddToClassList("phishing-target");
                modal.tooltip = trap.tooltipMessage;
                element.Add(modal);
                break;
        }
    }
}
