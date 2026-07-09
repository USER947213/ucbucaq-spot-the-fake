using UnityEngine;

public enum AnomalyType
{
    ChangeText,
    HideElement,
    AddClass,
    RemoveClass,
    InjectElement, // Вставляет новый UI-блок в указанный контейнер
    HoverSpoof     // Добавляет текст/картинку, которая меняется при наведении
}

[System.Serializable]
public class AnomalyDefinition
{
    [Tooltip("Название (name) элемента в UXML (целевой контейнер или элемент)")]
    public string targetElementName;

    [Tooltip("Класс (class) элемента в UXML (если name не задан)")]
    public string targetElementClass;

    [Tooltip("Что именно сделать с элементом")]
    public AnomalyType anomalyType;

    [Tooltip("Новый текст (используется для ChangeText)")]
    public string replacementText;

    [Tooltip("CSS класс для добавления/удаления (используется для AddClass/RemoveClass)")]
    public string cssClassModifier;

    [Tooltip("UXML для InjectElement. Этот блок будет добавлен внутрь цели.")]
    public UnityEngine.UIElements.VisualTreeAsset elementToInject;

    [Tooltip("Подсказка игроку (почему это фишинг)")]
    public string tooltipMessage;
}
