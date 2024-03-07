using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject Maze;
    public GameObject MainSimulation;

    public void MazeSim()
    {
        Maze.SetActive(true);
        MainSimulation.SetActive(false);
    }

    public void NormalSim()
    {
        Maze.SetActive(false);
        MainSimulation.SetActive(true);
    }
}
