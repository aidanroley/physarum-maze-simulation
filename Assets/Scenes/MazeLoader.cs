using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeSceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string MazeScene)
    {
        SceneManager.LoadScene(MazeScene);
    }
}
