using UnityEngine.InputSystem.LowLevel;

public class WalkRunState : BaseState
{
    public override void Enter(PlayerContext ctx)
    {
        if (ctx.motor == null) return;
        ctx.motor.wantCrouch = false;
    }

    public override void Tick(PlayerContext ctx, float dt)
    {
        ctx.motor.wantSprint = ctx.SprintHeld;
        ctx.motor.wantCrouch = false;
    }

    public override IPlayerState TryTransition(PlayerContext ctx)
    {
        if (ctx.CrouchHeld)
        {
            SwitchState(ctx, new CrouchState());
            return null;
        }

        if (ctx.JumpPressed)
        {
            SwitchState(ctx, new JumpState());
            return null;
        }

        if (ctx.Move.sqrMagnitude <= 0.01f)
        {
            SwitchState(ctx, new IdleState());
            return null;
        }

        return null;
    }
}
