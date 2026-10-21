using UnityEngine;
using PlayerSystem;

namespace MissionSystem
{
    /// <summary>
    /// Zona de entrega para misiones - detecta cuando el jugador deja el objeto
    /// </summary>
    public class MissionDeliveryZone : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private DeliveryMissionData targetMission;
        [SerializeField] private bool requireSpecificMission = true;
        [SerializeField] private float interactionRadius = 2f;
        
        [Header("Visual")]
        [SerializeField] private Color gizmoColor = Color.green;
        [SerializeField] private bool showGizmo = true;
        
        private MissionManager missionManager;
        private Collider zoneCollider;
        
        private void Awake()
        {
            missionManager = MissionManager.Instance;
            zoneCollider = GetComponent<Collider>();
            
            if (zoneCollider == null)
            {
                zoneCollider = gameObject.AddComponent<BoxCollider>();
            }
            zoneCollider.isTrigger = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null && player.IsCarryingItem())
            {
                // Verificar si es la misión correcta
                var currentMission = missionManager?.GetCurrentMission();
                if (currentMission != null && 
                    (!requireSpecificMission || currentMission == targetMission))
                {
                    // El jugador está en la zona de entrega con un objeto
                    // La entrega real ocurre cuando suelta el objeto (tecla E)
                    player.SetInDeliveryZone(true, this);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetInDeliveryZone(false, null);
            }
        }
        
        /// <summary>
        /// Confirma la entrega del objeto
        /// </summary>
        public void ConfirmDelivery()
        {
            var currentMission = missionManager?.GetCurrentMission();
            if (currentMission != null)
            {
                missionManager.OnItemDelivered(currentMission);
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showGizmo) return;
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
            
            // Etiqueta
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, "Zona de Entrega");
            #endif
        }
    }
}