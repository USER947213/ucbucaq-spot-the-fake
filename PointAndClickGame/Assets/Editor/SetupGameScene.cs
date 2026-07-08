using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class SetupGameScene : EditorWindow
{
    [MenuItem("Tools/1. Подготовить сцену для игры")]
    public static void SetupScene()
    {
        // Check if GameManager already exists
        if (Object.FindObjectOfType<GameManager>() != null)
        {
            Debug.LogWarning("GameManager уже существует на сцене!");
            return;
        }

        // 1. Create UIDocument
        GameObject uiObj = new GameObject("Game UI (Сюда выводится интерфейс)");
        UIDocument uiDoc = uiObj.AddComponent<UIDocument>();
        
        // 2. Load the MainOverlay UXML
        VisualTreeAsset mainUIAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/MainOverlay.uxml");
        if (mainUIAsset == null)
        {
            Debug.LogError("Не найден файл Assets/UI/MainOverlay.uxml. Дождитесь пока Unity скомпилирует все файлы.");
            return;
        }
        uiDoc.visualTreeAsset = mainUIAsset;

        // 3. Create GameManager
        GameObject gmObj = new GameObject("GameManager (Здесь логика)");
        GameManager gm = gmObj.AddComponent<GameManager>();
        
        // 4. Assign references
        gm.mainUIDocument = uiDoc;
        
        VisualTreeAsset tutorialAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/TutorialLevel.uxml");
        if (tutorialAsset != null)
        {
            gm.tutorialLevelAsset = tutorialAsset;
        }
        else
        {
            Debug.LogWarning("Не найден Assets/UI/TutorialLevel.uxml");
        }
        
        Debug.Log("✅ Сцена успешно настроена! Теперь вы можете нажать кнопку Play сверху.");
    }
}
