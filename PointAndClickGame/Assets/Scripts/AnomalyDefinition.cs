using UnityEngine;

public enum AnomalyType
{
    ChangeText,
    HideElement,
    AddClass,
    RemoveClass
}

[System.Serializable]
public class AnomalyDefinition
{
    [Tooltip("Название (name) элемента в UXML (например, url-text)")]
    public string targetElementName;

    [Tooltip("Класс (class) элемента в UXML (если name не задан)")]
    public string targetElementClass;

    [Tooltip("Что именно сделать с элементом")]
    public AnomalyType anomalyType;

    [Tooltip("Новый текст (используется для ChangeText)")]
    public string replacementText;

    [Tooltip("CSS класс для добавления/удаления (используется для AddClass/RemoveClass)")]
    public string cssClassModifier;

    [Tooltip("Подсказка игроку (почему это фишинг)")]
    public string tooltipMessage;
}
