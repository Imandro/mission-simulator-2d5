using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MissionSystem
{
    /// <summary>
    /// UI simple para mostrar estado de misión actual
    /// </summary>
    public class MissionUI : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private GameObject missionPanel;
        [SerializeField] private TextMeshProUGUI missionNameText;
        [SerializeField] private TextMeshProUGUI missionDescriptionText;
        [SerializeField] private TextMeshProUGUI missionObjectiveText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image missionIcon;
        
        [Header("Iconos")]
        [SerializeField] private Sprite deliveryIcon;
        [SerializeField] private Sprite pickupIcon;
        [SerializeField] private Sprite returnIcon;
        
        private MissionManager missionManager;
        private DeliveryMissionData currentMission;
        
        private void Awake()
        {
            missionManager = MissionManager.Instance;
            
            if (missionPanel != null)
            {
                missionPanel.SetActive(false);
            }
        }
        
        private void Start()
        {
            if (missionManager != null)
            {
                missionManager.missionEvents.OnMissionStarted.AddListener(OnMissionStarted);
                missionManager.missionEvents.OnMissionCompleted.AddListener(OnMissionCompleted);
                missionManager.missionEvents.OnMissionFailed.AddListener(OnMissionFailed);
                missionManager.missionEvents.OnObjectiveUpdated.AddListener(OnObjectiveUpdated);
            }
        }
        
        private void Update()
        {
            if (currentMission != null && currentMission.state == MissionState.InProgress)
            {
                UpdateTimer();
                UpdateProgress();
            }
        }
        
        private void OnMissionStarted(DeliveryMissionData mission)
        {
            currentMission = mission;
            UpdateUI();
            
            if (missionPanel != null)
            {
                missionPanel.SetActive(true);
            }
        }
        
        private void OnMissionCompleted(DeliveryMissionData mission)
        {
            if (missionPanel != null)
            {
                missionPanel.SetActive(false);
            }
            currentMission = null;
        }
        
        private void OnMissionFailed(DeliveryMissionData mission)
        {
            if (missionPanel != null)
            {
                missionPanel.SetActive(false);
            }
            currentMission = null;
        }
        
        private void OnObjectiveUpdated(DeliveryMissionData mission)
        {
            currentMission = mission;
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (currentMission == null) return;
            
            if (missionNameText != null)
                missionNameText.text = currentMission.missionName;
                
            if (missionDescriptionText != null)
                missionDescriptionText.text = currentMission.description;
            
            // Objetivo según tipo y estado
            string objective = GetObjectiveText();
            if (missionObjectiveText != null)
                missionObjectiveText.text = objective;
            
            // Icono
            if (missionIcon != null)
            {
                switch (currentMission.type)
                {
                    case MissionType.Delivery:
                        missionIcon.sprite = deliveryIcon;
                        break;
                    case MissionType.Pickup:
                        missionIcon.sprite = pickupIcon;
                        break;
                    case MissionType.DeliveryAndReturn:
                        missionIcon.sprite = returnIcon;
                        break;
                }
            }
            
            // Progress bar
            if (progressBar != null)
            {
                progressBar.value = 0f; // Se actualiza en UpdateProgress
            }
        }
        
        private string GetObjectiveText()
        {
            if (currentMission == null) return "";
            
            switch (currentMission.type)
            {
                case MissionType.Delivery:
                    if (currentMission.spawnedItem != null)
                        return "Ve a la ubicación de recogida (marcador azul)";
                    else
                        return "Entrega el objeto en la zona verde";
                        
                case MissionType.Pickup:
                    return "Recoge el objeto en la ubicación marcada";
                    
                case MissionType.DeliveryAndReturn:
                    if (currentMission.spawnedItem != null)
                        return "Recoge el objeto y llévalo a la zona de entrega";
                    else if (MissionManager.Instance.GetCurrentMission().state == MissionState.InProgress)
                        return "Vuelve al punto de partida";
                    else
                        return "Misión completada";
                        
                default:
                    return "";
            }
        }
        
        private void UpdateTimer()
        {
            if (timerText == null || currentMission == null) return;
            
            if (currentMission.timeLimit > 0)
            {
                float remaining = currentMission.timeLimit - (Time.time - currentMission.startTime);
                remaining = Mathf.Max(0, remaining);
                
                int minutes = Mathf.FloorToInt(remaining / 60);
                int seconds = Mathf.FloorToInt(remaining % 60);
                
                timerText.text = $"{minutes:00}:{seconds:00}";
                
                // Cambiar color si queda poco tiempo
                if (remaining < 30)
                {
                    timerText.color = Color.red;
                }
                else if (remaining < 60)
                {
                    timerText.color = Color.yellow;
                }
                else
                {
                    timerText.color = Color.white;
                }
            }
            else
            {
                timerText.text = "--:--";
                timerText.color = Color.white;
            }
        }
        
        private void UpdateProgress()
        {
            if (progressBar == null || currentMission == null) return;
            
            float progress = 0f;
            
            switch (currentMission.type)
            {
                case MissionType.Delivery:
                    // 50% al recoger, 100% al entregar
                    if (currentMission.spawnedItem == null)
                    {
                        // Objeto ya recogido
                        progress = 0.5f;
                        
                        // Calcular distancia a entrega
                        if (currentMission.deliveryLocation != null)
                        {
                            var player = FindObjectOfType<PlayerSystem.PlayerController>();
                            if (player != null)
                            {
                                float dist = Vector3.Distance(player.transform.position, currentMission.deliveryLocation.position);
                                float maxDist = 50f; // Distancia máxima estimada
                                progress = 0.5f + 0.5f * Mathf.Clamp01(1f - dist / maxDist);
                            }
                        }
                    }
                    break;
                    
                case MissionType.Pickup:
                    if (currentMission.pickupLocation != null)
                    {
                        var player = FindObjectOfType<PlayerSystem.PlayerController>();
                        if (player != null)
                        {
                            float dist = Vector3.Distance(player.transform.position, currentMission.pickupLocation.position);
                            float maxDist = 50f;
                            progress = Mathf.Clamp01(1f - dist / maxDist);
                        }
                    }
                    break;
                    
                case MissionType.DeliveryAndReturn:
                    if (currentMission.spawnedItem != null)
                    {
                        // Aún no recogido
                        progress = 0f;
                    }
                    else if (currentMission.state == MissionState.InProgress)
                    {
                        // Entregado, volviendo
                        if (currentMission.pickupLocation != null)
                        {
                            var player = FindObjectOfType<PlayerSystem.PlayerController>();
                            if (player != null)
                            {
                                float dist = Vector3.Distance(player.transform.position, currentMission.pickupLocation.position);
                                float maxDist = 50f;
                                progress = 0.5f + 0.5f * Mathf.Clamp01(1f - dist / maxDist);
                            }
                        }
                    }
                    break;
            }
            
            progressBar.value = progress;
        }
        
        private void OnDestroy()
        {
            if (missionManager != null)
            {
                missionManager.missionEvents.OnMissionStarted.RemoveListener(OnMissionStarted);
                missionManager.missionEvents.OnMissionCompleted.RemoveListener(OnMissionCompleted);
                missionManager.missionEvents.OnMissionFailed.RemoveListener(OnMissionFailed);
                missionManager.missionEvents.OnObjectiveUpdated.RemoveListener(OnObjectiveUpdated);
            }
        }
    }
}