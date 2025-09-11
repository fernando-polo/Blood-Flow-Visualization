using UnityEngine;

public class ParticleColorBySpeed : MonoBehaviour
{
    ParticleSystem ps;
    ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void LateUpdate()
    {
        int count = ps.GetParticles(particles);
        for (int i = 0; i < count; i++)
        {
            float speed = particles[i].velocity.magnitude;
            // Normaliza velocidad a rango 0–1
            float t = Mathf.InverseLerp(1f, 10f, speed); 
            // Azul = velocidad baja, Rojo = velocidad alta
            Color color = Color.Lerp(Color.blue, Color.red, t);
            particles[i].startColor = color;
        }
        ps.SetParticles(particles, count);
    }
}
