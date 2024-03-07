## Physarum Maze Simulator
Physarum (slime mold) simulator that uses depth-first-search algorithm to randomly generate a maze for the slime to navigate through. It's built with Unity using C# and some HLSL .compute shaders that the GPU runs with parallelism to keep things running smoothly with a high amount of slime agents.

### Normal Mode
Straightforward physarum simulation 

### Maze Mode
A maze is randomly generated and the slime agents are dropped in to see how they handle it.

#### Instructions
Open the exe from the zip file in releases. Once the program runs, it will default to maze mode, hit "Esc" key to go back to the main menu where you can then select which mode you want to do.

<img src="https://github.com/hrblr/PhysarumMazeGenerator/blob/main/.vscode/Sim.PNG?raw=true">
<img src = "https://github.com/hrblr/PhysarumMazeGenerator/blob/main/.vscode/Capture1.PNG?raw=true">
