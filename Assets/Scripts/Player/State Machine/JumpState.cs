using UnityEngine;

public class JumpState : BaseState
{
    private bool leftGround;
    private float t;
    private float groundedHold;

    private const float FailSafeAir = 0.35f;
    private const float MaxAirTime = 2.0f;
    private const float GroundHyst = 0.06f;

    public override void Enter(PlayerContext ctx)
    {
        leftGround = false;
        t = 0f;
        groundedHold = 0f;

        if (ctx.motor != null)
        {
            // trigger jump
            ctx.motor.wantJumpPulse = true;

            // JUMP SFX (only once when the jump actually starts)
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX("sfx_jump");
            }
        }
    }

    public override void Tick(PlayerContext ctx, float dt)
    {
        t += dt;

        if (ctx.motor != null)
            ctx.motor.wantSprint = ctx.SprintHeld;

        bool groundedNow = ctx.IsGrounded || ctx.IsGroundedProbing(0.12f);

        if (!leftGround && !groundedNow)
            leftGround = true;

        if (leftGround && groundedNow)
            groundedHold += dt;
        else
            groundedHold = 0f;

        // Future: can call _subState?.UpdateStates(ctx, dt);
    }

    public override IPlayerState TryTransition(PlayerContext ctx)
    {
        // 1) Fail-safe: never actually left the ground → NO landing sound
        if (!leftGround && t >= FailSafeAir)
        {
            return new GroundedState();
        }

        // 2) Proper landing: was in air, then grounded for a short time
        if (leftGround && groundedHold >= GroundHyst)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX("sfx_landing");
            }
            return new GroundedState();
        }

        // 3) Max air time bailout (e.g. weird case staying in air too long)
        if (t >= MaxAirTime)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX("sfx_landing");
            }
            return new GroundedState();
        }

        return null;
    }
}
