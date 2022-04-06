using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NPC {
    [CustomEditor(typeof(NPCSchedule))]
    public class ScheduleEditor : Editor
    {
        NPCSchedule _schedule;
        static GUISkin _customStyle = null;
        List<HourContainer> _hours = new List<HourContainer>();
        //List<NPCAction> _init = new List<NPCAction>();

        List<MapArea> MapAreas = new List<MapArea>();
        string[] _groups;
        string[] _areas;
        string[] _characters;


        void OnEnable() {
            _schedule = (NPCSchedule) target;
            InitStyle();
            InitVariables();
            Undo.undoRedoPerformed += OnUndoRedo;
        }
        
        void OnDisable() 
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            InitVariables();
        }


        void InitStyle()
        {
            if(_customStyle == null)
            {
                _customStyle = EditorGUIUtility.Load("mapSkin.guiskin") as GUISkin;
            }
        }

        void InitVariables()
        {
            //Clear
            _hours.Clear();
            //Init groups
            _groups = System.Enum.GetNames(typeof(Group));

            //Initial values
            _hours.Add(new HourContainer(-1, new List<ActionBind>()));

            //Init areas
            if(_schedule.World.Mappings.Count != 0)
            {
                //Add a null object for the disabled state
                MapAreas.Add(SceneMapping.DisabledArea);  

                //Fill the areas
                foreach (SceneMapping mapping in _schedule.World.Mappings)
                {
                    MapAreas.AddRange(mapping.Areas);
                }

                //Convert the area list to an array of string for the popup
                var newAreas = new string[MapAreas.Count];
                for (int i = 0; i < MapAreas.Count; i++)
                {
                    newAreas[i] = MapAreas[i].AreaName;
                }

                _areas = newAreas;
            }

            //Init Characters
            if(_schedule.SceneNPCS.NPCs.Count != 0)
            {
                //Convert the characters list to an array of string for the popup
                var newCharacters = new string[_schedule.SceneNPCS.NPCs.Count];
                for (int i = 0; i < _schedule.SceneNPCS.NPCs.Count; i++)
                {
                    newCharacters[i] = _schedule.SceneNPCS.NPCs[i].CharacterName;
                }
                _characters = newCharacters;
            }

            //Init actions
            foreach (NPCAction action in _schedule.CharActList)
            {
                ActionBind ab = new ActionBind(action.Character, MapAreas.FindIndex(x => x.Equals(action.Area)));

                //See if the hour already exist
                if (_hours.Any(x => x.hour == action.Hour)) { 
                    //TODO: Searching twice, implement a TryFind method
                    //If the hour exist, add the action to it's action list
                    _hours.Find(x => x.hour == action.Hour).actions.Add(ab);
                } else
                {
                    var abList = new List<ActionBind>();
                    abList.Add(ab);
                    _hours.Add(new HourContainer(action.Hour, abList));

                }
            }

            foreach (GroupAction action in _schedule.GroupActList)
            {
                ActionBind ab = new ActionBind(action.Group, MapAreas.FindIndex(x => x.Equals(action.Area)));

                //See if the hour already exist
                if (_hours.Any(x => x.hour == action.Hour)) { 
                    //TODO: Searching twice, implement a TryFind method
                    //If the hour exist, add the action to it's action list
                    _hours.Find(x => x.hour == action.Hour).actions.Add(ab);
                } else
                {
                    var abList = new List<ActionBind>();
                    abList.Add(ab);
                    _hours.Add(new HourContainer(action.Hour, abList));
                }
            }

            //Sort the list by hour
            _hours = _hours.OrderBy(x => x.hour).ToList();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("World"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SceneNPCS"));
            if(EditorGUI.EndChangeCheck())
            {
                //Reset the variables
                serializedObject.ApplyModifiedProperties();

                if(_schedule.World != null && _schedule.SceneNPCS != null)
                {
                    InitVariables();
                }
            }

            if(_schedule.World != null && _schedule.SceneNPCS != null)
            { 
                List<HourContainer> hcList = new List<HourContainer>();
                hcList.AddRange(_hours);
                foreach(HourContainer hourContainer in hcList)
                {
                    DrawHour(hourContainer);
                }

                EditorGUI.BeginChangeCheck();
                if(GUILayout.Button("Add Hour"))
                {
                    CreateHour();
                }
                if(EditorGUI.EndChangeCheck())
                {
                    SerializeHourContainer();
                }
                
            } else
            {
                EditorGUILayout.LabelField("You need to setup a Mapping and an NPC list!", _customStyle.customStyles[0]);
            }
        }
        
        private void CreateHour()
        {
            _hours.Add(new HourContainer(0, new List<ActionBind>()));
        }

        private void CreateAction(HourContainer hourContainer)
        {
            hourContainer.actions.Add(new ActionBind(Group.Student, 0));
        }
        
        private void SerializeHourContainer()
        {
            _schedule.GroupActList.Clear();
            _schedule.CharActList.Clear();
            foreach (HourContainer hour in _hours)
            {
                foreach (ActionBind action in hour.actions)
                {
                    if(action.entity.GetType() == typeof(Group))
                    {
                        var ac = new GroupAction(hour.hour, (Group)action.entity, MapAreas[action.area]);
                        _schedule.GroupActList.Add(ac);
                    } else
                    {
                        var ac = new NPCAction(hour.hour, (NPCCharacter)action.entity, MapAreas[action.area]);
                        _schedule.CharActList.Add(ac);                    
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private void DrawHour(HourContainer hourContainer)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal("HelpBox");
                GUILayout.BeginVertical(GUILayout.Width(80));
                    EditorGUIUtility.labelWidth = 40;
                    EditorGUIUtility.fieldWidth = 20;
                    
                    //Check if it's init
                    if(hourContainer.hour >= 0)
                    {
                        hourContainer.hour = Mathf.Clamp(EditorGUILayout.IntField("Hour", hourContainer.hour, GUILayout.MaxWidth(80)), 0, 100);
                    } else
                    {
                        EditorGUILayout.LabelField("Init");
                    }

                    if(GUILayout.Button("Add Action", GUILayout.Width(80)))
                    {
                        CreateAction(hourContainer);
                    }
                    if(GUILayout.Button("Remove", GUILayout.Width(80)))
                    {
                        Undo.RecordObject(target, "Removed Hour");
                        _hours.RemoveAll(x => x == hourContainer);
                    }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                    List<ActionBind> acList = new List<ActionBind>();
                    acList.AddRange(hourContainer.actions);
                    foreach (ActionBind action in acList)
                    {
                        GUILayout.BeginVertical("HelpBox");
                        GUILayout.BeginHorizontal("HelpBox");
                            //0 = Group, 1 = Character
                            int typeIndex = action.entity.GetType() == typeof(Group) ? 0 : 1;

                            
                            int selectionIndex;

                            if(typeIndex == 0)
                            {
                                //Get current group
                                selectionIndex = (int)(Group)action.entity;
                            } else
                            {
                                //Get current character
                                selectionIndex = _schedule.SceneNPCS.NPCs.IndexOf((NPCCharacter)action.entity);
                            }

                            EditorGUI.BeginChangeCheck();
                                typeIndex = EditorGUILayout.Popup(typeIndex, new string[] {"Group", "Character"},GUILayout.Width(160));
                            if(EditorGUI.EndChangeCheck())
                            {
                                if(typeIndex == 0)
                                {
                                    //Reset current group
                                    action.entity = (Group)0;
                                } else
                                {
                                    //Reset current character
                                    action.entity = _schedule.SceneNPCS.NPCs[0];
                                }
                            }

                            EditorGUI.BeginChangeCheck();
                                selectionIndex = EditorGUILayout.Popup(selectionIndex, typeIndex == 0 ? _groups : _characters);
                            if(EditorGUI.EndChangeCheck())
                            {
                                if(typeIndex == 0)
                                {
                                    //Set current group
                                    action.entity = (Group)selectionIndex;
                                } else
                                {
                                    //Set current character
                                    action.entity = _schedule.SceneNPCS.NPCs[selectionIndex];
                                }
                            }

                        GUILayout.EndHorizontal();
                        
                        GUILayout.BeginHorizontal("HelpBox");
                            action.area = EditorGUILayout.Popup(action.area, _areas);
                            if(GUILayout.Button("Remove", GUILayout.Width(80)))
                            {
                                Undo.RecordObject(target, "Removed Action");
                                hourContainer.actions.Remove(action);
                            }
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();

                        GUILayout.Space(20);
                    }
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if(EditorGUI.EndChangeCheck())
            {
                SerializeHourContainer();
            }
        }

        private class HourContainer
        {
            public int hour;
            public List<ActionBind> actions;

            public HourContainer(int h, List<ActionBind> a)
            {
                hour = h;
                actions = a;
            }
        }

        class ActionBind 
        {
            public object entity;
            public int area;

            public ActionBind(NPCCharacter npc, int ma)
            {
                entity = npc;
                area = ma;
            }

            public ActionBind(Group group, int ma)
            {
                entity = group;
                area = ma;
            }
        }
    }
}
