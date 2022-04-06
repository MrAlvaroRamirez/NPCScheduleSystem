using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC {
    [CreateAssetMenu(menuName = "NPC/Schedule")]
    public class NPCSchedule : ScriptableObject
    {
        public List<NPCAction> CharActList = new List<NPCAction>();
        public List<GroupAction> GroupActList = new List<GroupAction>();
        public WorldObject World;
        public WorldNPCS SceneNPCS;
    }

    [Serializable]
    public class NPCAction
    {
        public int Hour;
        public NPCCharacter Character;
        public MapArea Area;

        public NPCAction(int h, NPCCharacter c, MapArea m)
        {
            Hour = h;
            Character = c;
            Area = m;
        }
    }

    [Serializable]
    public class GroupAction
    {
        public int Hour;
        public Group Group;
        public MapArea Area;

        public GroupAction(int h, Group g, MapArea m)
        {
            Hour = h;
            Group = g;
            Area = m;
        }
    }

    //[System.Serializable] public class CharacterActionDictionary : SerializableDictionary<NPCCharacter, NPCAction> {}
    //[System.Serializable] public class GroupActionDictionary : SerializableDictionary<Group, NPCAction> {}
}
