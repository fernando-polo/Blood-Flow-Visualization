using UnityEngine;

[DisallowMultipleComponent]
public class BloodFlowController : MonoBehaviour
{
    [Header("Propiedades físicas del vaso (Inspector)")]
    [Tooltip("Presión de entrada (mmHg)")]
    public float pressureIn = 100f;   // mmHg
    [Tooltip("Presión de salida (mmHg)")]
    public float pressureOut = 95f;   // mmHg
    [Tooltip("Longitud del vaso en metros")]
    public float length = 0.25f;      // m
    [Tooltip("Diámetro interno (m)")]
    public float diameter = 0.012f;   // m
    [Tooltip("Viscosidad dinámica (Pa·s)")]
    public float viscosity = 0.0035f; // Pa·s

    [Header("Material / Renderer")]
    [Tooltip("Índice del material que contiene el shader BloodFlow (0..N-1)")]
    public int bloodFlowMaterialIndex = 1; // por defecto 1 (ajusta si tu FBX tiene otro orden)
    [Tooltip("Material alternativo para forzar el shader BloodFlow (opcional). Si se asigna, reemplaza la instancia en el índice")]
    public Material overrideFlowMaterial = null;

    [Header("Propiedades en Shader Graph (nombres exactos)")]
    public string flowSpeedProperty = "_FlowSpeed";
    public string pressureInProperty = "_PressureIn";
    public string pressureOutProperty = "_PressureOut";
    public string lengthProperty = "_Length";
    public string xMinProperty = "_XMin";
    public string xMaxProperty = "_XMax";
    public string epsilonProperty = "_Epsilon";
    public string pressureGradientProperty = "_PressureGradient";

    [Header("Opciones")]
    [Tooltip("Si true, recalcula bounds (_XMin/_XMax) cada frame (útil si el objeto se mueve/escala).")]
    public bool updateBoundsEveryFrame = false;
    [Tooltip("Valor epsilon enviado al shader para evitar divisiones por cero")]
    public float epsilon = 1e-5f;

    // Interno
    Renderer _renderer;
    Material[] _instances;
    Material _localMaterial; // material que controla BloodFlow (instancia)
    bool _initialized = false;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
            _renderer = GetComponentInChildren<Renderer>();

        if (_renderer == null)
        {
            Debug.LogWarning($"{name} → No se encontró Renderer en el GameObject o hijos.");
            return;
        }

