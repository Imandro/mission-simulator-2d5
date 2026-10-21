using UnityEngine;

namespace PlayerSystem
{
    /// <summary>
    /// Controlador del jugador para movimiento 2.5D (movimiento en X/Z, cámara fija en Y)
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 1.5f;
        
        [Header("Referencias")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform itemHoldPoint; // Punto donde se sostiene el objeto
        
        [Header("Interacción")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float interactRange = 2f;
        
        // Estado
        private Vector3 velocity;
        private bool isGrounded;
        private GameObject carriedItem;
        private MissionDeliveryZone currentDeliveryZone;
        private bool inDeliveryZone = false;
        
        // Componentes
        private Animator animator;
        
        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
                
            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;
                
            animator = GetComponentInChildren<Animator>();
        }
        
        private void Update()
        {
            HandleMovement();
            HandleGravity();
            HandleInteraction();
            UpdateAnimator();
        }
        
        private void HandleMovement()
        {
            // Input WASD o flechas
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            // Dirección relativa a la cámara (solo plano XZ)
            Vector3 forward = cameraTransform != null ? 
                Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : 
                Vector3.forward;
            Vector3 right = cameraTransform != null ? 
                Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : 
                Vector3.right;
            
            Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
            
            // Mover
            if (moveDirection.magnitude > 0.1f)
            {
                characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
                
                // Rotar suavemente hacia la dirección de movimiento
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        private void HandleGravity()
        {
            isGrounded = characterController.isGrounded;
            
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Pequeño valor para mantener pegado al suelo
            }
            
            // Salto
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
        
        private void HandleInteraction()
        {
            // Interacción con tecla E
            if (Input.GetKeyDown(interactKey))
            {
                // Si lleva objeto y está en zona de entrega -> entregar
                if (carriedItem != null && inDeliveryZone && currentDeliveryZone != null)
                {
                    DeliverItem();
                }
                // Si no lleva objeto -> intentar recoger
                else if (carriedItem == null)
                {
                    TryPickupItem();
                }
            }
        }
        
        private void TryPickupItem()
        {
            // Raycast hacia adelante para detectar objetos recogibles
            Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                var pickup = hit.collider.GetComponent<MissionSystem.MissionPickup>();
                if (pickup != null)
                {
                    // El OnTriggerEnter del MissionPickup manejará la recogida
                    Debug.Log($"Objeto detectado: {hit.collider.name}");
                }
            }
        }
        
        private void DeliverItem()
        {
            if (currentDeliveryZone != null)
            {
                currentDeliveryZone.ConfirmDelivery();
                carriedItem = null;
                inDeliveryZone = false;
                currentDeliveryZone = null;
            }
        }
        
        private void UpdateAnimator()
        {
            if (animator != null)
            {
                float speed = characterController.velocity.magnitude;
                animator.SetFloat("Speed", speed);
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetBool("CarryingItem", carriedItem != null);
            }
        }
        
        // Métodos públicos para el sistema de misiones
        
        public bool IsCarryingItem()
        {
            return carriedItem != null;
        }
        
        public void SetCarriedItem(GameObject item)
        {
            carriedItem = item;
            
            // Posicionar en punto de agarre
            if (itemHoldPoint != null && item != null)
            {
                item.transform.SetParent(itemHoldPoint);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
            }
        }
        
        public GameObject GetCarriedItem()
        {
            return carriedItem;
        }
        
        public void SetInDeliveryZone(bool inZone, MissionDeliveryZone zone)
        {
            inDeliveryZone = inZone;
            currentDeliveryZone = zone;
            
            // Feedback visual opcional
            if (inZone)
            {
                Debug.Log("En zona de entrega - Presiona E para entregar");
            }
        }
        
        public bool IsInDeliveryZone()
        {
            return inDeliveryZone;
        }
        
        // Para debug
        private void OnDrawGizmosSelected()
        {
            // Rango de interacción
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up + transform.forward * interactRange, 0.3f);
        }
    }
}