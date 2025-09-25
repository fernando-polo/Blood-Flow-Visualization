using UnityEngine;

[DisallowMultipleComponent]
public class PressureShaderController : MonoBehaviour
{
    [Header("Referencias")]
    public BloodFlowController sourceFlow; // Modelo principal con datos
    public Renderer targetRenderer;        // Renderer del modelo de presión
    public Material overrideMaterial;      // Optional, fuerza instancia propia

    [Header("Shader Property Names")]
    public string pressureProperty = "_PressureGradient";
    public string inMinProperty = "_PG_InMin";
    public string inMaxProperty = "_PG_InMax";

    [Header("Rango esperado (Pa/m)")]
    public float gradientMin = 0f;
    public float gradientMax = 4000f;

    Material _matInstance;

    void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();

        // Instanciar material para no modificar asset original
        _matInstance = new Material(overrideMaterial != null ? overrideMaterial : targetRenderer.sharedMaterials[1]);
        targetRenderer.materials[1] = _matInstance; // Sobrescribe solo el slot 1 (PressureGradient)

        // Set rango inicial
        _matInstance.SetFloat(inMinProperty, gradientMin);
        _matInstance.SetFloat(inMaxProperty, gradientMax);
    }

    void Update()
    {
        if (sourceFlow != null)
        {
            // Calcula gradiente de presión simple (Pa/m)
            float deltaP = (sourceFlow.pressureIn - sourceFlow.pressureOut) * 133.322f; // mmHg → Pa
            float grad = deltaP / Mathf.Max(0.0001f, sourceFlow.length);

            // Actualiza shader
            _matInstance.SetFloat(pressureProperty, grad);
        }
    }
}
