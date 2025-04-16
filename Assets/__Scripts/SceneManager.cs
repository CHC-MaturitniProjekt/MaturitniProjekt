using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStart : MonoBehaviour
{
    public void loadMainScene()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}
