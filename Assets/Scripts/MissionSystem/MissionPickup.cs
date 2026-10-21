using UnityEngine;

namespace MissionSystem
{
    /// <summary>
    /// Componente para detectar cuando el jugador recoge el objeto de la misión
    /// </summary>
    public class MissionPickup : MonoBehaviour
    {
        private MissionManager missionManager;
        private DeliveryMissionData missionData;
        private Collider itemCollider;
        
        public void Initialize(MissionManager manager, DeliveryMissionData data)
        {
            missionManager = manager;
            missionData = data;
            itemCollider = GetComponent<Collider>();
            
            // Asegurar que tiene collider trigger
            if (itemCollider != null)
            {
                itemCollider.isTrigger = true;
            }
            else
            {
                // Añadir collider si no tiene
                var boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Verificar si es el jugador
            var player = other.GetComponent<PlayerController>();
            if (player != null && missionManager != null)
            {
                // Verificar que el jugador no lleva nada ya
                if (!player.IsCarryingItem())
                {
                    missionManager.OnItemPickedUp(missionData);
                    player.SetCarriedItem(gameObject);
                    
                    // Desactivar este script para evitar múltiples triggers
                    enabled = false;
                }
            }
        }
    }
}