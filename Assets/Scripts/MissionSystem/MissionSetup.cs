using UnityEngine;
using System.Collections.Generic;

namespace MissionSystem
{
    /// <summary>
    /// Script para configurar rápidamente una escena de prueba con misiones
    /// </summary>
    public class MissionSetup : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private GameObject missionItemPrefab;
        [SerializeField] private GameObject pickupMarkerPrefab;
        [SerializeField] private GameObject deliveryMarkerPrefab;
        [SerializeField] private GameObject missionGiverPrefab;
        [SerializeField] private GameObject uiCanvasPrefab;
        
        [Header("Configuración de Misiones de Prueba")]
        [SerializeField] private bool createTestMissions = true;
        [SerializeField] private int numberOfTestMissions = 3;
        
        private MissionManager missionManager;
        
        [ContextMenu("Setup Test Scene")]
        public void SetupTestScene()
        {
            Debug.Log("Configurando escena de prueba...");
            
            // 1. Crear MissionManager
            CreateMissionManager();
            
            // 2. Crear Jugador
            CreatePlayer();
            
            // 3. Crear Cámara
            CreateCamera();
            
            // 4. Crear UI
            CreateUI();
            
            // 5. Crear misiones de prueba
            if (createTestMissions)
            {
                CreateTestMissions();
            }
            
            Debug.Log("Escena de prueba configurada correctamente.");
        }
        
        private void CreateMissionManager()
        {
            GameObject managerObj = new GameObject("MissionManager");
            missionManager = managerObj.AddComponent<MissionManager>();
        }
        
