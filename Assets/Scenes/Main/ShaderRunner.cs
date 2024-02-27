using UnityEngine;
using UnityEngine.UI; // Add this to access UI elements

public class ShaderRunner : MonoBehaviour
{
    public ComputeShader computeShader;
    //computeShader.SetFloat("deltaTime", Time.deltaTime);
     public int numAgents = 50;
    public RawImage displayImage; // Assign this in the Inspector

    private ComputeBuffer agentsBuffer;
    private RenderTexture renderTexture;
    private void Start()
    {
        // Adjust the size of the RawImage's RectTransform
        RectTransform rt = displayImage.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2560, 1440); // Set the size to 1920x1080

        InitializeRenderTexture();
        displayImage.texture = renderTexture;

        spawnCenter();
    }
    void Update()
    {
       
        int updateKernelID = computeShader.FindKernel("updateAgent");
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetBuffer(updateKernelID, "AgentsOut", agentsBuffer);
        computeShader.Dispatch(updateKernelID, agentsBuffer.count / 256, 1, 1);

        DrawAgentsOnTexture();
       
    }

void DrawAgentsOnTexture()
{
    // Example of setting up and dispatching a drawing kernel
    int drawKernelID = computeShader.FindKernel("DrawAgent");
    computeShader.SetBuffer(drawKernelID, "AgentsOut", agentsBuffer);
    computeShader.SetTexture(drawKernelID, "Result", renderTexture);
    computeShader.Dispatch(drawKernelID, renderTexture.width / 8, renderTexture.height / 8, 1);
}

    void InitializeRenderTexture()
{
    int width = 2560;
    int height = 1440;
    renderTexture = new RenderTexture(width, height, 24)
    {
        enableRandomWrite = true,
        format = RenderTextureFormat.ARGBFloat
    };
    renderTexture.Create();
}
    void spawnCenter() {


        agentsBuffer = new ComputeBuffer(numAgents, sizeof(float) * 3);
        int kernelID = computeShader.FindKernel("spawnInCenter");
        computeShader.SetBuffer(kernelID, "AgentsOut", agentsBuffer);
        computeShader.SetInt("numAgents", numAgents);

        int threadGroupsX = Mathf.CeilToInt(numAgents / 8.0f); // Adjust based on your numthreads x dimension.
        computeShader.Dispatch(kernelID, threadGroupsX, 1, 1);
        //displayImage.texture = renderTexture;
        if (agentsBuffer == null) {
        agentsBuffer = new ComputeBuffer(numAgents, sizeof(float) * 3);
    }
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
