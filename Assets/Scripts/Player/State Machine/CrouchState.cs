public class CrouchState : BaseState
{
    public override void Enter(PlayerContext ctx)
    {
        if (ctx.motor == null) return;
        ctx.motor.wantCrouch = true;
        ctx.motor.wantSprint = false;
    }

    public override void Tick(PlayerContext ctx, float dt)
    {
        ctx.motor.wantCrouch = true;
        ctx.motor.wantSprint = false;
    }

    public override IPlayerState TryTransition(PlayerContext ctx)
    {
        if (!ctx.CrouchHeld)
        {
            if (ctx.Move.sqrMagnitude > 0.01f)
                SwitchState(ctx, new WalkRunState());
            else
                SwitchState(ctx, new IdleState());
            return null;
        }

        if (ctx.JumpPressed)
        {
            SwitchState(ctx, new JumpState());
            return null;
        }

        return null;
    }
}