        private void CreatePlayer()
        {
            GameObject playerObj;
            
            if (playerPrefab != null)
            {
                playerObj = Instantiate(playerPrefab);
                playerObj.name = "Player";
            }
            else
            {
                // Crear jugador básico
                playerObj = new GameObject("Player");
                playerObj.tag = "Player";
                
                // CharacterController
                var controller = playerObj.AddComponent<CharacterController>();
                controller.height = 1.8f;
                controller.radius = 0.4f;
                controller.center = new Vector3(0, 0.9f, 0);
                
                // PlayerController
                var playerController = playerObj.AddComponent<PlayerSystem.PlayerController>();
                
                // Punto de agarre para objetos
                GameObject holdPoint = new GameObject("ItemHoldPoint");
                holdPoint.transform.SetParent(playerObj.transform);
                holdPoint.transform.localPosition = new Vector3(0, 1.2f, 0.8f);
                playerController.GetType().GetField("itemHoldPoint", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(playerController, holdPoint.transform);
            }
            
            // Posición inicial
            playerObj.transform.position = new Vector3(0, 1, 0);
        }
        
        private void CreateCamera()
        {
            GameObject cameraObj;
            
            if (cameraPrefab != null)
            {
                cameraObj = Instantiate(cameraPrefab);
                cameraObj.name = "MainCamera";
            }
            else
            {
                cameraObj = new GameObject("MainCamera");
                cameraObj.tag = "MainCamera";
                
                var cam = cameraObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.Skybox;
                
                var camController = cameraObj.AddComponent<PlayerSystem.Camera2D5>();
            }
            
            // Posición inicial para vista 2.5D
            cameraObj.transform.position = new Vector3(0, 15, -15);
            cameraObj.transform.rotation = Quaternion.Euler(30, 0, 0);
        }
        
        private void CreateUI()
        {
            GameObject uiObj;
            
            if (uiCanvasPrefab != null)
            {
                uiObj = Instantiate(uiCanvasPrefab);
                uiObj.name = "MissionUI";
            }
            else
            {
                // Crear Canvas básico
                uiObj = new GameObject("MissionUI");
                var canvas = uiObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiObj.AddComponent<CanvasScaler>();
                uiObj.AddComponent<GraphicRaycaster>();
                
                // Panel de misión
                GameObject panelObj = new GameObject("MissionPanel");
                panelObj.transform.SetParent(uiObj.transform, false);
                var panelRect = panelObj.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0, 1);
                panelRect.anchorMax = new Vector2(0, 1);
                panelRect.pivot = new Vector2(0, 1);
                panelRect.anchoredPosition = new Vector2(20, -20);
                panelRect.sizeDelta = new Vector2(300, 150);
                
                var panelImage = panelObj.AddComponent<Image>();
                panelImage.color = new Color(0, 0, 0, 0.7f);
                
                // Textos
                CreateUIText(panelObj, "MissionName", "Nombre Misión", 16, FontStyle.Bold, new Vector2(10, -10));
                CreateUIText(panelObj, "MissionDescription", "Descripción", 12, FontStyle.Normal, new Vector2(10, -35));
                CreateUIText(panelObj, "MissionObjective", "Objetivo: Ve a...", 12, FontStyle.Italic, new Vector2(10, -60));
                CreateUIText(panelObj, "TimerText", "--:--", 14, FontStyle.Bold, new Vector2(10, -90));
                
                // Progress bar
                GameObject sliderObj = new GameObject("ProgressBar");
                sliderObj.transform.SetParent(panelObj.transform, false);
                var sliderRect = sliderObj.AddComponent<RectTransform>();
                sliderRect.anchorMin = new Vector2(0, 0);
                sliderRect.anchorMax = new Vector2(1, 0);
                sliderRect.pivot = new Vector2(0.5f, 0);
                sliderRect.anchoredPosition = new Vector2(0, 10);
                sliderRect.sizeDelta = new Vector2(-20, 10);
                
                var slider = sliderObj.AddComponent<Slider>();
                var bg = new GameObject("Background");
                bg.transform.SetParent(sliderObj.transform, false);
                var bgRect = bg.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                var bgImage = bg.AddComponent<Image>();
                bgImage.color = new Color(1, 1, 1, 0.2f);
                slider.targetGraphic = bgImage;
                
                var fillArea = new GameObject("FillArea");
                fillArea.transform.SetParent(sliderObj.transform, false);
                var fillAreaRect = fillArea.AddComponent<RectTransform>();
                fillAreaRect.anchorMin = Vector2.zero;
                fillAreaRect.anchorMax = Vector2.one;
                fillAreaRect.sizeDelta = Vector2.zero;
                
                var fill = new GameObject("Fill");
                fill.transform.SetParent(fillArea.transform, false);
                var fillRect = fill.AddComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.sizeDelta = Vector2.zero;
                var fillImage = fill.AddComponent<Image>();
                fillImage.color = Color.green;
                slider.fillRect = fillRect;
                
                // Añadir MissionUI script
                var missionUI = uiObj.AddComponent<MissionUI>();
                missionUI.GetType().GetField("missionPanel", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(missionUI, panelObj);
                    
                missionUI.GetType().GetField("missionNameText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(missionUI, panelObj.transform.Find("MissionName").GetComponent<TextMeshProUGUI>());
                    
                missionUI.GetType().GetField("missionDescriptionText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(missionUI, panelObj.transform.Find("MissionDescription").GetComponent<TextMeshProUGUI>());
                    
                missionUI.GetType().GetField("missionObjectiveText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(missionUI, panelObj.transform.Find("MissionObjective").GetComponent<TextMeshProUGUI>());
                    
                missionUI.GetType().GetField("timerText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(missionUI, panelObj.transform.Find("TimerText").GetComponent<TextMeshProUGUI>());
                    
                missionUI.GetType().GetField("progressBar", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(missionUI, slider);
            }
        }
        
        private void CreateUIText(GameObject parent, string name, string text, int fontSize, FontStyle style, Vector2 anchoredPos)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(-20, 30);
            
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
        }
        
        private void CreateTestMissions()
        {
            // Crear ubicaciones para misiones
            Vector3[] pickupPositions = {
                new Vector3(-10, 0, 10),
                new Vector3(15, 0, -5),
                new Vector3(-15, 0, -15)
            };
            
            Vector3[] deliveryPositions = {
                new Vector3(10, 0, -10),
                new Vector3(-15, 0, 5),
                new Vector3(15, 0, 15)
            };
            
            // Crear objeto prefab si no existe
            GameObject itemPrefab = CreateDefaultItemPrefab();
            
            for (int i = 0; i < numberOfTestMissions; i++)
            {
                // Crear marcadores visuales
                CreateMarker(pickupPositions[i], true, $"PickupMarker_{i}");
                CreateMarker(deliveryPositions[i], false, $"DeliveryMarker_{i}");
                
                // Crear datos de misión
                var missionData = new DeliveryMissionData
                {
                    missionName = $"Misión {i + 1}: Entrega Express",
                    description = $"Entrega el paquete del punto A al punto B. Misión #{i + 1}.",
                    type = (MissionType)(i % 3), // Alterna entre los 3 tipos
                    pickupLocation = CreateTransformAt(pickupPositions[i], $"PickupLoc_{i}"),
                    deliveryLocation = CreateTransformAt(deliveryPositions[i], $"DeliveryLoc_{i}"),
                    itemPrefab = itemPrefab,
                    timeLimit = 120f + i * 30f, // 2, 2.5, 3 minutos
                    reward = 100 + i * 50
                };
                
                // Añadir al manager
                missionManager.AddMission(missionData);
            }
            
            // Crear dador de misiones inicial
            CreateMissionGiver(new Vector3(0, 0, 0), missionManager.GetAvailableMissions()[0]);
        }
        
        private GameObject CreateDefaultItemPrefab()
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = "MissionItem";
            item.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // Material distinguible
            var renderer = item.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.yellow;
            renderer.material = mat;
            
            // Rigidbody para física
            var rb = item.AddComponent<Rigidbody>();
            rb.mass = 1f;
            
            // Collider trigger
            var collider = item.GetComponent<BoxCollider>();
            collider.isTrigger = true;
            
            // Capa para ignorar colisiones con jugador
            item.layer = LayerMask.NameToLayer("IgnoreRaycast"); // O crear capa custom
            
            // Guardar como prefab en Resources para uso runtime
            #if UNITY_EDITOR
            string path = "Assets/Resources/MissionItem.prefab";
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(item, path);
            DestroyImmediate(item);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            #else
            return item;
            #endif
        }
        
        private Transform CreateTransformAt(Vector3 position, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = position;
            return obj.transform;
        }
        
        private void CreateMarker(Vector3 position, bool isPickup, string name)
        {
            GameObject markerObj = new GameObject(name);
            markerObj.transform.position = position + Vector3.up * 2f;
            
            // Visual simple: cilindro
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.SetParent(markerObj.transform);
            cylinder.transform.localScale = new Vector3(1, 0.1f, 1);
            cylinder.transform.localPosition = Vector3.zero;
            
            // Material
            var renderer = cylinder.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = isPickup ? Color.blue : Color.green;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", isPickup ? Color.blue * 0.5f : Color.green * 0.5f);
            renderer.material = mat;
            
            // Añadir script de marcador
            var marker = markerObj.AddComponent<WorldSystem.MissionMarker>();
            marker.GetType().GetField("type", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(marker, isPickup ? WorldSystem.MissionMarker.MarkerType.Pickup : WorldSystem.MissionMarker.MarkerType.Delivery);
                
            marker.GetType().GetField("markerRenderer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(marker, renderer);
            
            DestroyImmediate(cylinder.GetComponent<Collider>());
        }
        
        private void CreateMissionGiver(Vector3 position, DeliveryMissionData mission)
        {
            GameObject giverObj = new GameObject("MissionGiver");
            giverObj.transform.position = position;
            
            // Visual: cubo grande
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(giverObj.transform);
            cube.transform.localScale = new Vector3(2, 2, 2);
            cube.transform.localPosition = Vector3.up;
            
            var renderer = cube.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.cyan;
            renderer.material = mat;
            
            var collider = giverObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(3, 3, 3);
            collider.center = Vector3.up * 1.5f;
            
            var giver = giverObj.AddComponent<WorldSystem.MissionGiver>();
            giver.GetType().GetField("missionToGive", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(giver, mission);
        }
    }
}