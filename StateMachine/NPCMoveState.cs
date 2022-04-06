using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMoveState : NPCBaseState
{
    public NPCMoveState(NPCStateMachine currentContext, NPCStateFactory NPCStateFactory)
    : base (currentContext, NPCStateFactory){}
    public override void EnterState()
    {
        _ctx.navMeshAgent.enabled = true;
        _ctx.navMeshAgent.isStopped = false;
        _ctx.GetComponentInChildren<Renderer>().material.color = Color.red;

        _ctx.animator.SetInteger("Action", 0);

        if(_ctx.CurrentConnection != null)
            _ctx.navMeshAgent.SetDestination(_ctx.CurrentConnection.FromLink.position);
        else
            _ctx.navMeshAgent.SetDestination(_ctx.CurrentWaypoint.position);
    }
    public override void UpdateState()
    {
        _ctx.animator.SetFloat("Speed", _ctx.navMeshAgent.velocity.magnitude/_ctx.navMeshAgent.speed);
        CheckSwitchStates();
    }
    public override void ExitState(){}
    public override void CheckSwitchStates()
    {
        if(Vector3.Distance(_ctx.transform.position, _ctx.navMeshAgent.destination) <= 1f)
        {
            _ctx.ReachDestination();

            if(_ctx.CurrentConnection == null)
            {
                switch (_ctx.CurrentWaypoint.State)
                {
                    case NPC.WaypointState.Default:
                        SwitchState(_factory.Idle());
                        break;
                    case NPC.WaypointState.Area:
                        //TODO: Change to wander
                        SwitchState(_factory.Idle());
                        break;
                    case NPC.WaypointState.Match:
                        SwitchState(_factory.Idle());
                        break;
                    case NPC.WaypointState.Seat:
                        SwitchState(_factory.Sit());
                        break;
                }
            }
        }
    }

    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}
}
