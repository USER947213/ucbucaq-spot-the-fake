using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class SetupGameScene : EditorWindow
{
    [MenuItem("Tools/1. Подготовить сцену для игры")]
    public static void SetupScene()
    {
        // Check if GameManager already exists
        if (Object.FindFirstObjectByType<GameManager>() != null)
        {
            Debug.LogWarning("GameManager уже существует на сцене! Пожалуйста, удалите его перед повторной настройкой.");
            return;
        }

        // 1. Create UIDocument
        GameObject uiObj = new GameObject("Game UI (Сюда выводится интерфейс)");
        UIDocument uiDoc = uiObj.AddComponent<UIDocument>();

        // Автоматически импортируем ресурсы TextMeshPro через рефлексию для векторной четкости шрифтов
        if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
        {
            Debug.Log("Попытка импорта ресурсов TextMeshPro для векторной четкости шрифтов...");
            try
            {
                // Бесшумный импорт ресурсов TMP (без всплывающих окон)
                string tmpPackagePath = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
                if (System.IO.File.Exists(System.IO.Path.GetFullPath(tmpPackagePath)))
                {
                    AssetDatabase.ImportPackage(tmpPackagePath, false);
                    Debug.Log("TMP Essential Resources silently imported.");
                }
                else
                {
                    // Для старых версий Unity (до интеграции TMP в ugui)
                    string legacyTmpPath = "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";
                    if (System.IO.File.Exists(System.IO.Path.GetFullPath(legacyTmpPath)))
                    {
                        AssetDatabase.ImportPackage(legacyTmpPath, false);
                        Debug.Log("Legacy TMP Essential Resources silently imported.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Не удалось автоматически импортировать ресурсы TextMeshPro: " + ex.Message);
            }
        }

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

        // 3. Create GameManager and AudioManager
        GameObject gmObj = new GameObject("GameManager (Здесь логика)");
        GameManager gm = gmObj.AddComponent<GameManager>();
        AudioManager am = gmObj.AddComponent<AudioManager>();
        
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

        // 5. Setup Generator Modules
        ProceduralLevelGenerator generator = gmObj.AddComponent<ProceduralLevelGenerator>();
        
        if (AssetDatabase.IsValidFolder("Assets/UI/Modules"))
        {
            string[] moduleGuids = AssetDatabase.FindAssets("t:VisualTreeAsset", new[] { "Assets/UI/Modules" });
            foreach (var guid in moduleGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                
                if (asset.name.StartsWith("Header_"))
                    generator.headerModules.Add(asset);
                else if (asset.name.StartsWith("Body_"))
                    generator.bodyModules.Add(asset);
                else if (asset.name.StartsWith("Footer_"))
                    generator.footerModules.Add(asset);
                else if (asset.name == "Inject_UrgentTimer")
                    generator.urgentTimerAsset = asset;
            }
            Debug.Log($"Загружено модулей: {generator.headerModules.Count} хедеров, {generator.bodyModules.Count} тел, {generator.footerModules.Count} футеров.");
        }
        else
        {
            Debug.LogWarning("Папка Assets/UI/Modules не найдена. Создайте её и добавьте UXML модули для генерации.");
        }

        gm.levelGenerator = generator;
        
        Debug.Log("✅ Сцена успешно настроена! Теперь вы можете нажать кнопку Play сверху.");
    }
}
