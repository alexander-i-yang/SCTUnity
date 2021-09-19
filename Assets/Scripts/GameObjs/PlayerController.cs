using System;
using UnityEngine;
using UnityEditor;

public class PlayerController : MovablePhysObj {
    public double xSpeed = 50;
    public double jumpVelocity = 300;
    public double _velocityCutoff = 0.0;
    
    public BoxCollider2D Hitbox { get; private set; }

    public const float MAXCoyoteTime = 0.133f;
    public const float MAXJjp = 0.133f;
    
    private bool _inputJump = false;
    
    public AnimationCurve gravityTransitionCurve;
    public AnimationCurve gravityCurve;

    private double _jumpHeldTime = -1.0;

    private InputBuffer _jumpBuffer;

    private AutoDecrease _gravityLerpTime;
    private bool _recordingZ = false;
    private double _maxGravityLerpTime = -1;
    // Start is called before the first frame update

    new void Start() {
        base.Start();
        Hitbox = GetComponent<BoxCollider2D>();
        _jumpBuffer = InputBuffer.MakeInputBuffer(gameObject, MAXJjp, MAXCoyoteTime);

        _gravityLerpTime = AutoDecrease.MakeAutoDecrease(gameObject, 0);
    }

    public override bool OnCollide(StaticPhysObj collideWith, Vector2 direction) {
        // print("Player collided with: " + collideWith.transform + "direction: " + direction);
        CustomTag tags = collideWith.transform.GetComponent<CustomTag>();
        if (!tags) {
            throw new Exception($"Error: no tags on collidable obj {collideWith.transform}");
        } else {
            if (tags.HasTag("Ground")) {
                if (direction == Vector2.up) {
                    TotalVelocity = new Vector2(TotalVelocity.x, 0);
                } else if (direction == Vector2.down) {
                    //TODO: fix this to set to the velocity of the ground (or not, since actors should ride on solids anyway)
                    TotalVelocity = new Vector2(TotalVelocity.x, 0);
                }
            }
        }
        return true;
    }

    void Update() {
        if (!_inputJump) _inputJump = CheckInputJump();
        
        if (Input.GetKeyDown("r")) {
            transform.position = new Vector2(40, -20);
        }
    }

    private void UpdateJumpBuffer() {
        if (_inputJump) {
            _jumpBuffer.NowInput();
            _inputJump = false;
        }

        if (IsGrounded) {
            _jumpBuffer.NowValid();
        }
    }

    void UpdateTimeSinceJump(bool inputJumpHeld) {
        if (!inputJumpHeld) _recordingZ = false;
        if (_jumpHeldTime >= 0) {
            if (inputJumpHeld) {
                _jumpHeldTime += Time.fixedDeltaTime;
            } else {
                double transitionTime = gravityTransitionCurve.Evaluate((float) _jumpHeldTime);
                _maxGravityLerpTime = transitionTime;
                _gravityLerpTime.Val = _maxGravityLerpTime;
                _jumpHeldTime = -1;
            }
        }
    }

    new void FixedUpdate() {
        UpdateJumpBuffer();

        if (_jumpBuffer.InputCanExecute()) {
            _jumpBuffer.InputExecute();
            Jump();
        }

        UpdateTimeSinceJump(CheckInputJumpHeld());

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

    bool CheckInputJumpHeld() {
        return Input.GetKey("z");
    }

    void MoveH(float move) {
        if (move != 0) {
            TotalVelocity = new Vector2((float) xSpeed*move, TotalVelocity.y);
        } else {
            TotalVelocity = new Vector2(0, TotalVelocity.y);
        }
    }

    void Jump() {
        _jumpHeldTime = 0.0;
        _recordingZ = true;
        TotalVelocity = new Vector2(TotalVelocity.x, (float) jumpVelocity);
    }

    protected override float GetGravity() {
        if (TotalVelocity.y < _velocityCutoff) {
            _recordingZ = false;
            return gravDown;
        } else if (_recordingZ) {
            return gravUp/2;
        } else {
            return gravUp;
        }

        print($"GLT: {GetGravityLerpTime()}");
        return Mathf.Lerp(gravUp, gravDown, (float) GetGravityLerpTime());
    }

    double GetGravityLerpTime() {
        if (_jumpHeldTime < 0) {
            return 0;
        } else {
            print($"JHT: {_jumpHeldTime}");
            //TODO: Fix MLGT = 0 error.
            print($"MGLT: {_maxGravityLerpTime}");
            return _gravityLerpTime.Val/_maxGravityLerpTime;
        }
    }

    void OnDrawGizmosSelected() {
        
        GUIStyle largeFont = new GUIStyle();

        largeFont.fontSize = 32;
        largeFont.normal.textColor = Color.white;

        Handles.Label(new Vector2(10, -10), $"{GetGravity()}", largeFont);
        Handles.Label(new Vector2(10, -15), $"{IsGrounded}", largeFont);
        Handles.Label(new Vector2(10, -20), $"{_recordingZ}", largeFont);
        Handles.Label(new Vector2(10, -25), $"{TotalVelocity.y}", largeFont);
    }
}
