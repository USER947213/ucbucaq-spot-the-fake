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

        // Назначаем PanelSettings, чтобы интерфейс рендерился
        string[] panelGuids = AssetDatabase.FindAssets("t:PanelSettings");
        PanelSettings panelSettings = null;
        if (panelGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(panelGuids[0]);
            panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
        }
        else
        {
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }
            AssetDatabase.CreateAsset(panelSettings, "Assets/Settings/DefaultPanelSettings.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Создан новый PanelSettings в Assets/Settings/DefaultPanelSettings.asset");
        }
        uiDoc.panelSettings = panelSettings;
        
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

        // 5. Setup Generator
        ProceduralLevelGenerator generator = gmObj.AddComponent<ProceduralLevelGenerator>();
        VisualTreeAsset cleanAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/CleanBankLevel.uxml");
        if (cleanAsset != null)
        {
            generator.cleanSiteTemplate = cleanAsset;
        }
        else
        {
            Debug.LogWarning("Не найден Assets/UI/CleanBankLevel.uxml");
        }
        gm.levelGenerator = generator;
        
        Debug.Log("✅ Сцена успешно настроена! Теперь вы можете нажать кнопку Play сверху.");
    }
}
