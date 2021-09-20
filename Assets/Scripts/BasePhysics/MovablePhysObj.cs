using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public abstract class MovablePhysObj : StaticPhysObj
{
    // Start is called before the first frame update
    public float gravUp = -150;
    public float gravDown = -300;
    public float TerminalVelocity = -350;

    protected bool IsGrounded { get; private set; }
    public SideCollider SideColliders { get; private set; }

    public BoxCollider2D MyCollider { get; private set; }
    [NonSerialized] public Vector2 TotalVelocity = Vector2.zero;

    private bool _shouldDebug = false;
    
    protected new void Start() {
        base.Start();
        IsMovable = true;
        SideColliders = gameObject.GetComponent<SideCollider>();
        MyCollider = gameObject.GetComponent<BoxCollider2D>();
        if (!SideColliders) {
            throw new Exception("Forget to add a side collider on " + gameObject + "?");
        }
        if (!MyCollider) {
            throw new Exception("Forget to add a box collider on " + gameObject + "?");
        }
    }

    protected void FixedUpdate() {
        IsGrounded = SideColliders.IsGrounded();
        if (!IsGrounded) {
            Fall();
        }
        else {
            TotalVelocity = new Vector2(TotalVelocity.x, Math.Max(TotalVelocity.y, 0));
        }

        Move();
    }

    protected virtual float GetGravity() {
        return TotalVelocity.y > 0 ? gravUp : gravDown;
    }

    public void Fall() {
        TotalVelocity = new Vector2(TotalVelocity.x, Math.Max(TotalVelocity.y + GetGravity() * Time.fixedDeltaTime, TerminalVelocity));
    }

    protected List<StaticPhysObj> GetCollidingStatics(Vector2 direction, Vector2 futureOffset) {
        RaycastHit2D[] willCollide = CheckOneForward(direction, futureOffset, LayerMask.GetMask("WorldStatic"));
        List<StaticPhysObj> ret = new List<StaticPhysObj>();
        foreach (RaycastHit2D hit in willCollide) {
            if (_shouldDebug) {
                // print("Collide point: " + hit.point);
                // print("Direction: " + direction);
                // print("Origin: " + (Vector2) transform.position + MyCollider.offset);
                // print("Size: " + MyCollider.size);
            }
            ret.Add(hit.transform.GetComponent<StaticPhysObj>());
        }
        return ret;
    }

    //TODO: should pass futureOffset?
    protected List<MovablePhysObj> GetAllRidingActors(Vector2 futureOffset) {
        return GetAllPushingActors(Vector2.up, futureOffset);
    }

    private RaycastHit2D[] CheckOneForward(Vector2 direction, Vector2 futureOffset, LayerMask l) {
        return Physics2D.BoxCastAll(  
            origin: (Vector2) transform.position + MyCollider.offset+futureOffset,
            size: MyCollider.size*0.95f,
            angle: 0.0f,
            direction: direction,
            distance: 1,    
            l
        );
    }

    public List<MovablePhysObj> GetAllPushingActors(Vector2 direction, Vector2 futureOffset) {
        RaycastHit2D[] willCollide = CheckOneForward(direction, futureOffset, LayerMask.GetMask("WorldMovable"));
        List<MovablePhysObj> ret = new List<MovablePhysObj>();
        foreach (RaycastHit2D hit in willCollide) {
            ret.Add(hit.transform.GetComponent<MovablePhysObj>());
        }
        return ret;
    }
    
    MyCollisionData CheckGeneralCollisions(Vector2 direction, Vector2 futureOffset) {
        MyCollisionData retObj = new MyCollisionData(false);
        
        List<StaticPhysObj> collideStatics = GetCollidingStatics(direction, futureOffset);
        foreach(StaticPhysObj collideStatic in collideStatics) {
            // print("Collide Static: " + collideStatic);
            if (OnCollide(collideStatic, direction)) {
                retObj.StopCollision = true;
                return retObj;
            }
        }
        
        List<MovablePhysObj> pushingActors = GetAllPushingActors(direction, futureOffset);
        List<MovablePhysObj> ridingActors = GetAllRidingActors(futureOffset);
        foreach (MovablePhysObj actor in pushingActors) {
            // print(actor);
            // print(this);
            // print(actor == this);
            // print(actor.gameObject);
            // Debug.Break();
            if (actor != this) {
                if(!CanPush(actor, direction) || actor.CheckGeneralCollisions(direction, futureOffset).StopCollision) {
                    // print("Collide Actor: " + actor);
                    retObj.StopCollision = OnCollide(actor, direction);
                    return retObj;
                }
                retObj.PushActors.Add(actor);
            }
        }
        
        foreach (MovablePhysObj actor in ridingActors) {
            if (pushingActors.Contains(actor) || actor == this) continue;
            else if (!actor.CanRide(this, direction)) {
                
            }
            else {
                retObj.RideActors.Add(actor);
            }
        }

        return retObj;
    }

    public bool CanRide(MovablePhysObj ridingOn, Vector2 direction) {
        return true;
    }

    private bool CanPush(MovablePhysObj pushObj, Vector2 direction) {
        return true;
    }

    /// <summary>
    /// Enacts collision physics.
    /// </summary>
    /// <param name="collideWith"></param>
    /// <param name="direction"></param>
    /// <returns>True if collision results in stopping motion</returns>
    abstract public bool OnCollide(StaticPhysObj collideWith, Vector2 futureOffset);
    
    //TODO: fix future offset bug.
    private int MoveOneAxis(Vector2 velocity, Vector2 futureOffset) {
        //Round to avoid precision errors when typecasting to int
        int magnitude = (int) Math.Round(velocity.x != 0 ? velocity.x : velocity.y);
        Vector2 direction = new Vector2(Math.Sign(velocity.x), Math.Sign(velocity.y));
        int ret = 0;
        if (velocity.y == 0) {
            // print("M1A V: " + velocity);
            // print("M1A Dir: " + direction);
            // print("M1A Mag: " + magnitude);
        }

        while (magnitude != 0) {
            MyCollisionData collisionData = CheckGeneralCollisions(direction, futureOffset);
            if (direction.y != 0) {
                // print("CollisionData: " + collisionData.ToString());
            }

            if (collisionData.StopCollision) {
                if(_shouldDebug) {
                    print("Stop collision");
                }
                // Debug.Break();
                return ret;
            }

            foreach (MovablePhysObj actor in collisionData.PushActors) {
                actor.MoveOneAxis(direction, futureOffset);
            }
            foreach (MovablePhysObj actor in collisionData.RideActors) {
                actor.MoveOneAxis(direction, futureOffset);
            }
            
            ret += Math.Sign(magnitude);
            magnitude -= Math.Sign(magnitude);
            // MyRb.MovePosition(MyRb.position+direction);
            if (direction.x != 0 && _shouldDebug) {
                print($"D: {direction}");
                print($"OP: {MyRb.position}");
                print($"NP: {MyRb.position}");
            }
            futureOffset += direction;
        }
        print(ret + " " + futureOffset);
        return (int) (ret+(futureOffset.x == 0 ? futureOffset.y : futureOffset.x));
    }

    private void Move() {
        Vector2 curPos = MyRb.transform.position;
        // Store newpos as doubles to avoid precision errors
        Vector2 newPos = curPos + TotalVelocity * Time.fixedDeltaTime;
        Vector2 newClampedPos = PhysUtil.ClampScalarPos(newPos, TotalVelocity);
        Vector2 clampedVelocity = newClampedPos - curPos;
        //return (float) (v > 0 ? Math.Floor(curPos) : Math.Ceiling(curPos));
        int yMove = MoveOneAxis(new Vector2(0, clampedVelocity.y), Vector2.zero);
        int xMove = MoveOneAxis(new Vector2(clampedVelocity.x, 0), Vector2.zero);
        clampedVelocity = new Vector2(xMove, yMove);
        MyRb.MovePosition(curPos + clampedVelocity);

        if (_shouldDebug && TotalVelocity.x != 0) {
            print($"PV: {newClampedPos-curPos}");
            print($"AV: {clampedVelocity}");
            // Debug.Break();
        }
        else {
            // if (clampedVelocity.y > 0) _shouldDebug = true;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position+(Vector3)TotalVelocity*Time.fixedDeltaTime);
    }
}