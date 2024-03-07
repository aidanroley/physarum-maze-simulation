using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string NormalScene)
    {
        SceneManager.LoadScene(NormalScene);
    }
}
