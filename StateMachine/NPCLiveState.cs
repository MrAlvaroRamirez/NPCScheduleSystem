using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLiveState : NPCBaseState
{
    public NPCLiveState(NPCStateMachine currentContext, NPCStateFactory NPCStateFactory)
    : base (currentContext, NPCStateFactory) 
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        if(_ctx.HasReachedDestination)
        {
           SetSubState(_factory.Idle());
        } else
        {
            ProcessWaypoint();
        }
    }
    public override void UpdateState(){}
    public override void ExitState(){}
    public override void CheckSwitchStates(){}
    public override void InitializeSubState(){}
    public override void Trigger(string trigger)
    {
        if(trigger == "WaypointUpdate")
        {
            ProcessWaypoint();
        }
    }
    public override void Trigger(int damage, Vector3 position){}

    //Function to check the current waypoint and decide the action
    private void ProcessWaypoint()
    {
        if(_ctx.CurrentWaypoint == null)
        {
            Debug.LogWarning("NPC doesn't have an active waypoint");
            SetSubState(_factory.Idle());
            return;
        }

        if(_ctx.HasReachedDestination)
        {
            SetSubState(_factory.Idle());
            return;
        }
        
        SetSubState(_factory.Move());
    }
}
