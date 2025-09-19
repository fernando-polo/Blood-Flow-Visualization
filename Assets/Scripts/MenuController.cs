using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Cada método carga la escena correspondiente
    public void LoadScene_AortaEstenosisSevera()
    {
        SceneManager.LoadScene("Aorta_EstenosisSevera");
    }

    public void LoadScene_AortaEstenosisLeve()
    {
        SceneManager.LoadScene("Aorta_EstenosisLeve");
    }

    public void LoadScene_AortaSana()
    {
        SceneManager.LoadScene("Aorta_Sana");
    }

    public void LoadScene_AortaPolicitemia()
    {
        SceneManager.LoadScene("Aorta_Policitemia");
    }

    // Método para volver al menú (por si lo usas en las otras escenas)
    public void LoadScene_Menu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    // Método para salir de la aplicación
    public void QuitApplication()
    {
        Application.Quit();
    }
}
