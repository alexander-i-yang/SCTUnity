using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class SideCollider : MonoBehaviour {
    private BoxCollider2D _hitbox;
    public const float Epsilon = 0.02f;
    
    // Start is called before the first frame update
    void Start() {
        _hitbox = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update() {
        
    }

    public RaycastHit2D[] TouchingBottomObjs(LayerMask l) {
        Vector3 bottomLeft = _hitbox.bounds.min+new Vector3(0, -Epsilon, 0);
        RaycastHit2D[] bottomLeftRaycast = Physics2D.RaycastAll(bottomLeft, Vector2.right, _hitbox.size.x*transform.localScale.x, l);
        Debug.DrawLine(bottomLeft, bottomLeft+transform.localScale.x*_hitbox.size.x*Vector3.right);
        return bottomLeftRaycast;
    }

    public bool IsGrounded() {
        foreach(RaycastHit2D r in TouchingBottomObjs(LayerMask.GetMask("WorldStatic", "WorldMovable"))) {
            if (r.collider) {
                return true;
            }
        }
        return false;
    }

    public bool TouchingGround(float distanceToCheck) {
        Vector3 bottomLeft = _hitbox.bounds.min;
        Vector3 bottomRight = bottomLeft + new Vector3(_hitbox.size.x, 0, 0);
        LayerMask groundLayers = LayerMask.GetMask("World");
        RaycastHit2D bottomLeftRaycast = Physics2D.Raycast(bottomLeft, Vector2.down, distanceToCheck, groundLayers);
        RaycastHit2D bottomRightRaycast = Physics2D.Raycast(bottomRight, Vector2.down, distanceToCheck, groundLayers);
        return Math.Max(bottomLeftRaycast.distance, bottomRightRaycast.distance) > 0;
    }
}