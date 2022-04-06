using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NPC;

public class NPCIdleState : NPCBaseState
{
    public NPCIdleState(NPCStateMachine currentContext, NPCStateFactory NPCStateFactory)
    : base (currentContext, NPCStateFactory){}
    public override void EnterState()
    {
        _ctx.GetComponentInChildren<Renderer>().material.color = Color.green;
        _ctx.animator.SetFloat("Speed", 0);

        SetAnimation();
    }


    public override void UpdateState(){}
    public override void ExitState(){}
    public override void CheckSwitchStates(){}
    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}
    private void SetAnimation()
    {
        if(_ctx.CurrentWaypoint == null) return;

        Debug.Log(_ctx.CurrentWaypoint.State);

        switch(_ctx.CurrentWaypoint.State)
        {
            case WaypointState.Default:
                _ctx.navMeshAgent.isStopped = true;
                _ctx.animator.SetInteger("Action", 0);
                break;
            case WaypointState.Match:
                _ctx.navMeshAgent.enabled = false;
                _ctx.transform.position = _ctx.CurrentWaypoint.position;
                _ctx.animator.SetInteger("Action", 0);
                break;
            case WaypointState.Seat:
                _ctx.navMeshAgent.enabled = false;
                _ctx.transform.position = _ctx.CurrentWaypoint.position;
                _ctx.animator.SetInteger("Action", 1);
                break;
            default:
                break;
        }
    }
}
