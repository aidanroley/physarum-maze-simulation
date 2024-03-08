using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
public class MazeRunner : MonoBehaviour
{
    private System.Random rnd = new System.Random(); //for maze generator
    public ComputeShader computeShader;
    
     int numAgents = 10000;
     int width = 2560;
      int height = 1440;
    public RawImage displayImage; 

    private ComputeBuffer agentsBuffer;
    private RenderTexture renderTexture;
    private RenderTexture trailTexture;
    private RenderTexture mazeTexture1;
    private RenderTexture mazeRead;
    private void Start()
    {
        computeShader.SetInt("numAgents", numAgents);
        computeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        computeShader.SetFloat("time", Time.fixedTime);
        computeShader.SetFloat("width", width);
        computeShader.SetFloat("height", height);
        // Adjust the size of the RawImage's RectTransform
        RectTransform rt = displayImage.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height); 

        InitializeMazeTexture();
        InitializeRenderTexture();
        InitializeTrailTexture();
        
        
        
        
        GenerateMaze();
        displayImage.texture = renderTexture;
        Graphics.Blit(mazeTexture1, renderTexture);
        
        spawnCenter();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Load the main menu scene
            SceneManager.LoadScene("Mainmenuscene"); 
        }
       CopyTrailToRenderTexture();
        int updateKernelID = computeShader.FindKernel("updateAgent");
        computeShader.SetTexture(updateKernelID, "TrailMap", trailTexture);
        computeShader.SetTexture(updateKernelID, "ReadTexture", mazeRead);
        computeShader.SetBuffer(updateKernelID, "AgentsOut", agentsBuffer);
        computeShader.Dispatch(updateKernelID, numAgents / 256, 1, 1);

        DrawAgentsOnTexture();
        CopyTrailToRenderTexture();
        UpdateTrailMap();  
        CopyTrailToRenderTexture();
        CopyTrailToRenderTexture();
       
    }

    void GenerateMaze() {
        int heightMaze = 15;
        int widthMaze = 29;
         Texture2D mazeTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
         Color[] pixels = new Color[width * height];
       
        //set all pixels to black by default
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i] = Color.black;
        }
        mazeTexture.SetPixels(pixels);

        // Set filter mode to Point for crisp pixel art
        mazeTexture.filterMode = FilterMode.Point; 

        //instantiate the arrays for wall pixels here
        bool[,] horizontalWalls = new bool[widthMaze + 1, heightMaze];
        bool[,] verticalWalls = new bool[widthMaze, heightMaze + 1];
        GenMaze(widthMaze, heightMaze, horizontalWalls, verticalWalls);

        for (int i = 0; i < heightMaze; i++) {
            for (int j = 0; j < widthMaze + 1; j++) {
                for (int k = 0; k < 80; k++) {
                    int xPos = j * 80 + k;
                    Color color = horizontalWalls[j, i] ? Color.yellow : Color.black; // Wall is black, no wall is white

                    for (int thickness = 0; thickness < 10; thickness++) {
                        mazeTexture.SetPixel(70 + xPos,80 + i * 80 + thickness, color);
                        if (i == 0 && j < widthMaze) {
                            mazeTexture.SetPixel(70 + xPos,80 + i * 80 + thickness, Color.yellow);
                        }
                        if (i == heightMaze - 1 && j < widthMaze) {
                            mazeTexture.SetPixel(70 + xPos,160 + i * 80 + thickness, Color.yellow);
                        }

                    }
                    
                }            
            }
        }
        for (int i = 0; i < heightMaze + 1; i++) {
            for (int j = 0; j < widthMaze; j++) {
                for (int k = 0; k < 90; k++) {
                    int yPos = i * 80 + k;
                    Color color = verticalWalls[j, i] ? Color.yellow : Color.black;
                    for (int thickness = 0; thickness < 10; thickness++) {   
                    mazeTexture.SetPixel(70 + j * 80 + thickness, 80 + yPos, color);
                    if (j == 0 && i < heightMaze) {
                        mazeTexture.SetPixel(70 + j * 80 + thickness, 80 + yPos, Color.yellow);
                    }
                    if (j == widthMaze - 1 && i < heightMaze) {
                        mazeTexture.SetPixel(140 + j * 80 + thickness, 80 + yPos, Color.yellow);
                    }
                    }
                }
                
            }
        }
        mazeTexture.Apply();
        Graphics.Blit(mazeTexture, mazeTexture1);
        Graphics.Blit(mazeTexture1, mazeRead);
        
        int mazeKernelID = computeShader.FindKernel("MazeGen");
        computeShader.SetTexture(mazeKernelID, "MazeTexture", mazeTexture1);
        computeShader.Dispatch(mazeKernelID, mazeTexture1.width / 8, mazeTexture1.height / 8, 1);
        
    }


    //instantiates the grid and calls the algorithm to generate the maze
    void GenMaze(int width, int height, bool[,] horizontalWalls, bool[,] verticalWalls) {
        bool[,] grid = new bool[width, height];
        (int x, int y) startVertex = (0, 0);

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                grid[i, j] = false;
            }
        }
        randomizedDFS(grid, startVertex, width, height, horizontalWalls, verticalWalls);


    }
    //main method/algorithm for generating the maze
    void randomizedDFS(bool[,] grid, (int x, int y) vertex, int width, int height, bool[,] horizontalWalls, bool[,] verticalWalls) {
        
        markVisited(grid, vertex);
        (int x, int y) nextVertex = randomUnvisitedNeighbor(grid, vertex, width, height);
         while (nextVertex.x != -1 && nextVertex.y != -1) {

            connectCells(vertex, nextVertex, horizontalWalls, verticalWalls);
            randomizedDFS(grid, nextVertex, width, height, horizontalWalls, verticalWalls);
            nextVertex = randomUnvisitedNeighbor(grid, vertex, width, height);
         }  
    }
    void connectCells((int x, int y) vertex, (int x, int y) nextVertex, bool[,] horizontalWalls, bool[,] verticalWalls) {
        
        if (nextVertex.x - vertex.x == 1) {
            verticalWalls[vertex.x + 1, vertex.y] = true;
        }
        else if (nextVertex.x - vertex.x == - 1) {
            verticalWalls[vertex.x, vertex.y] = true;
        }
        else if (nextVertex.y - vertex.y ==  1) {
            horizontalWalls[vertex.x, vertex.y + 1] = true;
        }
        else if (nextVertex.y - vertex.y == - 1) {
            horizontalWalls[vertex.x, vertex.y] = true;
        }
    }

    //pass a grid square in here to mark it as visited (to true)
    void markVisited(bool[,] grid, (int x, int y) vertex) {
        grid[vertex.x, vertex.y] = true;
        
    }

    //check boundaries then find a direction to go based on that, then check to make sure new box is false, then if true, go back to finding a new direction and repeat until false.
    (int x, int y)  randomUnvisitedNeighbor(bool[,] grid, (int x, int y) vertex, int width, int height) {
        (int x, int y) nextVertex = (vertex.x, vertex.y);
        bool found = false;
        //boundary checks, this holds the convention that (0,0) is the top left instead of Cartesian bottom left
        bool left = vertex.x > 0;
        bool right = vertex.x < width - 1;
        bool down = vertex.y < height - 1;
        bool up = vertex.y > 0;

        List<int> uncheckedDirections = new List<int> { 1, 2, 3, 4 };

        while (uncheckedDirections.Count > 0 && !found) {
            int index = rnd.Next(0, uncheckedDirections.Count);
            int randNum = uncheckedDirections[index];
            uncheckedDirections.RemoveAt(index);

            if (randNum == 1 && left) {
                if (!grid[vertex.x - 1, vertex.y]) {
                nextVertex = (vertex.x - 1, vertex.y);
                found = true;
                }
            }
            else if (randNum == 2 && right) {
                if (!grid[vertex.x + 1, vertex.y]) {
                nextVertex = (vertex.x + 1, vertex.y);
                found = true;
                }
            }
            else if (randNum == 3 && down) {
                if (!grid[vertex.x, vertex.y + 1]) {
                nextVertex = (vertex.x, vertex.y + 1);
                found = true;
                }
            }
            else if (randNum == 4 && up) {
                if (!grid[vertex.x, vertex.y - 1]) {
                nextVertex = (vertex.x, vertex.y - 1);
                found = true;
            }
        }
        
    }
    if (!found) {
        return (-1, -1);
    }
    return nextVertex; 
    }

    

    void Blur() {
    int blurKernelID = computeShader.FindKernel("BlurTrail");
    computeShader.SetTexture(blurKernelID, "TrailMap", trailTexture); 
    computeShader.SetTexture(blurKernelID, "ReadTexture", mazeRead);
    computeShader.SetTexture(blurKernelID, "Blur", renderTexture);
    computeShader.Dispatch(blurKernelID, renderTexture.width / 8, renderTexture.height / 8, 1);
}



