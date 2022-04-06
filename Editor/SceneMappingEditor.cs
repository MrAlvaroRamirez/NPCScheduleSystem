using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NPC {
    [CustomEditor(typeof(SceneMapping))]
    public class SceneMappingEditor : Editor
    {
        SceneMapping _sceneMapping;
        bool _showWaypoints = false;
        bool ShowWaypoints {
            get{
                return _showWaypoints;
            }
            set{
                _showWaypoints = value;
                SceneView.RepaintAll();
            }
        }
        Dictionary<MapArea, Color> _hiddenColors = new Dictionary<MapArea, Color>();
        GUISkin _areaStyle = null;
    
        void OnEnable() {
            _sceneMapping = (SceneMapping) target;
            _sceneMapping.selectedWaypoint = null;
            _sceneMapping.selectedArea = null;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            _sceneMapping.selectedWaypoint = null;
            _sceneMapping.selectedArea = null;
        }

        void InitStyle()
        {
            if(_areaStyle == null)
            {
                _areaStyle = EditorGUIUtility.Load("mapSkin.guiskin") as GUISkin;
            }
        }

        public override void OnInspectorGUI()
        {
            InitStyle();

            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("MapName"));

            ShowWaypoints = GUILayout.Toggle(ShowWaypoints, "Show Waypoints");

            if(GUILayout.Button("Populate New Area"))
            {
                if(Selection.transforms.Length <= 0)
                {
                    Debug.LogError("No transform selected");
                } else
                {
                    var newArea = new MapArea(_sceneMapping);
                    newArea.AreaName = "Area " + _sceneMapping.Areas.Count;

                    //Populate with waypoints
                    foreach (var waypoint in Selection.transforms)
                    {
                        if(!waypoint.TryGetComponent<WaypointInfo>(out var wpComp))
                        {
                            Debug.LogError("Couldn't find Waypoint Info on the seleced Waypoint");
                            continue;
                        }

                        var wp = new MapArea.Waypoint(waypoint.position, waypoint.rotation.eulerAngles);
                        if(wpComp.MatchTransform)
                        {
                            wp.State = WaypointState.Match;
                            wp.Animation = wpComp.Animation ?? null;
                        } else if (wpComp.IsSeat)
                        {
                            wp.State = WaypointState.Seat;
                        }

                        newArea.Waypoints.Add(wp);
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                    _sceneMapping.Areas.Add(newArea);

                    //EditorUtility.SetDirty(target);
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Areas"));
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI(SceneView sceneView) {
            if(_showWaypoints)
            {
                //Disable scene selection
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));


                foreach (var area in _sceneMapping.Areas)
                {
                    List<Vector3> points = new List<Vector3>();
                    int count = 0;

                    GUIStyle labelStyle = new GUIStyle();
                    labelStyle.normal.textColor = area.AreaColor;
                    labelStyle.fontStyle = FontStyle.Bold;
                    labelStyle.alignment = TextAnchor.MiddleCenter;

                    foreach (var waypoint in area.Waypoints)
                    {
                        points.Add(waypoint.position);

                        //Generate dict
                        if (!_hiddenColors.TryGetValue(area, out Color handleColor))
                        {
                            handleColor = area.AreaColor;
                            handleColor.a = 0.4f;
                            _hiddenColors.Add(area, handleColor);
                        }

                        if(NPCEditorHelper.DrawWaypoint(waypoint, count, area.AreaColor, handleColor, labelStyle))
                        {
                            _sceneMapping.selectedWaypoint = waypoint;
                            _sceneMapping.selectedArea = area;
                        }

                        count++;
                    }

                    //Title
                    var averagePosition = points.Aggregate(Vector3.zero, (acc, v) => acc + v) / points.Count;
                    averagePosition += Vector3.up * 2;
                    Handles.Label(averagePosition, area.AreaName, _areaStyle.button);
                }

                if(_sceneMapping.selectedWaypoint != null)
                {
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;


                    if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if(GUIUtility.hotControl != 0)
                        {
                            _sceneMapping.selectedWaypoint = null;
                            _sceneMapping.selectedArea = null;
                            return;
                        }
                    }


                    EditorGUI.BeginChangeCheck();
                    Vector3 pos = Handles.PositionHandle(_sceneMapping.selectedWaypoint.position, Quaternion.Euler(_sceneMapping.selectedWaypoint.rotation));
                    Quaternion rot = Quaternion.identity;
                    
                    switch (_sceneMapping.selectedWaypoint.State)
                    {
                        case WaypointState.Default:
                            rot = Handles.Disc(Quaternion.Euler(_sceneMapping.selectedWaypoint.rotation), _sceneMapping.selectedWaypoint.position, Vector3.up, 1, true, 0);
                            break;
                        case WaypointState.Area:
                            break;
                        case WaypointState.Match:
                            rot = Handles.RotationHandle(Quaternion.Euler(_sceneMapping.selectedWaypoint.rotation), _sceneMapping.selectedWaypoint.position);
                            break;
                        case WaypointState.Seat:
                            rot = Handles.RotationHandle(Quaternion.Euler(_sceneMapping.selectedWaypoint.rotation), _sceneMapping.selectedWaypoint.position);
                            break;
                    }

                    if(EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Changed Waypoint");
                        _sceneMapping.selectedWaypoint.position = pos;
                        _sceneMapping.selectedWaypoint.rotation = rot.eulerAngles;
                    }
                }

                //Duplicate Waypoint
                if( Event.current.type == EventType.KeyDown ) {
                    if( Event.current.control && Event.current.keyCode == KeyCode.D ) {
                        var wp = new MapArea.Waypoint(_sceneMapping.selectedWaypoint.position, _sceneMapping.selectedWaypoint.rotation, _sceneMapping.selectedWaypoint.AreaRadius)
                        {
                            State = _sceneMapping.selectedWaypoint.State,
                            Animation = _sceneMapping.selectedWaypoint.Animation
                        };
                        
                        _sceneMapping.selectedArea.Waypoints.Add(wp);
                    }
                }
            }
        }

        [CustomPropertyDrawer(typeof(MapArea.Waypoint))]
        public class WaypointAttributeDrawer : PropertyDrawer
        {
            private static GUIStyle ToggleButtonStyleNormal = null;
            private static GUIStyle ToggleButtonStyleToggled = null;
            SceneMapping mapping = null;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                if(mapping == null)
                {
                    mapping = (SceneMapping)property.serializedObject.targetObject;
                }

                if ( ToggleButtonStyleNormal == null )
                {
                    ToggleButtonStyleNormal = "Button";
                    ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
                    ToggleButtonStyleToggled.normal.background = new Texture2D(2,2);
                }


                position.height = 16f;
                property.isExpanded = EditorGUI.Foldout (position, property.isExpanded, label);
                
                if(property.isExpanded)
                {
                    SerializedProperty pos = property.FindPropertyRelative("position");
                    SerializedProperty rot = property.FindPropertyRelative("rotation");
                    SerializedProperty rad = property.FindPropertyRelative("AreaRadius");
                    SerializedProperty state = property.FindPropertyRelative("State");
                    SerializedProperty max = property.FindPropertyRelative("MaxNPCS");

                    EditorGUI.PropertyField(new Rect( position.x, position.y + 18, position.width, 16), state);
                    EditorGUI.PropertyField(new Rect( position.x, position.y + 40, position.width, 16), pos, GUIContent.none);


                    if(state.enumValueIndex == 1)
                    {
                        EditorGUI.PropertyField(new Rect( position.x, position.y + 62, position.width, 20), rad, GUIContent.none);
                        EditorGUI.PropertyField(new Rect( position.x, position.y + 84, position.width, 20), max, GUIContent.none);
                    } else
                    {
                        EditorGUI.PropertyField(new Rect( position.x, position.y + 62, position.width, 16), rot, GUIContent.none);
                    }

                    if(state.enumValueIndex == 2)
                    {
                        SerializedProperty animation = property.FindPropertyRelative("Animation");

                        EditorGUI.PropertyField(new Rect( position.x, position.y + 62, position.width, 16), rot, GUIContent.none);
                        EditorGUI.PropertyField(new Rect( position.x, position.y + 82, position.width, 20), animation, GUIContent.none);
                    }
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return property.isExpanded ? EditorGUI.GetPropertyHeight(property,true) : 16;
            }
        }
    }
}