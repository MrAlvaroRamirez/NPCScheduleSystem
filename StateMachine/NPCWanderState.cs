using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCWanderState : NPCBaseState
{
    public NPCWanderState(NPCStateMachine currentContext, NPCStateFactory NPCStateFactory)
    : base (currentContext, NPCStateFactory) {}
    public override void EnterState(){}
    public override void UpdateState(){}
    public override void ExitState(){}
    public override void CheckSwitchStates(){}
    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}
}
