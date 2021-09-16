using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.PlayerLoop;

public class PlayerController : MovablePhysObj {
    public const float xSpeed = 80;
    public const float jumpVelocity = 100;

    public BoxCollider2D Hitbox { get; private set; }

    public const float MAXCoyoteTime = 0.133f;
    public const float MAXJjp = 0.133f;
    private float _curCoyoteTime = 0;
    private float _curJjp = 0; //Jump just pressed - aka the time since the player last pressed jump

    private bool _inputJump = false;

    // Start is called before the first frame update

    new void Start() {
        base.Start();
        Hitbox = GetComponent<BoxCollider2D>();
    }

    public override bool OnCollide(StaticPhysObj collideWith, Vector2 direction) {
        print("Player collided with: " + collideWith.transform + "direction: " + direction);
        return true;
    }

    void Update() {
        if (!_inputJump) _inputJump = CheckInputJump();
        
        if (Input.GetKeyDown("r")) {
            transform.position = new Vector2(40, 20);
        }
    }

    new void FixedUpdate() {
        if (_inputJump) {
            _curJjp = MAXJjp;
            _inputJump = false;
        }
        else if (_curJjp > 0) {
            _curJjp = Math.Max(0, _curJjp - Time.deltaTime);
        }

        if (IsGrounded) {
            _curCoyoteTime = MAXCoyoteTime;
            if (_curJjp > 0) {
                Jump();
            }
        }
        else {
            if (_curCoyoteTime > 0 && _curJjp > 0) {
                Jump();
            }
        }

        _curCoyoteTime = Math.Max(0, _curCoyoteTime - Time.deltaTime);

        float inputH = CheckInputH();
        MoveH(inputH);
        base.FixedUpdate();
    }

    /*protected RaycastHit2D DistanceToLayerLeft(float distanceToCheck, LayerMask layerMask) {
        RaycastHit2D bottomLeftRaycast = Physics2D.Raycast(bottomLeft, Vector2.left, distanceToCheck, layerMask);
        RaycastHit2D topLeftRaycast = Physics2D.Raycast(topLeft, Vector2.left, distanceToCheck, layerMask);
        RaycastHit2D raycastHits = 
        if (bottomLeftRaycast.collider) return bottomLeftRaycast;
        else if(bottomRig)
        RaycastHit2D bottomRightRaycast = Physics2D.Raycast(bottomRight, Vector2.down, distanceToCheck, layerMask);
        return Math.Max(bottomLeftRaycast.distance, bottomRightRaycast.distance);
    }*/

    float CheckInputH() {
        bool leftDown = Input.GetKey("left");
        bool rightDown = Input.GetKey("right");
        return leftDown ? -1 : (rightDown ? 1 : 0);
    }
    
    bool CheckInputJump() {
        bool zDown = Input.GetKeyDown("z");
        return zDown;
    }

    void MoveH(float move) {
        if (move != 0) {
            TotalVelocity = new Vector2(xSpeed*move, TotalVelocity.y);
        } else {
            TotalVelocity = new Vector2(0, TotalVelocity.y);
        }
        print("MoveH: " + TotalVelocity);
    }
    
    void Jump() {
        _curCoyoteTime = 0;
        TotalVelocity = new Vector2(TotalVelocity.x, jumpVelocity);
    }
}
