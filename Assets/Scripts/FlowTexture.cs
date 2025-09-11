using UnityEngine;

public class FlowTexture : MonoBehaviour
{
    public Material arteryMat;   // aquí arrastraremos el material
    public float flowSpeed = 0.2f;

    void Update()
    {
        if (arteryMat != null)
        {
            Vector2 offset = arteryMat.mainTextureOffset;
            offset.y += flowSpeed * Time.deltaTime; // desplaza la textura en Y
            arteryMat.mainTextureOffset = offset;
        }
    }
}
