using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class FlowBarColor : MonoBehaviour
{
    [Header("Referencias")]
    public Slider flowSlider;    // arrastra aquí tu FlowBar (Slider)
    public Image fillImage;      // arrastra aquí Fill (Image) del slider (opcional)

    [Header("Colores")]
    public Color lowFlowColor  = new Color(0f, 0.749f, 1f); // #00BFFF (azul)
    public Color midFlowColor  = new Color(0f, 1f, 0f);     // verde
    public Color highFlowColor = new Color(1f, 0.27f, 0f);  // #FF4500 (rojo)

    void Start()
    {
        if (flowSlider == null)
        {
            Debug.LogWarning("FlowBarColor: asigna el Slider (flowSlider). Intentando obtenerlo automáticamente...");
            flowSlider = GetComponentInParent<Slider>();
        }

        // Si no asignaste el fillImage explícitamente, lo intentamos obtener desde el slider
        if (fillImage == null && flowSlider != null && flowSlider.fillRect != null)
        {
            fillImage = flowSlider.fillRect.GetComponent<Image>();
            if (fillImage == null)
                Debug.LogWarning("FlowBarColor: no se encontró Image en flowSlider.fillRect.");
        }

        if (flowSlider == null || fillImage == null)
            Debug.LogWarning("FlowBarColor: faltan referencias (flowSlider o fillImage). El color no se actualizará.");
    }

    void Update()
    {
        if (flowSlider == null || fillImage == null) return;

        // Normaliza el valor del slider entre 0 y 1 (funciona incluso si min/max no son 0/1)
        float t = Mathf.InverseLerp(flowSlider.minValue, flowSlider.maxValue, flowSlider.value);
        // Interpolación 2-etapas: low -> mid -> high
        if (t < 0.5f)
        {
            // 0..0.5 -> mapa a 0..1 para Lerp(low, mid)
            float s = t * 2f;
            fillImage.color = Color.Lerp(lowFlowColor, midFlowColor, s);
        }
        else
        {
            // 0.5..1 -> mapa a 0..1 para Lerp(mid, high)
            float s = (t - 0.5f) * 2f;
            fillImage.color = Color.Lerp(midFlowColor, highFlowColor, s);
        }
    }
}
