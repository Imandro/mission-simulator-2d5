using UnityEngine;

namespace PlayerSystem
{
    /// <summary>
    /// Cámara 2.5D - sigue al jugador en plano XZ, altura fija, ángulo isométrico opcional
    /// </summary>
    public class Camera2D5 : MonoBehaviour
    {
        [Header("Objetivo")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 10, -10);
        
        [Header("Configuración")]
        [SerializeField] private float followSpeed = 10f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private bool useIsometricAngle = true;
        [SerializeField] private float isometricAngle = 30f; // Grados desde horizontal
        
        [Header("Límites")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector3 minBounds = new Vector3(-50, 0, -50);
        [SerializeField] private Vector3 maxBounds = new Vector3(50, 20, 50);
        
        [Header("Zoom")]
        [SerializeField] private float zoomDistance = 15f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 30f;
        [SerializeField] private float zoomSpeed = 5f;
        
        private Vector3 currentVelocity;
        private float currentZoom;
        
        private void Start()
        {
            // Buscar jugador si no asignado
            if (target == null)
            {
                var player = FindObjectOfType<PlayerController>();
                if (player != null)
                    target = player.transform;
            }
            
            currentZoom = zoomDistance;
            
            // Posición inicial
            if (target != null)
            {
                transform.position = target.position + CalculateOffset();
                UpdateRotation();
            }
        }
        
        private void LateUpdate()
        {
            if (target == null) return;
            
            HandleZoom();
            FollowTarget();
            UpdateRotation();
        }
        
        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                currentZoom -= scroll * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            }
        }
        
        private void FollowTarget()
        {
            Vector3 targetPosition = target.position + CalculateOffset();
            
            // Aplicar límites
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
                targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.z, maxBounds.z);
            }
            
            // Suavizar movimiento
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / followSpeed);
        }
        
        private Vector3 CalculateOffset()
        {
            if (useIsometricAngle)
            {
                // Calcular offset para ángulo isométrico
                float radians = isometricAngle * Mathf.Deg2Rad;
                float horizontalDist = currentZoom * Mathf.Cos(radians);
                float verticalDist = currentZoom * Mathf.Sin(radians);
                
                return new Vector3(0, verticalDist, -horizontalDist);
            }
            else
            {
                // Offset simple
                return new Vector3(0, currentZoom, -currentZoom);
            }
        }
        
        private void UpdateRotation()
        {
            if (useIsometricAngle)
            {
                // Rotación isométrica fija
                Quaternion targetRotation = Quaternion.Euler(isometricAngle, 0, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                // Mirar al objetivo
                Vector3 lookDir = (target.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        // Método público para cambiar objetivo (útil para cinemáticas)
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = Color.cyan;
                Vector3 center = (minBounds + maxBounds) / 2;
                Vector3 size = maxBounds - minBounds;
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}