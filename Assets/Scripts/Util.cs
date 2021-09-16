using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public struct MyCollisionData {
    public bool StopCollision;
    public readonly List<MovablePhysObj> PushActors;
    public readonly List<MovablePhysObj> RideActors;
    public MyCollisionData(bool r) {
        StopCollision = r;
        PushActors = new List<MovablePhysObj>();
        RideActors = new List<MovablePhysObj>();
    }

    public String ToString() {
        return "Ret: " + StopCollision + "\nPushActors: " + PushActors + "\nRideActors: " + RideActors;
    }
}

public class Util {
    public const float PrecisionEpsilon = 0.0001f;
    
    public double FloatCeiling(float f) {
        return Math.Ceiling(f - PrecisionEpsilon);
        
    }

    public double FloatFloor(float f) {
        double mathFloor = Math.Floor(f);
        double preciseFloor = Math.Floor(f + PrecisionEpsilon);
        if (preciseFloor > mathFloor) return preciseFloor;
        else return mathFloor;
        return Math.Floor(f - PrecisionEpsilon);
        //  a       r   m
        //  20.01   20  20
        //  20.5    20  20
        //  20.99   21  20
        // -20.01  -20 -21
        // -20.5   -21 -21
        // -20.99  -21 -21
    }
}