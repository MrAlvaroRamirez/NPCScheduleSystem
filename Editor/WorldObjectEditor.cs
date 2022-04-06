using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NPC {
    [CustomEditor(typeof(WorldObject))]
    public class WorldObjectEditor : Editor
    {
        WorldObject _world;
        static GUISkin _customStyle = null;
        static string[] _mappings = {}; //autogenerate
        static Vector3 _selectedWaypointPos;
        static string _selectedWaypointPath;

        void OnEnable() {
            _world = (WorldObject) target;
            _selectedWaypointPath = "";
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void InitStyle()
        {
            if(_customStyle == null)
            {
                _customStyle = EditorGUIUtility.Load("mapSkin.guiskin") as GUISkin;

                RefreshMappings();
            }
        }

        void RefreshMappings()
        {
            if(_world.Mappings.Count != 0)
            {
                //Replace array
                var newMappings = new string[_world.Mappings.Count];
                for (int i = 0; i < _world.Mappings.Count; i++)
                {
                    if(_world.Mappings[i] == null) continue;
                    newMappings[i] = _world.Mappings[i].MapName;
                } 

                _mappings = newMappings;
            }
        }

        public override void OnInspectorGUI()
        {
            InitStyle();

            //serializedObject.Update();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Scope"));
            
            //If mappings change, update our array
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Mappings"));
            if (EditorGUI.EndChangeCheck())
            {
                RefreshMappings();
            }

            if(_world.Mappings.Count == 0)
            {
                EditorGUILayout.LabelField("You need to setup at least one Mapping!", _customStyle.customStyles[0]);
            } else 
            {
                if(_world.Mappings[0] != null)
                {
                    //If connections change, fill empty fields
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Connections"));
                    if (EditorGUI.EndChangeCheck())
                    {
                        FillConnections();
                    }
                } else 
                {
                    EditorGUILayout.LabelField("Mapping cannot be null!", _customStyle.customStyles[0]);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI(SceneView sceneView) {
            if(_selectedWaypointPath != "")
            {
                NPCEditorHelper.DrawWaypoint(_selectedWaypointPos);
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

                EditorGUI.BeginChangeCheck();
                _selectedWaypointPos = Handles.PositionHandle(_selectedWaypointPos, Quaternion.identity);
                if(EditorGUI.EndChangeCheck())
                {
                    Repaint();
                }
            }
        }

        private void FillConnections()
        {
            if(_world.Connections.Count == 0) return;

            foreach(Connection con in _world.Connections)
            {
                if(con.From == null)
                {
                    con.From = _world.Mappings[0];
                }
                if(con.To == null)
                {
                    con.To = _world.Mappings[0];
                }

                //Generate a new waypoint
                if(con.FromLink == null)
                {
                    con.FromLink = new MapArea.Waypoint(Vector3.zero,
                                                    Vector3.zero,
                                                    0,
                                                    WaypointState.Default);
                }
                if(con.ToLink == null)
                {
                    con.ToLink = new MapArea.Waypoint(Vector3.zero,
                                                    Vector3.zero,
                                                    0,
                                                    WaypointState.Default);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        [CustomPropertyDrawer(typeof(Connection))]
        public class ConnectionAttributeDrawer : PropertyDrawer
        {
            WorldObject _world = null;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                if(_world == null)
                {
                    _world = (WorldObject)property.serializedObject.targetObject;
                }

                position.height = 16f;
                property.isExpanded = EditorGUI.Foldout (position, property.isExpanded, label);
                
                if(property.isExpanded)
                {
                    //----------------From----------------------
                    property.serializedObject.Update();

                    EditorGUI.LabelField(new Rect( position.x, position.y + 18, 60, 16), "From:");

                    var FromProp = property.FindPropertyRelative("From");
                     
                    //Too heavy performance-wise?
                    var FromIndex = _world.Mappings.FindIndex(x => x == (SceneMapping)FromProp.objectReferenceValue);

                    EditorGUI.BeginChangeCheck();
                    FromIndex = EditorGUI.Popup(new Rect( position.x + 60, position.y + 18, position.width - 60, 16), FromIndex, _mappings);

                    if(EditorGUI.EndChangeCheck())
                    {
                        FromProp.objectReferenceValue = _world.Mappings[FromIndex];
                    }

                    //------------------Waypoint--------------------

                    bool isSelected = _selectedWaypointPath != "" && (_selectedWaypointPath == property.FindPropertyRelative("FromLink").propertyPath);
                    var WaypointProp = property.FindPropertyRelative("FromLink").FindPropertyRelative("position");

                    if(isSelected)
                    {
                        WaypointProp.vector3Value = _selectedWaypointPos;
                        property.serializedObject.ApplyModifiedProperties();
                    }

                    if(GUI.Button(new Rect(position.x , position.y + 38, position.width, 20), "Edit Waypoint"))
                    {
                        if(!isSelected)
                        {
                            _selectedWaypointPath = property.FindPropertyRelative("FromLink").propertyPath;
                            _selectedWaypointPos = WaypointProp.vector3Value;
                        } else 
                        {
                            _selectedWaypointPath = "";
                        }

                        SceneView.RepaintAll();
                    }

                    EditorGUI.LabelField(new Rect(position.x, position.y + 58, position.width, 16), WaypointProp.vector3Value.ToString(), _customStyle.customStyles[1]);

                    //------------------To--------------------

                    EditorGUI.LabelField(new Rect( position.x, position.y + 80, 60, 16), "To:");

                    var ToProp = property.FindPropertyRelative("To");
                    
                    //Too heavy performance-wise?
                    var ToIndex = _world.Mappings.FindIndex(x => x == (SceneMapping)ToProp.objectReferenceValue);

                    EditorGUI.BeginChangeCheck();
                    ToIndex = EditorGUI.Popup(new Rect( position.x + 60, position.y + 80, position.width - 60, 16), ToIndex, _mappings);

                    if(EditorGUI.EndChangeCheck())
                    {
                        ToProp.objectReferenceValue = _world.Mappings[ToIndex];
                    }

                    //------------------Waypoint 2--------------------

                    bool isSelected2 = _selectedWaypointPath != "" && (_selectedWaypointPath == property.FindPropertyRelative("ToLink").propertyPath);
                    var WaypointProp2 = property.FindPropertyRelative("ToLink").FindPropertyRelative("position");

                    if(isSelected2)
                    {
                        WaypointProp2.vector3Value = _selectedWaypointPos;
                        property.serializedObject.ApplyModifiedProperties();
                    }

                    if(GUI.Button(new Rect(position.x , position.y + 100, position.width, 20), "Edit Waypoint"))
                    {
                        if(!isSelected2)
                        {
                            _selectedWaypointPath = property.FindPropertyRelative("ToLink").propertyPath;
                            _selectedWaypointPos = WaypointProp2.vector3Value;
                        } else 
                        {
                            _selectedWaypointPath = "";
                        }

                        SceneView.RepaintAll();
                    }

                    EditorGUI.LabelField(new Rect(position.x, position.y + 120, position.width, 16), WaypointProp2.vector3Value.ToString(), _customStyle.customStyles[1]);
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return property.isExpanded ? 140 : 16;
            }
        }
    }
}
