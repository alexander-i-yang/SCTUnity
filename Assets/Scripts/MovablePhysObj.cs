using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MovablePhysObj : PhysObj
{
    // Start is called before the first frame update
    public const float gravUp = -200;
    public const float gravDown = -220f;
    public const float TerminalVelocity = -360f;

    protected bool IsGrounded { get; private set; }
    public SideCollider SideColliders { get; private set; }
    private Vector2 _remainderV = new Vector2();

    protected new void Start() {
        base.Start();
        SideColliders = gameObject.GetComponent<SideCollider>();
        if (!SideColliders) {
            throw new Exception("Forget to add a side collider on " + gameObject + "?");
        }
    }

    private float clampScalarPos(float curPos, float v) {
        return (float) (v > 0 ? Math.Floor(curPos) : Math.Ceiling(curPos));
    }

    private void OnCollisionEnter2D(Collision2D other) {
        Debug.Break();
    }

    protected void FixedUpdate() {
        Vector2 velocity = MyRb.velocity;
        bool wasGrounded = IsGrounded;

        IsGrounded = SideColliders.IsGrounded();
        if (!IsGrounded) {
            Fall();
        }

        Vector2 curPos = MyRb.transform.position;
        Vector2 newPos = curPos + velocity * Time.fixedDeltaTime;
        Vector2 clampedNewPos = new Vector2(
            clampScalarPos(newPos.x, velocity.x), 
            clampScalarPos(newPos.y, MyRb.velocity.y));
        MyRb.MovePosition(clampedNewPos);
    }

    protected bool CheckGrounded() {
        float result = DistanceToGround(0.02f);
        return result != 0 && result <= 0.02f;
    }

    protected float DistanceToGround(float distanceToCheck) {
        return 0.0f;
    }

    public void Fall() {
        Vector2 velocity = MyRb.velocity;
        MyRb.velocity = new Vector3(velocity.x, Math.Max(velocity.y + (velocity.y > 0 ? gravUp : gravDown) * Time.fixedDeltaTime, TerminalVelocity), 0);
    }
}