using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PhysObj : MonoBehaviour
{
    public Rigidbody2D MyRb { get; private set; }
    
    public bool isJumpable = true;
    public bool isWallJumpable = true;
    
    // private Vector2 _remainderV = new Vector2();

    protected void Start() {
        MyRb = GetComponentInChildren<Rigidbody2D>();
    }
}