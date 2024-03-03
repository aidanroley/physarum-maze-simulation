using UnityEngine;
using UnityEngine.UI; // Add this to access UI elements

public class MazeRunner : MonoBehaviour
{
    public ComputeShader computeShader;
    
     int numAgents = 10000;
     int width = 2560;
      int height = 1440;
    public RawImage displayImage; // Assign this in the Inspector

    private ComputeBuffer agentsBuffer;
    private RenderTexture renderTexture;
    private RenderTexture trailTexture;
    private RenderTexture mazeTexture;
    private void Start()
    {
        computeShader.SetInt("numAgents", numAgents);
        computeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        computeShader.SetFloat("time", Time.fixedTime);
        computeShader.SetFloat("width", width);
        computeShader.SetFloat("height", height);
        // Adjust the size of the RawImage's RectTransform
        RectTransform rt = displayImage.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height); // Set the size to 1920x1080

        InitializeMazeTexture();
        InitializeRenderTexture();
        InitializeTrailTexture();
        
        displayImage.texture = mazeTexture;
        
        
        GenerateMaze();
        
        spawnCenter();
    }
    void Update()
    {
        
       CopyTrailToRenderTexture();
        int updateKernelID = computeShader.FindKernel("updateAgent");
        computeShader.SetTexture(updateKernelID, "TrailMap", trailTexture);
        computeShader.SetBuffer(updateKernelID, "AgentsOut", agentsBuffer);
        computeShader.Dispatch(updateKernelID, numAgents / 256, 1, 1);

        DrawAgentsOnTexture();
        CopyTrailToRenderTexture();
        UpdateTrailMap();  
        CopyTrailToRenderTexture();
       Blur(); 
       CopyTrailToRenderTexture();
       
    }

    void GenerateMaze() {
        int mazeKernelID = computeShader.FindKernel("MazeGen");
        computeShader.SetTexture(mazeKernelID, "MazeTexture", mazeTexture);
        computeShader.Dispatch(mazeKernelID, width / 8, height / 8, 1);
    }

    void GenMaze(width, height) {
        startVertex = (0, height);
        randomizedDFS():


    }

    void randomizedDFS(vertex) {
        
    }
    void Blur() {
    int blurKernelID = computeShader.FindKernel("BlurTrail");
    computeShader.SetTexture(blurKernelID, "TrailMap", trailTexture); 
    computeShader.SetTexture(blurKernelID, "Blur", renderTexture);
    computeShader.Dispatch(blurKernelID, renderTexture.width / 8, renderTexture.height / 8, 1);
}



void UpdateTrailMap() {
    int trailKernelID = computeShader.FindKernel("UpdateTrail");
    computeShader.SetTexture(trailKernelID, "TrailMap", renderTexture);
    computeShader.Dispatch(trailKernelID, width / 8, height / 8 , 1);
    


}
void DrawAgentsOnTexture()
{
    // Example of setting up and dispatching a drawing kernel
    int drawKernelID = computeShader.FindKernel("DrawAgent");
    computeShader.SetBuffer(drawKernelID, "AgentsOut", agentsBuffer);
    computeShader.SetTexture(drawKernelID, "Result", renderTexture);
    computeShader.Dispatch(drawKernelID, numAgents / 16, 1, 1);
}
void CopyTrailToRenderTexture()
{
    // If using a compute shader for copying, set up and dispatch it here.
    // For simplicity, here's how you could do it with Graphics.Blit():
    Graphics.Blit(renderTexture, trailTexture);
}

void InitializeMazeTexture()
{
    mazeTexture = new RenderTexture(width, height, 0)
    {
        enableRandomWrite = true,
        format = RenderTextureFormat.ARGBFloat
    };
    mazeTexture.Create();
}
    void InitializeRenderTexture()
{
    renderTexture = new RenderTexture(width, height, 0)
    {
        enableRandomWrite = true,
        format = RenderTextureFormat.ARGBFloat
    };
    renderTexture.Create();
}
    void InitializeTrailTexture()
{
    trailTexture = new RenderTexture(width, height, 0)
    {
        enableRandomWrite = true,
        format = RenderTextureFormat.ARGBFloat
    };
    trailTexture.Create();
}
    void spawnCenter() {


        agentsBuffer = new ComputeBuffer(numAgents, sizeof(float) * 3);
        int kernelID = computeShader.FindKernel("spawnInCenter");
        computeShader.SetBuffer(kernelID, "AgentsOut", agentsBuffer);

        computeShader.Dispatch(kernelID, numAgents / 16, 1, 1);
        //displayImage.texture = renderTexture;
       
void Noise() {
        
        int width = 2560;
        int height = 1440;
        RenderTexture renderTexture = new RenderTexture(width, height, 24)
        
        {
            enableRandomWrite = true,
            format = RenderTextureFormat.ARGBFloat
        };
        renderTexture.Create();

        int kernelID = computeShader.FindKernel("CSMain");

        computeShader.SetTexture(kernelID, "Result", renderTexture);
        computeShader.Dispatch(kernelID, Mathf.CeilToInt((float)width / 8f), Mathf.CeilToInt((float)height / 8f), 1);



        displayImage.texture = renderTexture; // Set the RenderTexture as the RawImage texture
    }

    }
private void OnDestroy()
    {
        // Release the ComputeBuffer when the GameObject is destroyed
        if (agentsBuffer != null)
        {
            agentsBuffer.Release();
        }
    }


   
}
