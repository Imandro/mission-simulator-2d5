using UnityEngine;

namespace WorldSystem
{
    /// <summary>
    /// Marcador visual para ubicaciones de misión (recogida/entrega)
    /// </summary>
    public class MissionMarker : MonoBehaviour
    {
        [Header("Tipo de marcador")]
        [SerializeField] private MarkerType type = MarkerType.Pickup;
        
        [Header("Visual")]
        [SerializeField] private Color pickupColor = Color.blue;
        [SerializeField] private Color deliveryColor = Color.green;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 0.2f;
        [SerializeField] private float rotationSpeed = 50f;
        
        [Header("Componentes")]
        [SerializeField] private MeshRenderer markerRenderer;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private Light markerLight;
        
        public enum MarkerType
        {
            Pickup,
            Delivery,
            Waypoint
        }
        
        private Vector3 originalScale;
        private Color originalColor;
        private Material markerMaterial;
        
        private void Awake()
        {
            originalScale = transform.localScale;
            
            // Configurar color inicial
            if (markerRenderer != null)
            {
                markerMaterial = markerRenderer.material;
                originalColor = type == MarkerType.Pickup ? pickupColor : deliveryColor;
                markerMaterial.color = originalColor;
                
                if (markerLight != null)
                {
                    markerLight.color = originalColor;
                }
            }
            
            // Configurar partículas
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = originalColor;
            }
        }
        
        private void Update()
        {
            // Animación de pulso
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseScale + 1f;
            transform.localScale = originalScale * pulse;
            
            // Rotación
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        /// <summary>
        /// Cambia el tipo de marcador en runtime
        /// </summary>
        public void SetType(MarkerType newType)
        {
            type = newType;
            Color newColor = type == MarkerType.Pickup ? pickupColor : deliveryColor;
            
            if (markerMaterial != null)
            {
                markerMaterial.color = newColor;
            }
            
            if (markerLight != null)
            {
                markerLight.color = newColor;
            }
            
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = newColor;
            }
        }
        
        /// <summary>
        /// Activa/desactiva el marcador
        /// </summary>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        
        /// <summary>
        /// Efecto de completado
        /// </summary>
        public void PlayCompleteEffect()
        {
            // Escalar y desvanecer
            LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEaseOutBack();
            
            if (particles != null)
            {
                particles.Play();
            }
        }
    }
}