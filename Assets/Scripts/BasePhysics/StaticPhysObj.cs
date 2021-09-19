using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class StaticPhysObj : MonoBehaviour
{
    public Rigidbody2D MyRb { get; private set; }

    // public bool isJumpable = true;
    [NonSerialized] public bool IsMovable = false;
    
    // private Vector2 _remainderV = new Vector2();

    protected void Start() {
        MyRb = GetComponentInChildren<Rigidbody2D>();
    }

    public bool IsWallJumpable() {
        return true;
    }
}