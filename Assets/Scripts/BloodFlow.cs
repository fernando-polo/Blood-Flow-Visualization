using UnityEngine;

public class BloodFlow : MonoBehaviour
{
    public ParticleSystem bloodFlowParticles;
    public Transform stenosisSection;
    public Color normalFlowColor = Color.blue;
    public Color turbulentFlowColor = Color.red;

    private ParticleSystem.MainModule psMain;

    void Start()
    {
        psMain = bloodFlowParticles.main;
        psMain.startColor = normalFlowColor;
    }

    void Update()
    {
        if (stenosisSection.localScale.x < 0.4f)
        {
            psMain.startColor = turbulentFlowColor;
        }
        else
        {
            psMain.startColor = normalFlowColor;
        }
    }
}
