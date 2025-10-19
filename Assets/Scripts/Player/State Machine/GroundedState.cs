public class GroundedState : BaseState
{
    public override void Enter(PlayerContext ctx)
    {
        ctx.motor.wantJumpPulse = false;
    }

    public override void Tick(PlayerContext ctx, float dt)
    {
        // Common grounded logic
        ctx.motor.wantSprint = ctx.SprintHeld;
        ctx.motor.wantCrouch = ctx.CrouchHeld;
    }

    public override void InitializeSubState(PlayerContext ctx)
    {
        if (ctx.CrouchHeld) SetSubState(new CrouchState());
        else if (ctx.Move.sqrMagnitude > 0.01f) SetSubState(new WalkRunState());
        else SetSubState(new IdleState());
    }

    public override IPlayerState TryTransition(PlayerContext ctx)
    {
        if (ctx.JumpPressed)
            return new JumpState();
        return null;
    }
}
