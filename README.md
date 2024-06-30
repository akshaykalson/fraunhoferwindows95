Demonstration of the application:



**PipeManagerFinal Unity Script**
This README will guide you through setting up the PipeManagerFinal script in Unity. The script is used to generate a series of interconnected pipe segments and bends dynamically in a game scene.

**Prerequisites**
Unity (version 2020.3 or later recommended)
Basic understanding of Unity Editor and C# scripting

**Setup Instructions**
-Create Unity Project
-Open Unity Hub and create a new 3D project. Name it appropriately (e.g., PipeManagerProject).
-Create Prefabs
-Create Cylinder Prefab for Straight Pipes
-In the Unity Editor, go to GameObject > 3D Object > Cylinder.
-Rename this cylinder to PipePrefab.
-Adjust the cylinder's dimensions to represent the pipe segment:
-Set the Scale in the Transform component to X: 1, Y: 0.5, Z: 1 (or as needed to fit your design).
-Right-click on the PipePrefab in the Hierarchy and select Prefab > Create Prefab to save it as a prefab in your Assets folder.
-Delete the PipePrefab from the Hierarchy to keep your scene clean.
-Create Sphere Prefab for Bends

-Go to GameObject > 3D Object > Sphere.
-Rename this sphere to BendPrefab.
-Adjust the sphere's scale as needed to fit your design.
-Right-click on the BendPrefab in the Hierarchy and select Prefab > Create Prefab to save it as a prefab in your Assets folder.
-Delete the BendPrefab from the Hierarchy to keep your scene clean.
-Create and Attach Script

-Create a New Script

-In the Assets folder, right-click and select Create > C# Script.
-Name the script PipeManagerFinal.
-Add the Script to a GameObject

-Create an empty GameObject in your scene by going to GameObject > Create Empty.
-Rename this GameObject to PipeManagerFinal.
-Drag the PipeManagerFinal script from the Assets folder onto the PipeManagerFinal GameObject to attach it.
-Configure the Script

-Assign Prefabs to the Script

-Select the PipeManagerFinal GameObject in the Hierarchy.
-In the Inspector window, you will see the PipeManagerFinal script component.
-Drag the PipePrefab from the Assets folder to the Pipe Prefab field in the Inspector.
-Drag the BendPrefab from the Assets folder to the Bend Prefab field in the Inspector.
-Adjust Script Parameters

-Set the Spawn Rate field to your desired value (e.g., 0.01 for frequent spawning).
-Set the Pipe Length to your desired segment length (e.g., 0.5).
-Configure the Pipe Colors array with colors for the pipes by clicking the small circle next to the Pipe Colors field and selecting the colors you want.
-Adjust Min Straight Segments to control how many straight segments are needed before a bend is placed.
-Set the Collision Threshold to control how close new pipe segments can be to existing ones (e.g., 0.1).
-Run the Scene

Click the Play button in the Unity Editor to start the scene.
Observe the dynamic generation of pipes and bends as specified in the PipeManagerFinal script.
