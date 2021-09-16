﻿using System;
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
    public const float gravUp = -200;
    public const float gravDown = -220f;
    public const float TerminalVelocity = -360f;

    protected bool IsGrounded { get; private set; }
    public SideCollider SideColliders { get; private set; }

    public BoxCollider2D MyCollider { get; private set; }
    [NonSerialized] public Vector2 TotalVelocity = new Vector2();

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

    public void Fall() {
        TotalVelocity = new Vector2(TotalVelocity.x, Math.Max(TotalVelocity.y + (TotalVelocity.y > 0 ? gravUp : gravDown) * Time.fixedDeltaTime, TerminalVelocity));
    }

    protected List<StaticPhysObj> GetCollidingStatics(Vector2 direction) {
        RaycastHit2D[] willCollide = CheckOneForward(direction, LayerMask.GetMask("WorldStatic"));
        List<StaticPhysObj> ret = new List<StaticPhysObj>();
        foreach (RaycastHit2D hit in willCollide) {
            print("Collide point: " + hit.point);
            print("Direction: " + direction);
            print("Origin: " + (Vector2) transform.position + MyCollider.offset);
            print("Size: " + MyCollider.size);
            Debug.Break();
            ret.Add(hit.transform.GetComponent<StaticPhysObj>());
        }
        return ret;
    }

    protected List<MovablePhysObj> GetAllRidingActors() {
        return GetAllPushingActors(Vector2.up);
    }

    private RaycastHit2D[] CheckOneForward(Vector2 direction, LayerMask l) {
        return Physics2D.BoxCastAll(  
            origin: (Vector2) transform.position + MyCollider.offset,
            size: MyCollider.size*0.95f,
            angle: 0.0f,
            direction: direction,
            distance: 1,    
            l
        );
    }

    public List<MovablePhysObj> GetAllPushingActors(Vector2 direction) {
        RaycastHit2D[] willCollide = CheckOneForward(direction, LayerMask.GetMask("WorldMovable"));
        List<MovablePhysObj> ret = new List<MovablePhysObj>();
        foreach (RaycastHit2D hit in willCollide) {
            ret.Add(hit.transform.GetComponent<MovablePhysObj>());
        }
        return ret;
    }
    
    MyCollisionData CheckGeneralCollisions(Vector2 direction) {
        MyCollisionData retObj = new MyCollisionData(false);
        
        List<StaticPhysObj> collideStatics = GetCollidingStatics(direction);
        foreach(StaticPhysObj collideStatic in collideStatics) {
            print("Collide Static: " + collideStatic);
            if (OnCollide(collideStatic, direction)) {
                retObj.StopCollision = true;
                return retObj;
            }
        }
        
        List<MovablePhysObj> pushingActors = GetAllPushingActors(direction);
        List<MovablePhysObj> ridingActors = GetAllRidingActors();
        foreach (MovablePhysObj actor in pushingActors) {
            // print(actor);
            // print(this);
            // print(actor == this);
            // print(actor.gameObject);
            // Debug.Break();
            if (actor != this) {
                if(!CanPush(actor, direction) || actor.CheckGeneralCollisions(direction).StopCollision) {
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
    abstract public bool OnCollide(StaticPhysObj collideWith, Vector2 direction);

    private int MoveOneAxis(Vector2 velocity) {
        //Round to avoid precision errors when typecasting to int
        int magnitude = (int) Math.Round(velocity.x != 0 ? velocity.x : velocity.y);
        Vector2 direction = new Vector2(Math.Sign(velocity.x), Math.Sign(velocity.y));
        int ret = 0;
        if (velocity.y == 0) {
            // print("M1A V: " + velocity);
            // print("M1A Dir: " + direction);
            // print("M1A Mag: " + magnitude);
        }

        /*if (velocity == Vector2.right && magnitude == 0) {
            print("no x needed");
            print("v: " + velocity);
            print(direction);
            print(velocity.x != 0);
            print(Util.FloatCeiling(velocity.x));
            print("mag: " + (int) (velocity.x != 0 ? velocity.x : velocity.y));
        }*/

        while (magnitude != 0) {
            MyCollisionData collisionData = CheckGeneralCollisions(direction);
            if (direction.y != 0) {
                // print("CollisionData: " + collisionData.ToString());
            }

            if (collisionData.StopCollision) {
                print("Stop collision");
                Debug.Break();
                return ret;
            }

            foreach (MovablePhysObj actor in collisionData.PushActors) {
                actor.MoveOneAxis(direction);
            }
            foreach (MovablePhysObj actor in collisionData.RideActors) {
                actor.MoveOneAxis(direction);
            }
            
            ret += Math.Sign(magnitude);
            magnitude -= Math.Sign(magnitude);
            MyRb.MovePosition(MyRb.position+direction);
            if (direction.y == 0) {
                print($"End of loop {ret}");
            }
        }
        return ret;
    }

    private void Move() {
        Vector2 curPos = MyRb.transform.position;
        // Store newpos as doubles to avoid precision errors
        Vector2 newPos = curPos + TotalVelocity * Time.fixedDeltaTime;
        Vector2 newClampedPos = Util.ClampScalarPos(newPos, TotalVelocity);
        Vector2 clampedVelocity = newClampedPos - curPos;
        //return (float) (v > 0 ? Math.Floor(curPos) : Math.Ceiling(curPos));
        int xMove = MoveOneAxis(new Vector2(clampedVelocity.x, 0));
        int yMove = MoveOneAxis(new Vector2(0, clampedVelocity.y));
        clampedVelocity = new Vector2(xMove, yMove);
        if (_shouldDebug) {
            print($"PV: {newClampedPos-curPos}");
            print($"AV: {clampedVelocity}");
            Debug.Break();
        }
        else {
            if (clampedVelocity.y > 0) _shouldDebug = true;
        }
        // MyRb.velocity = clampedVelocity/Time.fixedDeltaTime;
    }
}