void UpdateTrailMap() {
    int trailKernelID = computeShader.FindKernel("UpdateTrail");
    computeShader.SetTexture(trailKernelID, "ReadTexture", mazeRead);
    computeShader.SetTexture(trailKernelID, "TrailMap", renderTexture);
    computeShader.Dispatch(trailKernelID, width / 8, height / 8 , 1);
    


}
void DrawAgentsOnTexture()
{
    // Example of setting up and dispatching a drawing kernel
    int drawKernelID = computeShader.FindKernel("DrawAgent");
    computeShader.SetBuffer(drawKernelID, "AgentsOut", agentsBuffer);
    computeShader.SetTexture(drawKernelID, "Result", renderTexture);
    computeShader.SetTexture(drawKernelID, "ReadTexture", mazeRead);
    computeShader.Dispatch(drawKernelID, numAgents / 16, 1, 1);
}
void CopyTrailToRenderTexture()
{
    //this just copies the textures
    Graphics.Blit(renderTexture, trailTexture);
}

void InitializeMazeTexture()
{
    mazeTexture1 = new RenderTexture(width, height, 0)
    
    {
        enableRandomWrite = true,
        format = RenderTextureFormat.ARGBFloat
    };
    mazeRead = new RenderTexture(width, height, 0)
{
    enableRandomWrite = false, 
    format = RenderTextureFormat.ARGB32,
    useMipMap = false,
    autoGenerateMips = false, 
    filterMode = FilterMode.Bilinear, 
    wrapMode = TextureWrapMode.Repeat, 
};

    mazeTexture1.Create();
    mazeRead.Create();
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
        int randomInt = rnd.Next();     
        agentsBuffer = new ComputeBuffer(numAgents, sizeof(float) * 3);
        int kernelID = computeShader.FindKernel("spawnInCenter");
        computeShader.SetTexture(kernelID, "Result", mazeTexture1);
        computeShader.SetBuffer(kernelID, "AgentsOut", agentsBuffer);
        computeShader.SetInt("randomValue", randomInt);

        computeShader.Dispatch(kernelID, numAgents / 16, 1, 1);
        //displayImage.texture = renderTexture;

//this is just a test method to make sure my hash function worked properly      
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
