using UnityEngine;

[DisallowMultipleComponent]
public class PressureShaderDebugger : MonoBehaviour
{
    [Header("Referencias")]
    public BloodFlowController sourceFlow;   // arrastra el objeto principal con BloodFlowController
    public Renderer targetRenderer;          // arrastra el MeshRenderer del Aorta_Pressure (o deja vacío para buscar)
    [Tooltip("Índice del material que quieres reemplazar (ej. 1). Si -1: intenta auto-detectar.")]
    public int materialSlotIndex = 1;
    [Tooltip("Si asignas, el script usará esta Material como base (instanciada).")]
    public Material overrideMaterial;

    [Header("Nombres de propiedad en el shader (case-sensitive)")]
    public string pressureProperty = "_PressureGradient";
    public string inMinProperty = "_PG_InMin";
    public string inMaxProperty = "_PG_InMax";
    public string normalizedProperty = "_PressureValue"; // opcional: si tu shader usa un float normalizado

    [Header("Rango esperado (Pa/m)")]
    public float gradientMin = 0f;
    public float gradientMax = 4000f;

    [Header("Debug")]
    public bool debugLogs = true;
    [Tooltip("Segundos entre logs para evitar spam")]
    public float logInterval = 0.5f;

    Material _matInstance;
    int _usedIndex = -1;
    float _nextLogTime = 0f;

    [Header("Test Mode (ignora cálculos)")]
    [Range(0f, 1f)] public float testT = -1f;

    void OnEnable()
    {
        // Reconectar material cada vez que el objeto se activa
        if (debugLogs) Debug.Log($"[{name}] OnEnable llamado");
        InitializeMaterial();
        RefreshShader();
    }
    void RefreshShader()
    {
        if (_matInstance == null) return;

        if (_matInstance.HasProperty(inMinProperty)) _matInstance.SetFloat(inMinProperty, gradientMin);
        if (_matInstance.HasProperty(inMaxProperty)) _matInstance.SetFloat(inMaxProperty, gradientMax);

        // Aplica un valor inicial de gradiente (para no depender de Update)
        float grad = 0f;
        if (sourceFlow != null)
        {
            float deltaP_Pa = (sourceFlow.pressureIn - sourceFlow.pressureOut) * 133.322f;
            grad = deltaP_Pa / Mathf.Max(1e-6f, sourceFlow.length);
        }

        if (_matInstance.HasProperty(pressureProperty))
        {
            _matInstance.SetFloat(pressureProperty, grad);
            if (debugLogs) Debug.Log($"[{name}] [RefreshShader] Reaplicado {pressureProperty}={grad}");
        }

        if (_matInstance.HasProperty(normalizedProperty))
        {
            float t = Mathf.InverseLerp(gradientMin, gradientMax, grad);
            if (testT >= 0f) t = testT;
            _matInstance.SetFloat(normalizedProperty, t);
        }
    }
    void Awake()
    {
        if (debugLogs) Debug.Log($"[{name}] Awake llamado");

        // Intentar asignar sourceFlow automáticamente
        if (sourceFlow == null)
        {
            sourceFlow = FindObjectOfType<BloodFlowController>();
            if (sourceFlow != null && debugLogs)
                Debug.Log($"[{name}] SourceFlow asignado automáticamente a {sourceFlow.name}");
            else if (sourceFlow == null)
                Debug.LogWarning($"[{name}] No se encontró ningún BloodFlowController en la escena.");
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError($"[{name}] No se encontró targetRenderer. Arrastra manualmente el MeshRenderer del Aorta_Pressure.");
                return;
            }
        }

