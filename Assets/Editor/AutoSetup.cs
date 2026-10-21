using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MissionSystem;

public class AutoSetup
{
    [MenuItem("Tools/Auto Setup Scene")]
    public static void SetupScene()
    {
        // Crear nueva escena
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Crear GameObject Setup
        GameObject setupObj = new GameObject("Setup");
        var missionSetup = setupObj.AddComponent<MissionSetup>();
        
        // Llamar al método de setup
        missionSetup.SetupTestScene();
        
        // Guardar la escena
        string scenePath = "Assets/Scenes/TestScene.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        // Añadir escena al Build Settings
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == scenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        
        Debug.Log("Escena de prueba creada y guardada en: " + scenePath);
    }
    
    // Auto-ejecutar al cargar el proyecto
    [InitializeOnLoadMethod]
    public static void AutoRun()
    {
        // Solo ejecutar si no hay escenas guardadas
        if (!System.IO.File.Exists("Assets/Scenes/TestScene.unity"))
        {
            EditorApplication.delayCall += () => {
                SetupScene();
                // Abrir la escena
                EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity");
            };
        }
    }
}