using UnityEngine;

public class MazeObjectActivator : MonoBehaviour
{
    public void ActivateGameObject(GameObject Maze)
    {
        Maze.SetActive(true);
    }

    public void DeactivateGameObject(GameObject Maze)
    {
        Maze.SetActive(false);
    }
}
