using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlowUIController : MonoBehaviour
{
    [Header("Referencias al modelo")]
    public BloodFlowController bloodFlow;

    [Header("UI - usa solo una de las dos")]
    public Image fillImage;   
    public Slider flowSlider; 
    public TextMeshProUGUI valueText;

    [Header("Ajustes de visualización")]
    public float maxFlow_m3s = 0.00012f;
    public float smoothSpeed = 6f;

    [Header("Colores de la barra")]
    public Color lowFlowColor = Color.blue;    // flujo bajo
    public Color midFlowColor = Color.green;   // flujo normal
    public Color highFlowColor = Color.red;    // flujo alto

    // Interno
    float displayNormalized = 0f;

    // void Start()
    // {
    //     if (bloodFlow == null)
    //     {
    //         bloodFlow = Object.FindFirstObjectByType<BloodFlowController>();
    //         if (bloodFlow == null)
    //             Debug.LogWarning("FlowUIController: no se encontró BloodFlowController en la escena.");
    //     }
    // }
    void Start()
    {
        if (bloodFlow == null)
        {
            // Busca el BloodFlowController en los hijos de este GameObject
            bloodFlow = GetComponentInChildren<BloodFlowController>();
            if (bloodFlow == null)
            {
                // Fallback: busca cualquier BloodFlowController en la escena
                bloodFlow = Object.FindFirstObjectByType<BloodFlowController>();
                if (bloodFlow == null)
                    Debug.LogWarning("FlowUIController: no se encontró BloodFlowController en la escena.");
            }
        }
    }

    void Update()
    {
        if (bloodFlow == null) return;

        // 1) Flujo real (m³/s)
        float q_m3s = bloodFlow.CurrentFlow;

        // 2) Normalizar flujo a la barra
        // Cambio mínimo: usar el flujo actual como máximo si es mayor que maxFlow_m3s
        float autoMax = Mathf.Max(maxFlow_m3s, q_m3s);
        float targetNormalized = Mathf.Clamp01(q_m3s / Mathf.Max(1e-12f, autoMax));

        // 3) Suavizado
        displayNormalized = Mathf.Lerp(displayNormalized, targetNormalized, Time.deltaTime * smoothSpeed);

        // 4) Actualizar barra (fill/slider)
        if (fillImage != null)
        {
            fillImage.fillAmount = displayNormalized;

            // ---- CAMBIO DE COLOR ----
            // Interpolación: bajo → normal → alto
            if (displayNormalized < 0.5f)
            {
                fillImage.color = Color.Lerp(lowFlowColor, midFlowColor, displayNormalized * 2f);
            }
            else
            {
                fillImage.color = Color.Lerp(midFlowColor, highFlowColor, (displayNormalized - 0.5f) * 2f);
            }
        }

        if (flowSlider != null) flowSlider.value = displayNormalized;

        // 5) Mostrar valor en L/min
        if (valueText != null)
        {
            float q_L_per_min = q_m3s * 1000f * 60f; // m³/s → L/min
            valueText.text = $"{q_L_per_min:F2} L/min";
        }

        Debug.Log($"[UI] Leyendo CurrentFlow de: {bloodFlow.name} = {bloodFlow.CurrentFlow}");

    }
}
