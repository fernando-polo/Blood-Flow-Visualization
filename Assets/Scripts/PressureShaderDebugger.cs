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

    void Awake()
    {
        if (targetRenderer == null)
        {
            // intentar encontrar renderer en este GameObject
            targetRenderer = GetComponentInChildren<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError($"[{name}] No se encontró targetRenderer. Arrastra manualmente el MeshRenderer del Aorta_Pressure.");
                return;
            }
        }

        // Obtener materiales compartidos
        Material[] shared = targetRenderer.sharedMaterials;
        if (shared == null || shared.Length == 0)
        {
            Debug.LogError($"[{name}] targetRenderer no tiene materiales asignados.");
            return;
        }

        // Determinar índice a usar
        int idx = materialSlotIndex;
        if (idx < 0 || idx >= shared.Length)
        {
            // intentar detectar primer material que tenga la propiedad
            idx = -1;
            for (int i = 0; i < shared.Length; i++)
            {
                if (shared[i] != null)
                {
                    // NO usamos HasProperty en assets null-safe
                    try
                    {
                        if (shared[i].HasProperty(pressureProperty))
                        {
                            idx = i;
                            break;
                        }
                    }
                    catch { /* ignorar */ }
                }
            }

            if (idx == -1)
            {
                // si aún no encontramos, fallback al slot 1 si existe
                idx = Mathf.Clamp(materialSlotIndex, 0, shared.Length - 1);
                Debug.LogWarning($"[{name}] Índice solicitado fuera de rango. Se usará índice {idx} por fallback. Considera poner Override Material para garantizar el shader correcto.");
            }
        }

        _usedIndex = idx;

        // Instanciar material para no tocar asset original
        Material baseMat = overrideMaterial != null ? overrideMaterial : shared[_usedIndex];
        if (baseMat == null)
        {
            Debug.LogError($"[{name}] Material base es null en índice {_usedIndex}");
            return;
        }
        _matInstance = new Material(baseMat);

        // Reemplazar solo el slot objetivo en la lista de materiales
        Material[] mats = targetRenderer.materials; // esto devuelve instancias
        if (_usedIndex < 0 || _usedIndex >= mats.Length)
        {
            Debug.LogError($"[{name}] Índice {_usedIndex} fuera de rango para materials array (length {mats.Length}).");
            return;
        }
        mats[_usedIndex] = _matInstance;
        targetRenderer.materials = mats;

        // Setear min/max en material si la propiedad existe
        if (_matInstance.HasProperty(inMinProperty)) _matInstance.SetFloat(inMinProperty, gradientMin);
        if (_matInstance.HasProperty(inMaxProperty)) _matInstance.SetFloat(inMaxProperty, gradientMax);

        if (debugLogs)
            Debug.Log($"[{name}] Inicializado. Usando material slot {_usedIndex} ({_matInstance.name}). Overrides: {(overrideMaterial!=null ? "sí":"no")}");
    }

    void Update()
    {
        if (sourceFlow == null)
        {
            if (debugLogs) Debug.LogWarning($"[{name}] sourceFlow es null. Arrastra el objeto que contiene BloodFlowController al campo Source Flow.");
            return;
        }
        if (_matInstance == null) return;

        // Calculos
        float deltaP_mmHg = sourceFlow.pressureIn - sourceFlow.pressureOut;
        float deltaP_Pa = deltaP_mmHg * 133.322f;
        float length = Mathf.Max(1e-6f, sourceFlow.length);
        float grad = deltaP_Pa / length; // Pa/m

        // Normalizado [0,1]
        float t = Mathf.InverseLerp(gradientMin, gradientMax, grad);

        if (testT >= 0f) t = testT;

        // Actualizar propiedades en material SOLO si existen
        if (_matInstance.HasProperty(pressureProperty))
            _matInstance.SetFloat(pressureProperty, grad);
        if (_matInstance.HasProperty(normalizedProperty))
            _matInstance.SetFloat(normalizedProperty, t);
        if (_matInstance.HasProperty(inMinProperty))
            _matInstance.SetFloat(inMinProperty, gradientMin);
        if (_matInstance.HasProperty(inMaxProperty))
            _matInstance.SetFloat(inMaxProperty, gradientMax);

        // Logging controlado
        if (debugLogs && Time.time >= _nextLogTime)
        {
            _nextLogTime = Time.time + Mathf.Max(0.001f, logInterval);
            string props = $"HasPressureProp={_matInstance.HasProperty(pressureProperty)} HasNormProp={_matInstance.HasProperty(normalizedProperty)}";
            Debug.Log($"[{name}] ΔP={deltaP_mmHg:F3} mmHg → {deltaP_Pa:F1} Pa | Grad={grad:F1} Pa/m | t={t:F3} | slot={_usedIndex} | {props}");
        }
    }
}
