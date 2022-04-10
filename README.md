# NPCScheduleSystem
System developed in Unity to allow NPCs have a schedule and move through the world freely

## Features
- Very scalable and allows for changes to the NPCs at any point of the development
- NPCs can travel through scenes and be updated off-screen in a performant way
- Supports different waypoints (Idle, sitting, wander, perform an action)
- Easy map/waypoint creation

## Setting up a scene
The NPCScheduleSystem consists of 5 types of objects:
- SceneMapping: Contains information of the multiple areas and it's waypoints inside of a scene
- World: Contains a collection of SceneMappings that belong to a same zone and establishes the connections between them
- NPCCharacter: A simple container for the NPC specific info (such as name, group, 3D model, etc...)
- World NPCs: A list of the NPCs that populate a World
- Schedule: Container of the actions that each group or individual npc should perform at a given time

<img src="https://github.com/MrAlvaroRamirez/NPCScheduleSystem/blob/main/preview/objects.PNG" width="60%"/>

### Scene Maping
<img src="https://github.com/MrAlvaroRamirez/NPCScheduleSystem/blob/main/preview/waypoints.PNG" width="60%"/>

### World Connections
<img src="https://github.com/MrAlvaroRamirez/NPCScheduleSystem/blob/main/preview/connections.PNG" width="60%"/>

### Schedule
<img src="https://github.com/MrAlvaroRamirez/NPCScheduleSystem/blob/main/preview/schedule.PNG" width="40%"/>

### Example result
<img src="https://github.com/MrAlvaroRamirez/NPCScheduleSystem/blob/main/preview/final.gif" width="60%"/>