        InitializeMaterials();
    }

    void InitializeMaterials()
    {
        // Instanciar todos los materiales del renderer (para no modificar assets)
        Material[] shared = _renderer.sharedMaterials;
        _instances = new Material[shared.Length];

        for (int i = 0; i < shared.Length; i++)
        {
            if (i == bloodFlowMaterialIndex && overrideFlowMaterial != null)
            {
                // Si se proveyó overrideFlowMaterial, usarla (instanciada) en el índice seleccionado
                _instances[i] = new Material(overrideFlowMaterial);
            }
            else if (shared[i] != null)
            {
                _instances[i] = new Material(shared[i]);
            }
            else
            {
                _instances[i] = null;
            }
        }

        // Asignar instancias al renderer
        _renderer.materials = _instances;

        // Guardar referencia al material que vamos a controlar
        int idx = Mathf.Clamp(bloodFlowMaterialIndex, 0, _instances.Length - 1);
        if (_instances.Length > 0 && _instances[idx] != null)
        {
            _localMaterial = _instances[idx];
            Debug.Log($"{name} → Instancia de material para BloodFlow creada (índice {idx}): {_localMaterial.name}");
        }
        else
        {
            _localMaterial = null;
            Debug.LogWarning($"{name} → No se pudo obtener la instancia del material en el índice {idx}. Revisa bloodFlowMaterialIndex y materiales del Renderer.");
        }

        // Enviar epsilon inicial si el shader lo espera
        if (_localMaterial != null)
        {
            _localMaterial.SetFloat(epsilonProperty, epsilon);
        }

        // Calcular bounds iniciales
        UpdateBoundsToMaterial();

        _initialized = true;
    }

    void Update()
    {
        if (!_initialized || _localMaterial == null) return;

        // 1) Calcular velocidad (FlowSpeed) usando Ley de Poiseuille
        float velocity = CalculateAverageVelocity();
        _localMaterial.SetFloat(flowSpeedProperty, velocity);

        // 2) Enviar presiones y longitud al shader
        _localMaterial.SetFloat(pressureInProperty, pressureIn);
        _localMaterial.SetFloat(pressureOutProperty, pressureOut);
        _localMaterial.SetFloat(lengthProperty, length);
        _localMaterial.SetFloat(epsilonProperty, epsilon);

        // 3) Enviar gradiente de presión al shader 👇
        float gradient = (pressureIn - pressureOut) / Mathf.Max(length, 1e-6f);
        if (_localMaterial.HasProperty(pressureGradientProperty))
            _localMaterial.SetFloat(pressureGradientProperty, gradient);

        Debug.Log($"{name} → ΔP={pressureIn - pressureOut} mmHg | Grad={gradient:F2} mmHg/m");

        // 4) Actualizar bounds si requerido (Xmin/Xmax en local space)
        if (updateBoundsEveryFrame)
            UpdateBoundsToMaterial();

    }

    public float CurrentFlow { get; private set; }  // flujo en m³/s


    /// <summary>
    /// Flujo en litros por minuto (L/min)
    /// </summary>
    public float flowLpm
    {
        get { return CurrentFlow * 1000f * 60f; } // m³/s → L/min
    }


    public float CalculateAverageVelocity()
    {
        // ΔP en Pascales
        float deltaP_mmHg = pressureIn - pressureOut;
        float deltaP = deltaP_mmHg * 133.322f;

        // radio (m)
        float radius = Mathf.Max(1e-6f, diameter / 2f); // evita 0

        // resistencia R = (8 η L) / (π r^4)
        float r4 = Mathf.Pow(radius, 4);
        float resistance = (8f * viscosity * length) / (Mathf.PI * Mathf.Max(r4, 1e-18f));

        // flujo volumétrico Q = ΔP / R
        float flow = 0f;
        if (Mathf.Abs(resistance) > Mathf.Epsilon)
            flow = deltaP / resistance;


        CurrentFlow = flow; // <-- Guardamos el Q


        // área
        float area = Mathf.PI * Mathf.Pow(radius, 2);

        // velocidad = Q / A
        float velocity = 0f;
        if (area > Mathf.Epsilon)
            velocity = flow / area;

        // si hay números extraños, clamp a 0 mínimo
        if (float.IsNaN(velocity) || float.IsInfinity(velocity))
            velocity = 0f;

        return velocity;
    }

    /// <summary>
    /// Calcula Xmin/Xmax del renderer en espacio local del objeto y los asigna al material.
    /// </summary>
    public void UpdateBoundsToMaterial()
    {
        if (_renderer == null || _localMaterial == null) return;

        Bounds worldB = _renderer.bounds; // en world space
        Vector3 localMin = transform.InverseTransformPoint(worldB.min);
        Vector3 localMax = transform.InverseTransformPoint(worldB.max);

        float xMin = Mathf.Min(localMin.x, localMax.x);
        float xMax = Mathf.Max(localMin.x, localMax.x);

        // Enviar al material
        _localMaterial.SetFloat(xMinProperty, xMin);
        _localMaterial.SetFloat(xMaxProperty, xMax);

        // Debug (opcional)
        //Debug.Log($"{name} → Bounds local X: min={xMin:F4}, max={xMax:F4}");
    }

    // Editor helper: recalcula bounds cuando cambias parámetros en el inspector (solo en Editor)
#if UNITY_EDITOR
    void OnValidate()
    {
        // Evitar ejecución en modo play al validar
        if (!Application.isPlaying)
        {
            // Intenta actualizar referencias y bounds para feedback en editor
            _renderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
            if (_renderer != null)
            {
                // No forzamos instanciado aquí (eso lo hacemos en Awake), pero si ya existen instancias actualizamos bounds
                if (_instances != null && _instances.Length > 0)
                {
                    int idx = Mathf.Clamp(bloodFlowMaterialIndex, 0, _instances.Length - 1);
                    if (_instances[idx] != null)
                    {
                        _instances[idx].SetFloat(epsilonProperty, epsilon);
                        // actualizar bounds en la instancia si posible
                        Bounds worldB = _renderer.bounds;
                        Vector3 localMin = transform.InverseTransformPoint(worldB.min);
                        Vector3 localMax = transform.InverseTransformPoint(worldB.max);
                        float xMin = Mathf.Min(localMin.x, localMax.x);
                        float xMax = Mathf.Max(localMin.x, localMax.x);
                        _instances[idx].SetFloat(xMinProperty, xMin);
                        _instances[idx].SetFloat(xMaxProperty, xMax);
                    }
                }
            }
        }
    }
#endif
}
