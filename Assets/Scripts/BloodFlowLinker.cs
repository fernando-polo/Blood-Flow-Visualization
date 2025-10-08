using UnityEngine;

[DisallowMultipleComponent]
public class BloodFlowLinker : MonoBehaviour
{
    [Header("Referencia al BloodFlowController principal")]
    public BloodFlowController mainFlow;

    [Header("Prefabs a sincronizar")]
    public TurbulenceShaderDebugger turbulenceScript;
    public PressureShaderDebugger pressureScript;

    [Header("Opciones de debug")]
    public bool debugLogs = true;

    void Awake()
    {
        // Autoasignar si no se han arrastrado
        if (mainFlow == null)
        {
            mainFlow = Object.FindFirstObjectByType<BloodFlowController>();
            if (debugLogs)
            {
                if (mainFlow != null) Debug.Log($"[BloodFlowLinker] mainFlow asignado automáticamente a {mainFlow.name}");
                else Debug.LogWarning("[BloodFlowLinker] No se encontró BloodFlowController en la escena.");
            }
        }

        if (turbulenceScript == null)
            turbulenceScript = FindFirstObjectByType<TurbulenceShaderDebugger>();

        if (pressureScript == null)
            pressureScript = FindFirstObjectByType<PressureShaderDebugger>();
            
    }

    void Update()
    {
        if (mainFlow == null) return;

        // 1️⃣ Sincronizar Turbulence
        if (turbulenceScript != null)
        {
            turbulenceScript.pressureIn = mainFlow.pressureIn;
            turbulenceScript.pressureOut = mainFlow.pressureOut;
            turbulenceScript.length = mainFlow.length;
            turbulenceScript.diameter = mainFlow.diameter;
            turbulenceScript.viscosity = mainFlow.viscosity;

            turbulenceScript.UpdateTurbulenceValues();
        }

        // 2️⃣ Sincronizar Pressure
        if (pressureScript != null)
        {
            pressureScript.pressureIn = mainFlow.pressureIn;
            pressureScript.pressureOut = mainFlow.pressureOut;
            pressureScript.length = mainFlow.length;

            pressureScript.UpdatePressureValues();
        }

        // 3️⃣ Debug log opcional
        if (debugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[BloodFlowLinker] Valores sincronizados: ΔP={mainFlow.pressureIn - mainFlow.pressureOut} mmHg, Length={mainFlow.length} m, Diameter={mainFlow.diameter} m");
        }
    }
}
