using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC {
    [CreateAssetMenu(menuName = "NPC/World")]
    public class WorldObject : ScriptableObject
    {
        public Scopes Scope;
        public List<SceneMapping> Mappings = new List<SceneMapping>();
        public List<Connection> Connections = new List<Connection>();
    }

    [System.Serializable]
    public class Connection
    {
        public SceneMapping From;
        public SceneMapping To;
        //TODO: Waypoints -> lists
        public MapArea.Waypoint FromLink;
        public MapArea.Waypoint ToLink;
    }

    public enum Scopes
    {
        Classroom
    }
}