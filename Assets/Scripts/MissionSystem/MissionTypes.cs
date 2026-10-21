using System;
using UnityEngine;

namespace MissionSystem
{
    /// <summary>
    /// Tipos de misiones disponibles
    /// </summary>
    public enum MissionType
    {
        Delivery,      // Entrega de objeto
        Pickup,        // Recogida de objeto
        DeliveryAndReturn // Entrega y vuelta al origen
    }

    /// <summary>
    /// Estado actual de la misión
    /// </summary>
    public enum MissionState
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    /// <summary>
    /// Datos de una misión de entrega/recogida
    /// </summary>
    [Serializable]
    public class DeliveryMissionData
    {
        public string missionName = "Nueva Misión";
        public string description = "";
        public MissionType type = MissionType.Delivery;
        
        // Para misiones de entrega
        public Transform pickupLocation;
        public Transform deliveryLocation;
        public GameObject itemPrefab; // Prefab del objeto a transportar
        
        // Configuración
        public float timeLimit = 0f; // 0 = sin límite
        public int reward = 100;
        
        // Estado runtime
        [NonSerialized] public MissionState state = MissionState.NotStarted;
        [NonSerialized] public GameObject spawnedItem;
        [NonSerialized] public float startTime;
    }
}