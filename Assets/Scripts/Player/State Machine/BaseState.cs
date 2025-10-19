using UnityEngine;
public abstract class BaseState : IPlayerState
{
    protected BaseState _superState;
    protected BaseState _subState;
    public BaseState SubState => _subState;
    public BaseState SuperState => _superState;


    public virtual void Enter(PlayerContext ctx) { }
    public virtual void Exit(PlayerContext ctx) { }
    public virtual void Tick(PlayerContext ctx, float dt) { }
    public virtual IPlayerState TryTransition(PlayerContext ctx) => null;

    public virtual void InitializeSubState(PlayerContext ctx) { }

    public void UpdateStates(PlayerContext ctx, float dt)
    {
        Tick(ctx, dt);
        _subState?.UpdateStates(ctx, dt);
    }

    protected void SetSubState(BaseState sub)
    {
        _subState = sub;
        sub._superState = this;
    }

    protected void SwitchState(PlayerContext ctx, BaseState newState)
    {
        Exit(ctx);
        newState.Enter(ctx);

        // ✅ FIXED:
        // The old line ctx.fsm.CurrentState = newState caused null reference,
        // because PlayerContext has no fsm reference.
        // Instead, locate and update the state machine directly from the context.
        var fsm = ctx.GetComponent<PlayerStateMachine>();
        if (fsm != null)
            fsm.SetCurrentState(newState);        // root switch
        else if (_superState != null)
            _superState.SetSubState(newState);
        else
            Debug.LogError("FSM not found on context object.");
    }
}
