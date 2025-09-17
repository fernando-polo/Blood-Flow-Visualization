using UnityEngine;

public class BloodFlowController : MonoBehaviour
{
    [Header("Propiedades físicas del vaso")]
    public float pressureIn = 100f;
    public float pressureOut = 98f;
    public float length = 0.25f;
    public float diameter = 0.023f;
    public float viscosity = 0.0035f;

    [Header("Material del Shader Graph")]
    public Material flowMaterial;

    [Header("Propiedad en Shader Graph")]
    public string flowSpeedProperty = "_FlowSpeed"; // revisa el Reference exacto en Shader Graph

    private Material localMaterial;

    void Start()
    {
        if (flowMaterial != null)
        {
            // Crear una instancia del material para este objeto
            localMaterial = new Material(flowMaterial);

            // Buscar Renderer en el objeto o en sus hijos
            var renderer = GetComponent<Renderer>();
            if (renderer == null)
                renderer = GetComponentInChildren<Renderer>();

            if (renderer != null)
            {
                renderer.material = localMaterial;
                Debug.Log($"{name} → Material instanciado asignado correctamente");
            }
            else
            {
                Debug.LogWarning($"{name} → No se encontró Renderer");
            }
        }
        else
        {
            Debug.LogWarning($"{name} → No se asignó flowMaterial en el Inspector");
        }
    }

    void Update()
    {
        if (localMaterial == null) return;

        // ΔP en Pascales (1 mmHg ≈ 133.322 Pa)
        float deltaP = (pressureIn - pressureOut) * 133.322f;

        // Radio del vaso (m)
        float radius = diameter / 2f;

        // Resistencia R = (8 * η * L) / (π * r^4)
        float resistance = (8f * viscosity * length) / (Mathf.PI * Mathf.Pow(radius, 4));

        // Flujo volumétrico Q = ΔP / R (m^3/s)
        float flow = deltaP / resistance;

        // Velocidad promedio V = Q / Área
        float area = Mathf.PI * Mathf.Pow(radius, 2);
        float velocity = flow / area; // m/s

        // Enviar al shader
        localMaterial.SetFloat(flowSpeedProperty, velocity);

        // Debug de velocidad
        Debug.Log($"{name} → Velocity enviada al shader: {velocity:F6}");
    }
}
