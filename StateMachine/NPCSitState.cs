using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NPCSitState : NPCBaseState
{
    public NPCSitState(NPCStateMachine currentContext, NPCStateFactory NPCStateFactory)
    : base (currentContext, NPCStateFactory){}
    public override void EnterState()
    {
        _ctx.navMeshAgent.enabled = false;
        _ctx.GetComponentInChildren<Renderer>().material.color = Color.green;
        _ctx.animator.SetFloat("Speed", 0);
        _ctx.animator.SetInteger("Action", 1);
        Sit();
    }
    public override void UpdateState(){}
    public override void ExitState(){}
    public override void CheckSwitchStates(){}
    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}

    private void Sit()
    {
        _ctx.animator.SetTrigger("Sit");

        var startPos = _ctx.CurrentWaypoint.position;
        startPos.y = _ctx.transform.position.y;
        startPos += Quaternion.Euler(_ctx.CurrentWaypoint.rotation) * Vector3.forward * .5f;

        _ctx.transform.DORotate(_ctx.CurrentWaypoint.rotation, 0.5f);

        Sequence sitSequence = DOTween.Sequence();
        sitSequence.Append(_ctx.transform.DOMove(startPos, .6f))
            .AppendInterval(.2f)
            .Append(_ctx.transform.DOMove(_ctx.CurrentWaypoint.position, 1f))
            .OnComplete(() => SwitchState(_factory.Idle()));
    }
}
