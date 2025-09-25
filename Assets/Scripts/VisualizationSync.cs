using UnityEngine;

[DisallowMultipleComponent]
public class VisualizationSync : MonoBehaviour
{
    [Header("Referencia al controlador de flujo principal")]
    [Tooltip("Arrastra aquí el GameObject que tenga el BloodFlowController (el modelo principal).")]
    public BloodFlowController sourceController;

    [Header("Gradiente de presión (material destino)")]
    [Tooltip("Renderer del modelo que visualizará el gradiente de presión.")]
    public Renderer pressureRenderer;
    [Tooltip("Índice del material dentro del renderer (0..N-1).")]
    public int pressureMaterialIndex = 0;
    [Tooltip("Nombre de la propiedad en el Shader para asignar el gradiente de presión.")]
    public string pressureGradientProperty = "_PressureGradient";

    [Header("WSS (material destino)")]
    [Tooltip("Renderer del modelo que visualizará el WSS.")]
    public Renderer wssRenderer;
    [Tooltip("Índice del material dentro del renderer (0..N-1).")]
    public int wssMaterialIndex = 0;
    [Tooltip("Nombre de la propiedad en el Shader para asignar el valor de WSS.")]
    public string wssProperty = "_WSS";

    // Internos
    Material _pressureMat;
    Material _wssMat;

    void Awake()
    {
        // Instanciar materiales (igual que en tu BloodFlowController)
        if (pressureRenderer != null)
        {
            Material[] shared = pressureRenderer.sharedMaterials;
            if (pressureMaterialIndex >= 0 && pressureMaterialIndex < shared.Length)
                _pressureMat = new Material(shared[pressureMaterialIndex]);
            pressureRenderer.materials[pressureMaterialIndex] = _pressureMat;
        }

        if (wssRenderer != null)
        {
            Material[] shared = wssRenderer.sharedMaterials;
            if (wssMaterialIndex >= 0 && wssMaterialIndex < shared.Length)
                _wssMat = new Material(shared[wssMaterialIndex]);
            wssRenderer.materials[wssMaterialIndex] = _wssMat;
        }
    }

    void Update()
    {
        if (sourceController == null) return;

        // Datos base
        float deltaP_mmHg = sourceController.pressureIn - sourceController.pressureOut;
        float deltaP_Pa = deltaP_mmHg * 133.322f;
        float length = Mathf.Max(sourceController.length, 1e-6f);
        float radius = Mathf.Max(sourceController.diameter * 0.5f, 1e-6f);

        // 1) Gradiente de presión (Pa/m)
        float pressureGradient = deltaP_Pa / length;

        // 2) WSS (Pa) → fórmula (ΔP * R) / (2 * L)
        float wss = (radius / 2f) * (deltaP_Pa / length);

        // Asignar al material de presión
        if (_pressureMat != null)
        {
            _pressureMat.SetFloat("_PressureGradient", pressureGradient);
        }

        // Asignar al material de WSS
        if (_wssMat != null)
        {
            _wssMat.SetFloat(wssProperty, wss);
        }

        // Opcional: debug en consola
        // Debug.Log($"{name} → ΔP={deltaP_mmHg:F2} mmHg | Gradiente={pressureGradient:F2} Pa/m | WSS={wss:F2} Pa");
    }
}
