using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MissionSystem
{
    /// <summary>
    /// Eventos del sistema de misiones
    /// </summary>
    [System.Serializable]
    public class MissionEvents
    {
        public UnityEvent<DeliveryMissionData> OnMissionStarted;
        public UnityEvent<DeliveryMissionData> OnMissionCompleted;
        public UnityEvent<DeliveryMissionData> OnMissionFailed;
        public UnityEvent<DeliveryMissionData> OnObjectiveUpdated;
    }

    /// <summary>
    /// Gestor principal del sistema de misiones
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }
        
        [Header("Configuración")]
        [SerializeField] private List<DeliveryMissionData> availableMissions = new List<DeliveryMissionData>();
        [SerializeField] private bool autoStartFirstMission = false;
        
        [Header("Eventos")]
        public MissionEvents missionEvents;
        
        // Estado actual
        private DeliveryMissionData currentMission;
        private int currentMissionIndex = -1;
        private bool isMissionActive = false;
        
        // Referencias
        private PlayerController playerController;
        private GameObject carriedItem;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            playerController = FindObjectOfType<PlayerController>();
            
            if (autoStartFirstMission && availableMissions.Count > 0)
            {
                StartMission(0);
            }
        }
        
        private void Update()
        {
            if (!isMissionActive || currentMission == null) return;
            
            // Verificar límite de tiempo
            if (currentMission.timeLimit > 0)
            {
                float elapsed = Time.time - currentMission.startTime;
                if (elapsed >= currentMission.timeLimit)
                {
                    FailMission("Tiempo agotado");
                    return;
                }
            }
            
            // Verificar condiciones de completado según tipo de misión
            CheckMissionProgress();
        }
        
        /// <summary>
        /// Inicia una misión por índice
        /// </summary>
        public void StartMission(int missionIndex)
        {
            if (missionIndex < 0 || missionIndex >= availableMissions.Count)
            {
                Debug.LogError($"Índice de misión inválido: {missionIndex}");
                return;
            }
            
            if (isMissionActive)
            {
                Debug.LogWarning("Ya hay una misión activa. Finalízala antes de empezar otra.");
                return;
            }
            
            currentMissionIndex = missionIndex;
            currentMission = availableMissions[missionIndex];
            currentMission.state = MissionState.InProgress;
            currentMission.startTime = Time.time;
            isMissionActive = true;
            
            // Spawn del objeto si es misión de entrega
            if (currentMission.type == MissionType.Delivery || currentMission.type == MissionType.DeliveryAndReturn)
            {
                SpawnMissionItem();
            }
            
            missionEvents.OnMissionStarted?.Invoke(currentMission);
            Debug.Log($"Misión iniciada: {currentMission.missionName}");
        }
        
        /// <summary>
        /// Spawnea el objeto de la misión en la ubicación de recogida
        /// </summary>
        private void SpawnMissionItem()
        {
            if (currentMission.itemPrefab != null && currentMission.pickupLocation != null)
            {
                currentMission.spawnedItem = Instantiate(
                    currentMission.itemPrefab, 
                    currentMission.pickupLocation.position, 
                    currentMission.pickupLocation.rotation
                );
                
                // Añadir componente para detectar recogida
                var pickup = currentMission.spawnedItem.AddComponent<MissionPickup>();
                pickup.Initialize(this, currentMission);
            }
            else
            {
                Debug.LogWarning("Falta prefab del objeto o ubicación de recogida");
            }
        }
        
        /// <summary>
        /// El jugador ha recogido el objeto
        /// </summary>
        public void OnItemPickedUp(DeliveryMissionData mission)
        {
            if (mission != currentMission) return;
            
            carriedItem = mission.spawnedItem;
            carriedItem.transform.SetParent(playerController.transform);
            carriedItem.transform.localPosition = new Vector3(0, 1f, 1f); // Delante del jugador
            carriedItem.GetComponent<Collider>().enabled = false; // Desactivar colisiones mientras se lleva
            
            missionEvents.OnObjectiveUpdated?.Invoke(mission);
        }
        
        /// <summary>
        /// El jugador ha dejado el objeto en la zona de entrega
        /// </summary>
        public void OnItemDelivered(DeliveryMissionData mission)
        {
            if (mission != currentMission) return;
            
            // Completar según tipo de misión
            switch (mission.type)
            {
                case MissionType.Delivery:
                    CompleteMission();
                    break;
                    
                case MissionType.DeliveryAndReturn:
                    // Para delivery and return, el objetivo ahora es volver al inicio
                    missionEvents.OnObjectiveUpdated?.Invoke(mission);
                    break;
                    
                case MissionType.Pickup:
                    // Para pickup simple, completar al recoger
                    CompleteMission();
                    break;
            }
        }
        
        /// <summary>
        /// El jugador ha vuelto al punto de origen (para DeliveryAndReturn)
        /// </summary>
        public void OnReturnToStart(DeliveryMissionData mission)
        {
            if (mission != currentMission || mission.type != MissionType.DeliveryAndReturn) return;
            
            CompleteMission();
        }
        
        /// <summary>
        /// Verifica el progreso de la misión cada frame
        /// </summary>
        private void CheckMissionProgress()
        {
            // Para misiones de solo recogida, verificar si el jugador está en la zona
            if (currentMission.type == MissionType.Pickup && currentMission.pickupLocation != null)
            {
                float distance = Vector3.Distance(playerController.transform.position, currentMission.pickupLocation.position);
                if (distance < 2f) // Radio de interacción
                {
                    CompleteMission();
                }
            }
            
            // Para DeliveryAndReturn, verificar vuelta al origen tras entrega
            if (currentMission.type == MissionType.DeliveryAndReturn && 
                currentMission.state == MissionState.InProgress &&
                carriedItem == null && // Ya entregó el objeto
                currentMission.pickupLocation != null)
            {
                float distance = Vector3.Distance(playerController.transform.position, currentMission.pickupLocation.position);
                if (distance < 2f)
                {
                    OnReturnToStart(currentMission);
                }
            }
        }
        
        /// <summary>
        /// Completa la misión actual con éxito
        /// </summary>
        public void CompleteMission()
        {
            if (!isMissionActive || currentMission == null) return;
            
            currentMission.state = MissionState.Completed;
            isMissionActive = false;
            
            // Limpiar objeto llevado
            if (carriedItem != null)
            {
                Destroy(carriedItem);
                carriedItem = null;
            }
            
            // Limpiar objeto spawneado si aún existe
            if (currentMission.spawnedItem != null)
            {
                Destroy(currentMission.spawnedItem);
                currentMission.spawnedItem = null;
            }
            
            missionEvents.OnMissionCompleted?.Invoke(currentMission);
            Debug.Log($"Misión completada: {currentMission.missionName} - Recompensa: {currentMission.reward}");
            
            // Auto-iniciar siguiente misión si existe
            if (currentMissionIndex + 1 < availableMissions.Count)
            {
                // Opcional: añadir delay o esperar input del jugador
            }
        }
        
        /// <summary>
        /// Falla la misión actual
        /// </summary>
        public void FailMission(string reason = "Misión fallida")
        {
            if (!isMissionActive || currentMission == null) return;
            
            currentMission.state = MissionState.Failed;
            isMissionActive = false;
            
            // Limpiar objetos
            if (carriedItem != null)
            {
                Destroy(carriedItem);
                carriedItem = null;
            }
            
            if (currentMission.spawnedItem != null)
            {
                Destroy(currentMission.spawnedItem);
                currentMission.spawnedItem = null;
            }
            
            missionEvents.OnMissionFailed?.Invoke(currentMission);
            Debug.Log($"Misión fallida: {currentMission.missionName} - Razón: {reason}");
        }
        
        /// <summary>
        /// Obtiene la misión actual
        /// </summary>
        public DeliveryMissionData GetCurrentMission()
        {
            return currentMission;
        }
        
        /// <summary>
        /// Obtiene lista de misiones disponibles
        /// </summary>
        public List<DeliveryMissionData> GetAvailableMissions()
        {
            return availableMissions;
        }
        
        /// <summary>
        /// Añade una misión a la lista
        /// </summary>
        public void AddMission(DeliveryMissionData mission)
        {
            availableMissions.Add(mission);
        }
    }
}