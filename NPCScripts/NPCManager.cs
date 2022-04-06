using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace NPC
{
    public class NPCManager : MonoBehaviour
    {
        [SerializeField] private NPCSchedule _schedule;
        [SerializeField] private GameObject _npcTemplate;
        private static float timer;
        private static int _currentHour = -1;
        private int CurrentHour
        {
            get{return _currentHour;}
            set
            {
                if(value != CurrentHour)
                {
                    _currentHour = value;
                    HourChange(value);
                }
            }
        }
        //List to store all the npc actions (merging groups and characters)
        private static List<NPCAction> _totalActionList = new List<NPCAction>();
        private static List<NPCStatus> NPCStatuses = new List<NPCStatus>();
        private static Dictionary<NPCStatus, NPCStateMachine> liveNPCS = new Dictionary<NPCStatus, NPCStateMachine>();

        //TODO: Remove this
        public SceneMapping currentMap;
        public float UnseenNPCSpeed = 2.8f;
        //---

        private static Dictionary<string, int> areaWaypointCount = new Dictionary<string, int>(); 
        private static int bufferedWaypointCount = 0;
        private static int bufferedWaypoint = -1;


        private void Start() 
        {
            FillActionList();
            InitNPCS();
        }

        private void FillActionList()
        {
            _totalActionList.Clear();

            //Fill with all the groups
            foreach (GroupAction action in _schedule.GroupActList)
            {
                foreach (NPCCharacter character in _schedule.SceneNPCS.NPCs.FindAll(x => x.Group == action.Group))
                {
                    _totalActionList.Add(new NPCAction(action.Hour, character, action.Area));
                }
            }

            //Fill and override with the characters
            foreach (NPCAction action in _schedule.CharActList)
            {
                //Check if already exists, if so, eliminate it
                int index = _totalActionList.FindIndex(x => x.Character == action.Character && x.Hour == action.Hour);

                if(index != -1)
                {
                    //Action exists, override it
                    _totalActionList[index] = action;
                } else
                {
                    _totalActionList.Add(action);
                }
            }

            //Sort the list
            _totalActionList = _totalActionList.OrderBy(x => x.Hour).ToList();
        }

        private void Update()
        {
            timer += Time.deltaTime;

            CurrentHour = (int)(timer % 60);

            //TODO: InvokeRepeating for performance
            NPCTick();
        }

        private void ResetWaypointDict()
        {
            areaWaypointCount.Clear();
            bufferedWaypointCount = 0;
            bufferedWaypoint = -1;
        }

        private void InitNPCS()
        {
            //Check if already initialized
            if(CurrentHour != -1) 
            {
                NPCSceneReset();
                return;
            }

            //Clear the waypoint dictionary
            ResetWaypointDict();

            foreach (NPCAction action in _totalActionList.FindAll(x => x.Hour == -1))
            {
                if(action.Area.Equals(SceneMapping.DisabledArea)) continue;
                InitializeSingleNPC(action);
            }
        }

        private void InitializeSingleNPC(NPCAction action)
        {
            MapArea.Waypoint selectedWaypoint;

            try{
                selectedWaypoint = GetWaypoint(action);
            } catch (Exception)
            {
                Debug.LogError("Couldn't find any free space on the selected area");
                return;
            }

            var u = new NPCStatus(action.Character, action.Area.SceneMap, selectedWaypoint, true);
            NPCStatuses.Add(u);

            UpdateNPC(u);

            Debug.Log("Added character: " + action.Character.CharacterName);
        }

        private void NPCSceneReset()
        {
            //Clean the live npc list, we first unsubsribe the events
            foreach (var liveNpc in liveNPCS)
            {
                liveNpc.Value.onReachDestination -= ()=> UpdateNPCDestination(liveNpc.Key);
                liveNpc.Key.Unseen = true;
            }

            //Onve finished, the clean the dictionary
            liveNPCS.Clear();
            
            foreach (NPCStatus status in NPCStatuses)
            {
                if(status.CurrentMapping == currentMap)
                {
                    SpawnNPC(status);
                }
                UpdateNPC(status);
            }
        }

        private void HourChange(int currentHour)
        {
            //Clear the waypoint dictionary
            ResetWaypointDict();

            foreach (NPCAction action in _totalActionList.FindAll(x => x.Hour == currentHour))
            {
                if(action.Area.Equals(SceneMapping.DisabledArea)) continue;
                
                //TODO: Custom find implementation with out value
                if(NPCStatuses.Any(x => x.Npc == action.Character))
                {
                    NPCHourChange(action);
                } else
                {
                    Debug.LogError("Couldn't find the NPC, are you initializing it?");
                }
            }
        }

        private void NPCHourChange(NPCAction action)
        {
            MapArea.Waypoint selectedWaypoint;

            try{
                selectedWaypoint = GetWaypoint(action);
            } catch (Exception)
            {
                Debug.LogError("Couldn't find any free space on the selected area: ");
                return;
            }

            NPCStatus NPC = NPCStatuses.Find(x => x.Npc == action.Character);

            Vector3 startPosition;

            //Initialize the start position
            if (NPC.hasReachedDestination)
            {
                //If the NPC is idling, then his end position is the start position
                startPosition = NPC.EndPosition.position;
            } else
            {
                //If the npc is unseen, we need to estimate the current position
                if (NPC.Unseen)
                {
                    //We calculate the actual position
                    //TODO: Change this? Is this working? Should we expect the npcs to be anywhere else?
                    //Can be unnatural sometimes, but it's far simpler
                    startPosition = NPC.EndPosition.position;
                } else
                {
                    //If the NPC is live, we get the current position
                    startPosition = liveNPCS[NPC].transform.position;
                }
            }

            NPC.UpdateStatus(startPosition, selectedWaypoint, action.Area.SceneMap, NPC.CurrentMapping != action.Area.SceneMap, CurrentHour);
            UpdateNPC(NPC);
        }

        private Vector3 EstimateUnseenNPCPosition(Vector3 startPos, Vector3 endPos, int startHour)
        {
            //Calculate the AI Path
            var path = new NavMeshPath();
            NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, path);  //Conditional?

            //Get the distance
            //Time of the teleport, no the start!!!!!!!!!!!!!!!!
            var timeElapsed = CurrentHour - startHour;

            return NPCVectorHelper.FindPointAlongPath(path.corners, timeElapsed * UnseenNPCSpeed);
        }

        private MapArea.Waypoint GetWaypoint(NPCAction action)
        {
            //TODO: Get a better random waypoint
            if (!areaWaypointCount.TryGetValue(action.Area.AreaName, out int currentWaypoint))
            {
                areaWaypointCount.Add(action.Area.AreaName, 0);
            }

            if(bufferedWaypointCount > 0)
            {
                bufferedWaypointCount--;
                return action.Area.Waypoints[bufferedWaypoint];
            }
            
            //Check if waypoint is an area, if so, create a simple buffer
            if(action.Area.Waypoints[currentWaypoint].State == WaypointState.Area)
            {
                
                bufferedWaypointCount = action.Area.Waypoints[currentWaypoint].MaxNPCS - 1;
                bufferedWaypoint = currentWaypoint;
            }

            areaWaypointCount[action.Area.AreaName]++;

            try{
                var pos = action.Area.Waypoints[currentWaypoint];
                return pos;
            } catch (IndexOutOfRangeException)
            {
                throw new Exception();
            }
        }

        private void UpdateNPC(NPCStatus npcStatus)
        {
            if(npcStatus.lookingForConnection)
            {
                try
                {
                    npcStatus.TargetConnection = GetNextAreaConnection(npcStatus.CurrentMapping, npcStatus.DestinationMapping);
                    npcStatus.lookingForConnection = false;
                }
                catch (Exception)
                {
                    Debug.LogError("Couldn't find a connection between the maps");
                    return;
                }
            }

            //If the npc is in the current map, spawn it if it's not already there
            if(!liveNPCS.TryGetValue(npcStatus, out NPCStateMachine npcObj))
            {
                if(npcStatus.CurrentMapping == currentMap)
                {
                    SpawnNPC(npcStatus);
                }
            } else
            {
                if(npcStatus.CurrentMapping != currentMap)
                {
                    //Despawn the NPC
                    if(npcObj != null)
                        Destroy(npcObj.gameObject);

                    npcStatus.Unseen = true;
                    liveNPCS[npcStatus].onReachDestination -= ()=> UpdateNPCDestination(npcStatus);
                    liveNPCS.Remove(npcStatus);
                }
            }
            
            //Assign the target
            if(!npcStatus.hasReachedDestination)
            {
                //Update arrival time
                npcStatus.expectedArrivalTime = GetEstimatedArrivalTime(npcStatus.StartPosition, npcStatus.GetTargetPosition(), npcStatus.lastActionHour);

                if(!npcStatus.Unseen)
                {
                    liveNPCS[npcStatus].HasReachedDestination = false;
                    liveNPCS[npcStatus].CurrentConnection = (npcStatus.CurrentMapping == npcStatus.DestinationMapping) ? null : npcStatus.TargetConnection;
                    liveNPCS[npcStatus].CurrentWaypoint = npcStatus.EndPosition;
                }
            }
        }

        private NPCStateMachine SpawnNPC(NPCStatus npcStatus)
        {
            NPCStateMachine npcObj;
            //TODO: Clean this, repeated
            var livePosition = npcStatus.hasReachedDestination ? npcStatus.EndPosition.position : EstimateUnseenNPCPosition(npcStatus.StartPosition, npcStatus.GetTargetPosition(), npcStatus.lastActionHour);
            npcObj = Instantiate(_npcTemplate, livePosition, Quaternion.identity).GetComponent<NPCStateMachine>();
            liveNPCS.Add(npcStatus, npcObj);
            npcStatus.Unseen = false;

            //Assign Waypoint to the live npc
            npcObj.HasReachedDestination = npcStatus.hasReachedDestination;
            npcObj.CurrentWaypoint = npcStatus.EndPosition;

            //Subscribe to event
            npcObj.onReachDestination += ()=> UpdateNPCDestination(npcStatus);

            return npcObj;
        }

        private void NPCTick()
        {
            //foreach npc, get last && new area, calculate distance
            //check if hasReachedDestination
            //if currentarea != targetarea
            foreach (NPCStatus NPC in NPCStatuses)
            {
                if(NPC.hasReachedDestination) continue;
                
                //Check if character has reached destination
                if(NPC.Unseen && CheckDestination(NPC))
                {
                    UpdateNPCDestination(NPC);
                }
            }
        }

        private void UpdateNPCDestination(NPCStatus NPC)
        {
            //If is the same mapping, end
            if (NPC.CurrentMapping == NPC.DestinationMapping)
            {
                NPC.hasReachedDestination = true;
                Debug.Log(NPC.Npc.CharacterName + " arrived to the destination");
            }
            else
            {
                //If it's not, update the current mapping, current position and lookingforconnection = true
                NPC.CurrentMapping = NPC.TargetConnection.To;
                NPC.StartPosition = NPC.TargetConnection.ToLink.position;
                Debug.Log("Traveling to: " + NPC.CurrentMapping.MapName);

                NPC.UpdateHour(CurrentHour);

                UpdateNPC(NPC);
            }
        }

        private bool CheckDestination(NPCStatus NPC)
        {
            if(CurrentHour >= NPC.expectedArrivalTime)
            {
                return true;
            }

            return false;
        }

        private int GetEstimatedArrivalTime(Vector3 startPos, Vector3 endPos, int startHour)
        {
            //TODO: Fill this
            int disTime = Mathf.RoundToInt(Vector3.Distance(startPos, endPos) / UnseenNPCSpeed);
            Debug.Log("Got new estimated time: " + (startHour + disTime));
            return startHour + disTime;
        }

        private Connection GetNextAreaConnection(SceneMapping current, SceneMapping destination)
        {
            //keep a record of the possible areas to search
            List<SceneMapping> mapsToCheck = new List<SceneMapping>();
            mapsToCheck.Add(current);
            int mapIndex = 0;

            //Passes
            while(mapIndex < mapsToCheck.Count)
            {
                //Check for connections starting at the current map
                foreach (Connection con in _schedule.World.Connections.FindAll(x => x.From == current))
                {
                    //If the connection leads to the destination, stop
                    //If not, add the other maps to the list (if they're not already there)
                    if(con.To == destination)
                    {
                        Debug.Log("Updated connection, going to: " + con.To.MapName);
                        return con;
                    } else
                    {
                        if(!mapsToCheck.Contains(con.To))
                        {
                            mapsToCheck.Add(con.To);
                        }

                        mapIndex++;
                    }
                }
            }
            throw new Exception();
        }

        void OnGUI() {
            GUILayout.Label(CurrentHour.ToString());
            if(GUILayout.Button("Go To Map 1"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("yard");
            if(GUILayout.Button("Go To Map 2"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("school");
        }

        private class NPCStatus
        {
            public NPCCharacter Npc;
            public bool Unseen;
            public SceneMapping CurrentMapping;
            public SceneMapping DestinationMapping;
            public Vector3 StartPosition;
            public MapArea.Waypoint EndPosition;
            public Connection TargetConnection;
            public int lastActionHour = 0;
            public int expectedArrivalTime = 0;
            public bool hasReachedDestination, lookingForConnection;

            public NPCStatus(NPCCharacter n, SceneMapping cM, MapArea.Waypoint sP, bool u)
            {
                Npc = n;
                Unseen = u;
                CurrentMapping = cM;
                DestinationMapping = cM;
                StartPosition = sP.position;
                EndPosition = sP;
                hasReachedDestination = true;
                TargetConnection = null;
            }

            public void UpdateStatus(Vector3 sP, MapArea.Waypoint eP, SceneMapping dM, bool lFC, int h)
            {
                StartPosition = sP;
                EndPosition = eP;
                DestinationMapping = dM;
                lastActionHour = h;
                lookingForConnection = lFC;
                hasReachedDestination = false;
            }

            public void UpdateHour(int newHour)
            {
                lastActionHour = newHour;
            }

            public Vector3 GetTargetPosition()
            {
               return (CurrentMapping == DestinationMapping) ? EndPosition.position : TargetConnection.FromLink.position;
            }
        }
    }
}
