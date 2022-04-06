public abstract class NPCBaseState
{
    protected bool _isRootState = false;
    protected NPCStateMachine _ctx;
    protected NPCStateFactory _factory;
    protected NPCBaseState _currentSubState;
    protected NPCBaseState _currentSuperState;

    public NPCBaseState(NPCStateMachine currentContext, NPCStateFactory NPCStateFactory)
    {
        _ctx = currentContext;
        _factory = NPCStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubState();
    public abstract void Trigger(string trigger);
    public abstract void Trigger(int damage, UnityEngine.Vector3 position);
    public void UpdateStates()
    {
        UpdateState();
        if(_currentSubState != null)
        {
            _currentSubState.UpdateState();
        }
    }
    protected void SwitchState(NPCBaseState newState)
    {
        ExitState();

        if(_isRootState)
        {
            //Switch current state of the context
            _ctx.CurrentState = newState;
            newState.EnterState();
        } else if (_currentSuperState != null)
        {
            _currentSuperState.SetSubState(newState);
        }
    }
    protected void SetSuperState(NPCBaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }
    protected void SetSubState(NPCBaseState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.EnterState();
        newSubState.SetSuperState(this);
    }
}
