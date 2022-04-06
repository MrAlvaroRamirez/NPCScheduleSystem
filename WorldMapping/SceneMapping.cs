using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC {
    [CreateAssetMenu(menuName = "NPC/Scene Mapping")]
    public class SceneMapping : ScriptableObject
    {
        public string MapName;
        public List<MapArea> Areas = new List<MapArea>();
        private MapArea.Waypoint _selectedWaypoint = null;
        public MapArea.Waypoint selectedWaypoint
        {
            get{return _selectedWaypoint;}
            set{_selectedWaypoint = value;}
        }
        private MapArea _selectedArea = null;
        public MapArea selectedArea
        {
            get{return _selectedArea;}
            set{_selectedArea = value;}
        }

        // Here you store the actual instance
        private static MapArea _disabledArea;
        public static MapArea DisabledArea
        {
            get
            {
                if(_disabledArea != null) return _disabledArea;

                // Otherwise create an instance now 
                _disabledArea = new MapArea("Disabled");
                return _disabledArea;
            }
        }

        public MapArea GetArea(string AreaName)
        {
            //TODO: Performance
            var area = Areas.Find((x) => x.AreaName == AreaName);

            if(area == null)
            {
                return null;
            } else 
            {
                return area;
            }
        }
    }

    [System.Serializable]
    public class MapArea : IEquatable<MapArea>
    {
        public string AreaName;
        public Color AreaColor = Color.red;
        public List<Waypoint> Waypoints = new List<Waypoint>();
        public SceneMapping SceneMap;

        [System.Serializable]
        public class Waypoint
        {
            public Vector3 position;
            public Vector3 rotation;
            public WaypointState State;
            public float AreaRadius = 10;
            public int MaxNPCS = 5;
            public AnimationClip Animation = null;

            public Waypoint (Vector3 pos, Vector3 rot, float rad = 10, WaypointState state = WaypointState.Default, int max = 5)
            {
                position = pos;
                rotation = rot;
                AreaRadius = rad;
                MaxNPCS = max;
                State = state;
            }
        }

        public MapArea(string name)
        {
            AreaName = name;
        }

        public MapArea(SceneMapping mapping)
        {
            SceneMap = mapping;
        }

        public bool Equals(MapArea other)
        {
            return AreaName == other.AreaName;
        }
    }

    public enum WaypointState {Default, Area, Match, Seat}
}