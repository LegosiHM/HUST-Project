using UnityEngine;

public class DevHUD : MonoBehaviour
{
    public PlayerStateMachine fsm;
    public PlayerContext ctx;
    public PlayerController motor;
    public Gravity gravity;

    Rect baseRect = new Rect(10, 10, 800, 25);
    float lineHeight = 20f;

    void Awake()
    {
        if (!fsm) fsm = GetComponent<PlayerStateMachine>();
        if (!ctx) ctx = GetComponent<PlayerContext>();
        if (!motor) motor = GetComponent<PlayerController>();
        if (!gravity) gravity = GetComponent<Gravity>();
    }

    void OnGUI()
    {
        if (!fsm || !ctx || !motor || !gravity) return;

        GUI.skin.label.fontSize = 14;
        GUI.color = Color.white;

        var row = new Rect(baseRect);

        GUI.Label(row, $"STATE: {fsm.GetFullStateName()}");
        row.y += lineHeight * 1.3f;

        GUI.Label(row, $"Move: {ctx.Move}   SprintHeld: {ctx.SprintHeld}   CrouchHeld: {ctx.CrouchHeld}   JumpPressed: {ctx.JumpPressed}");
        row.y += lineHeight * 1.3f;

        GUI.Label(row, $"wantSprint: {motor.wantSprint}   wantCrouch: {motor.wantCrouch}   wantJumpPulse: {motor.wantJumpPulse}");
        row.y += lineHeight * 1.3f;

        GUI.Label(row, $"IsGrounded: {ctx.IsGrounded}  (probe: {ctx.IsGroundedProbing(0.12f)})   lastGroundedAt: {ctx.lastGroundedAt:F2}   now: {Time.time:F2}");
        row.y += lineHeight * 1.3f;

        GUI.Label(row, $"VerticalVel: {gravity.VerticalVelocity:F2}");
        row.y += lineHeight * 1.3f;
    }
}