        InitializeMaterial();
    }

    void InitializeMaterial()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning($"[{name}] targetRenderer es null en InitializeMaterial()");
            return;
        }

        // Obtener materiales compartidos
        Material[] shared = targetRenderer.sharedMaterials;
        if (shared == null || shared.Length == 0)
        {
            Debug.LogWarning($"[{name}] targetRenderer.sharedMaterials es null o vacío");
            return;
        }

        // Determinar índice a usar
        int idx = materialSlotIndex;
        if (idx < 0 || idx >= shared.Length)
        {
            idx = -1;
            for (int i = 0; i < shared.Length; i++)
            {
                if (shared[i] != null)
                {
                    try
                    {
                        if (shared[i].HasProperty(pressureProperty))
                        {
                            idx = i;
                            break;
                        }
                    }
                    catch { }
                }
            }
            if (idx == -1) idx = Mathf.Clamp(materialSlotIndex, 0, shared.Length - 1);
            if (debugLogs) Debug.Log($"[{name}] Material slot detectado: {idx}");
        }
        _usedIndex = idx;

        // Instanciar material
        Material baseMat = overrideMaterial != null ? overrideMaterial : shared[_usedIndex];
        if (baseMat == null)
        {
            Debug.LogWarning($"[{name}] baseMat es null");
            return;
        }

        _matInstance = new Material(baseMat);

        // Reemplazar solo el slot objetivo
        Material[] mats = targetRenderer.materials;
        if (_usedIndex < 0 || _usedIndex >= mats.Length)
        {
            Debug.LogWarning($"[{name}] _usedIndex fuera de rango en materials array");
            return;
        }

        mats[_usedIndex] = _matInstance;
        targetRenderer.materials = mats;

        // Setear min/max en material si la propiedad existe
        if (_matInstance.HasProperty(inMinProperty)) _matInstance.SetFloat(inMinProperty, gradientMin);
        if (_matInstance.HasProperty(inMaxProperty)) _matInstance.SetFloat(inMaxProperty, gradientMax);

        if (debugLogs)
            Debug.Log($"[{name}] Material inicializado correctamente en slot {_usedIndex}: {_matInstance.name}");
    }
    void SyncMaterialInstance()
    {
        if (targetRenderer == null || _usedIndex < 0) return;

        var mats = targetRenderer.materials;
        if (_usedIndex >= mats.Length) return;

        if (_matInstance == null || mats[_usedIndex].GetInstanceID() != _matInstance.GetInstanceID())
        {
            if (debugLogs)
                Debug.Log($"[{name}] 🔄 Reenganchando material en slot {_usedIndex} (old={_matInstance?.GetInstanceID()} new={mats[_usedIndex].GetInstanceID()})");

            _matInstance = mats[_usedIndex];
        }
    }

    void Update()
    {

        if (debugLogs && Time.frameCount % 60 == 0) // cada ~1 seg
        {
            var currentMats = targetRenderer.materials;
            if (_usedIndex >= 0 && _usedIndex < currentMats.Length)
                Debug.Log($"[{name}] Frame {Time.frameCount} -> Material en renderer[{_usedIndex}] = {currentMats[_usedIndex].GetInstanceID()} | _matInstance = {_matInstance.GetInstanceID()}");
        }

        if (sourceFlow == null)
        {
            Debug.LogWarning($"[{name}] sourceFlow es null en Update()");
            return;
        }
        if (_matInstance == null)
        {
            Debug.LogWarning($"[{name}] _matInstance es null en Update()");
            return;
        }
        if (targetRenderer == null)
        {
            Debug.LogWarning($"[{name}] targetRenderer es null en Update()");
            return;
        }

        // Calculos
        float deltaP_mmHg = sourceFlow.pressureIn - sourceFlow.pressureOut;
        float deltaP_Pa = deltaP_mmHg * 133.322f;
        float length = Mathf.Max(1e-6f, sourceFlow.length);
        float grad = deltaP_Pa / length; // Pa/m

        if (debugLogs)
        {
            Debug.Log($"[{name}] Calculado gradiente: ΔP={deltaP_mmHg:F3} mmHg → {deltaP_Pa:F1} Pa | length={sourceFlow.length} m | grad={grad:F1} Pa/m");
        }

        float t = Mathf.InverseLerp(gradientMin, gradientMax, grad);
        if (testT >= 0f) t = testT;

        // Enviar valores al shader y verificar
        if (_matInstance.HasProperty(pressureProperty))
        {
            _matInstance.SetFloat(pressureProperty, grad);
            float check = _matInstance.GetFloat(pressureProperty);
            if (debugLogs) Debug.Log($"[{name}] _PressureGradient enviado: {grad} → verificado: {check}");
        }
        else
        {
            if (debugLogs) Debug.LogWarning($"[{name}] Material NO tiene la propiedad {pressureProperty}");
        }

        if (_matInstance.HasProperty(normalizedProperty))
            _matInstance.SetFloat(normalizedProperty, t);
        if (_matInstance.HasProperty(inMinProperty))
            _matInstance.SetFloat(inMinProperty, gradientMin);
        if (_matInstance.HasProperty(inMaxProperty))
            _matInstance.SetFloat(inMaxProperty, gradientMax);

        // Log controlado para evitar spam
        if (debugLogs && Time.time >= _nextLogTime)
        {
            _nextLogTime = Time.time + Mathf.Max(0.001f, logInterval);
            string props = $"HasPressureProp={_matInstance.HasProperty(pressureProperty)} HasNormProp={_matInstance.HasProperty(normalizedProperty)}";
            Debug.Log($"[{name}] ΔP={deltaP_mmHg:F3} mmHg → Grad={grad:F1} Pa/m | t={t:F3} | slot={_usedIndex} | {props}");
        }

            SyncMaterialInstance();
    }

}
