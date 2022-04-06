using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC {
    [CreateAssetMenu(menuName = "NPC/World NPCs")]
    public class WorldNPCS : ScriptableObject
    {
        public List<NPCCharacter> NPCs = new List<NPCCharacter>();
    }
}