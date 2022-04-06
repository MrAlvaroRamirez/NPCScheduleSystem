using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace NPC
{
    public static class NPCEditorHelper
    {
        public static bool DrawWaypoint(Vector3 position)
        {
            var wp = new MapArea.Waypoint(position, Vector3.zero, 0);
            return DrawWaypoint(wp, -1, Color.red, Color.red, GUI.skin.label);
        }

        public static bool DrawWaypoint(MapArea.Waypoint waypoint, int index, Color showColor, Color hideColor, GUIStyle style)
        {
            Handles.color = showColor;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            if(waypoint.State == WaypointState.Area)
                waypoint.AreaRadius = Handles.RadiusHandle(Quaternion.identity, waypoint.position, waypoint.AreaRadius);

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            if(waypoint.State == WaypointState.Default)
                Handles.DrawLine(waypoint.position, waypoint.position + (Vector3.down * 3));

            //area.AreaName, GUI.skin.textField
            if(Handles.Button(waypoint.position, Quaternion.identity, .3f, .4f, Handles.SphereHandleCap))
            {
                return true;
            
            }
            
            Handles.ArrowHandleCap(0, waypoint.position, Quaternion.Euler(waypoint.rotation), .5f, EventType.Repaint);

            //Number
            if(index != -1)
                Handles.Label(waypoint.position + Vector3.up, index.ToString(), style);

            
            Handles.color = hideColor;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;

            if(waypoint.State == WaypointState.Default)
                Handles.DrawLine(waypoint.position, waypoint.position + (Vector3.down * 3));

            if(Handles.Button(waypoint.position, Quaternion.identity, .3f, .4f, Handles.SphereHandleCap))
            {
                return true;
            }
            //Number
            if(index != -1)
                Handles.Label(waypoint.position + Vector3.up, index.ToString(), style);

            return false;
        }

        public static T GetFieldByName<T>(string fieldName, BindingFlags bindingFlags, object obj)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(fieldName, bindingFlags);
                
            if(fieldInfo == null)
                return default(T);
                
            return (T)fieldInfo.GetValue (obj);
        }
    }
}
