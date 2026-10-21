using UnityEngine;
using UnityEngine.Events;

namespace WorldSystem
{
    /// <summary>
    /// NPC o punto que entrega misiones al jugador
    /// </summary>
    public class MissionGiver : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private MissionSystem.DeliveryMissionData missionToGive;
        [SerializeField] private bool autoAssignOnTrigger = true;
        [SerializeField] private bool requireInteraction = true;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float interactionRange = 3f;
        
        [Header("Visual")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private Color gizmoColor = Color.cyan;
        
        [Header("Eventos")]
        public UnityEvent OnMissionOffered;
        public UnityEvent OnMissionAccepted;
        public UnityEvent OnMissionDeclined;
        
        private MissionSystem.MissionManager missionManager;
        private PlayerSystem.PlayerController player;
        private bool playerInRange = false;
        private bool missionOffered = false;
        
        private void Awake()
        {
            missionManager = MissionSystem.MissionManager.Instance;
        }
        
        private void Start()
        {
            player = FindObjectOfType<PlayerSystem.PlayerController>();
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (requireInteraction && playerInRange && !missionOffered)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    OfferMission();
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerSystem.PlayerController>() != null)
            {
                playerInRange = true;
                
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }
                
                if (autoAssignOnTrigger && !requireInteraction)
                {
                    OfferMission();
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerSystem.PlayerController>() != null)
            {
                playerInRange = false;
                
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
            }
        }
        
        private void OfferMission()
        {
            if (missionManager == null || missionToGive == null) return;
            
            missionOffered = true;
            OnMissionOffered?.Invoke();
            
            // En un juego real, aquí mostrarías UI de diálogo
            // Por simplicidad, auto-aceptamos
            AcceptMission();
        }
        
        private void AcceptMission()
        {
            // Buscar índice de la misión en el manager
            int missionIndex = missionManager.GetAvailableMissions().FindIndex(m => m == missionToGive);
            
            if (missionIndex >= 0)
            {
                missionManager.StartMission(missionIndex);
                OnMissionAccepted?.Invoke();
                Debug.Log($"Misión aceptada: {missionToGive.missionName}");
            }
            else
            {
                Debug.LogWarning("La misión no está registrada en el MissionManager");
            }
        }
        
        private void DeclineMission()
        {
            missionOffered = false;
            OnMissionDeclined?.Invoke();
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, "Dador de Misiones");
            #endif
        }
    }
}