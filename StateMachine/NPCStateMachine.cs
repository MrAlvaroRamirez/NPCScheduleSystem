using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NPC;
using System;

public class NPCStateMachine : MonoBehaviour
{
    #region Variables

    [Space(20)]
    [Header("Component references")]
    [Space(20)]

    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public Animator animator;

    [Space(20)]
    [Header("Movement")]
    [Space(20)]

    public float WalkSpeed;
    public float RunSpeed;
    public float FollowDistance;

    //live stuff
    public bool HasReachedDestination = true;
    private MapArea.Waypoint _currentWaypoint;
    public MapArea.Waypoint CurrentWaypoint { get => _currentWaypoint; set => AssignWaypoint(value); }
    private Connection _currentConnection;
    public Connection CurrentConnection { get => _currentConnection; set => _currentConnection = value; }

    //Event
    public delegate void OnReachDestination();
    public event OnReachDestination onReachDestination;

    //combat stuff
    //check

    //

    //state
    public NPCBaseState CurrentState;
    NPCStateFactory _states;


    #endregion

    void Awake()
    {
        //references
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        //setup state
        _states = new NPCStateFactory(this);
        CurrentState = _states.Live();
        CurrentState.EnterState(); 
    }

    void Update()
    {
        CurrentState.UpdateStates();
    }

    public void StateTrigger(string trigger) {
        CurrentState.Trigger(trigger);
    }
    
    private void AssignWaypoint(MapArea.Waypoint value)
    {
        _currentWaypoint = value;
        StateTrigger("WaypointUpdate");
    }

    public void ReachDestination()
    {
        onReachDestination.Invoke();
    }

    void OnMouseDown()
    {
        Debug.Log("heey");
    }
}
