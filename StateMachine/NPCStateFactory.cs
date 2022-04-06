using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateFactory
{
    NPCStateMachine _context;
    Dictionary<string, NPCBaseState> states = new Dictionary<string, NPCBaseState>();

    public NPCStateFactory(NPCStateMachine currentContext)
    {
        _context = currentContext;

        states.Add("idle", new NPCIdleState(_context, this));
        states.Add("wander", new NPCWanderState(_context, this));
        states.Add("move", new NPCMoveState(_context, this));
        states.Add("live", new NPCLiveState(_context, this));
        states.Add("sit", new NPCSitState(_context, this));
    }

    public NPCBaseState Idle()
    {
        return states["idle"];
    }
    public NPCBaseState Wander()
    {
        return states["wander"];
    }
    public NPCBaseState Move()
    {
        return states["move"];
    }
    public NPCBaseState Live()
    {
        return states["live"];
    }
    public NPCBaseState Sit()
    {
        return states["sit"];
    }
}
