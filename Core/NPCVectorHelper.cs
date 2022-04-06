using UnityEngine;

namespace NPC {
    public static class NPCVectorHelper
    {
        //Code by: khemistry (https://forum.unity.com/threads/get-a-point-partway-along-a-navmeshagents-path.458353/)
        public static Vector3 FindPointAlongPath(Vector3[] path, float distanceToTravel)
        {
            if(distanceToTravel < 0)
            {
                return path[0];
            }
    
            //Loop Through Each Corner in Path
            for (int i = 0; i < path.Length -1; i++)
            {
                //If the distance between the next to points is less than the distance you have left to travel
                if (distanceToTravel <= Vector3.Distance(path[i], path[i + 1]))
                {
                    //Calculate the point that is the correct distance between the two points and return it
                    Vector3 directionToTravel = path[i + 1] - path[i];
                    directionToTravel.Normalize();
                    return (path[i] + (directionToTravel * distanceToTravel));
                }
                else
                {
                    //otherwise subtract the distance between those 2 points from the distance left to travel
                    distanceToTravel -= Vector3.Distance(path[i], path[i + 1]);
                }
            }
    
            //if the distance to travel is greater than the distance of the path, return the final point
            return path[path.Length - 1];
        }
    }
}