using UnityEngine;

[DisallowMultipleComponent]
public class TurbulenceShaderDebugger : MonoBehaviour
{
    [Header("Referencia al BloodFlowController")]
    public BloodFlowController flowController;

    [Header("Renderer / Material")]
    public Renderer targetRenderer;
    [Tooltip("Índice del material que contiene el TurbulenceShader")]
    public int materialIndex = 0;

    private Material _matInstance;
    private Material[] _materials;

    [Header("Opciones")]
    public bool debugLogs = true;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogError($"{name} → No se encontró Renderer.");
            return;
        }

        InitializeMaterial();
    }

    void InitializeMaterial()
    {
        _materials = targetRenderer.materials;

        if (materialIndex < 0 || materialIndex >= _materials.Length)
        {
            Debug.LogError($"{name} → materialIndex fuera de rango.");
            return;
        }

        _matInstance = _materials[materialIndex];
        DebugLog("Material inicializado");
    }

    void Update()
    {
        if (_matInstance == null || flowController == null) return;

        // 1️⃣ Actualizar Reynolds dinámicamente
        float reynolds = CalculateReynolds(flowController);
        if (_matInstance.HasProperty("_Reynolds"))
            _matInstance.SetFloat("_Reynolds", reynolds);

        // 2️⃣ Enviar otros parámetros opcionales si deseas
        if (_matInstance.HasProperty("_TurbulenceIntensity"))
            _matInstance.SetFloat("_TurbulenceIntensity", 1f); // o configurable

        if (_matInstance.HasProperty("_NoiseScale"))
            _matInstance.SetFloat("_NoiseScale", 2f);

        if (_matInstance.HasProperty("_NoiseSpeed"))
            _matInstance.SetFloat("_NoiseSpeed", 1f);

        if (_matInstance.HasProperty("_Alpha"))
            _matInstance.SetFloat("_Alpha", 1f);

        SyncMaterialInstance();
    }

    float CalculateReynolds(BloodFlowController flow)
    {
        // Re = (ρ * v * D) / μ
        // Suponemos densidad sanguínea ρ ≈ 1050 kg/m³
        float rho = 1050f;
        float v = flow.CalculateAverageVelocity();
        float D = flow.diameter;
        float mu = flow.viscosity;

        float Re = (rho * v * D) / mu;
        return Re;
    }

    // 🔄 Reenganchamos material si cambia instancia (igual que en PressureShaderDebugger)
    void SyncMaterialInstance()
    {
        if (targetRenderer == null || materialIndex < 0) return;

        var mats = targetRenderer.materials;
        if (materialIndex >= mats.Length) return;

        if (_matInstance == null || mats[materialIndex].GetInstanceID() != _matInstance.GetInstanceID())
        {
            if (debugLogs)
                Debug.Log($"[{name}] 🔄 Reenganchando material (old={_matInstance?.GetInstanceID()} new={mats[materialIndex].GetInstanceID()})");

            _matInstance = mats[materialIndex];
        }
    }

    void DebugLog(string msg)
    {
        if (debugLogs) Debug.Log($"[{name}] {msg}");
    }
}
