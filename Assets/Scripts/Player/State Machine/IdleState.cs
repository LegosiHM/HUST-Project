using UnityEngine.InputSystem.LowLevel;

public class IdleState : BaseState
{
    public override void Enter(PlayerContext ctx)
    {
        ctx.Cache();
        if (ctx.motor == null) return;
        ctx.motor.wantSprint = false;
        ctx.motor.wantCrouch = false;
    }

    public override void Tick(PlayerContext ctx, float dt)
    {
        ctx.motor.wantSprint = false;
        ctx.motor.wantCrouch = ctx.CrouchHeld;
    }

    public override IPlayerState TryTransition(PlayerContext ctx)
    {
        if (ctx.JumpPressed)
        {
            SwitchState(ctx, new JumpState());
            return null;
        }

        if (ctx.CrouchHeld)
        {
            SwitchState(ctx, new CrouchState());
            return null;
        }

        if (ctx.Move.sqrMagnitude > 0.01f)
        {
            SwitchState(ctx, new WalkRunState());
            return null;
        }

        return null;
    }
}
