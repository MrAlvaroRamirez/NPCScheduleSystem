using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC {
    [CreateAssetMenu(menuName = "NPC/Character")]
    public class NPCCharacter : ScriptableObject
    {
        public string CharacterName;
        public Group Group;
    }

    public enum Group
    {
        Student,
        Teacher,
        Other
    }
}