using UnityEngine;

public class BloodFlowManager : MonoBehaviour
{
    public ParticleSystem bloodParticles;
    public Material arteryMat;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Stenosis_Section")
        {
            // Cambiar partículas a rojo turbulento
            var main = bloodParticles.main;
            main.startColor = Color.red;

            // Cambiar material interior a rojo más intenso
            arteryMat.color = new Color(1f, 0f, 0f, 0.5f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Stenosis_Section")
        {
            // Regresar partículas a azul normal
            var main = bloodParticles.main;
            main.startColor = Color.blue;

            // Regresar material interior a rojo suave
            arteryMat.color = new Color(0.8f, 0f, 0f, 0.4f);
        }
    }
}
