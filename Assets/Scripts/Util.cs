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
    
    /// <summary>
    /// Returns Ceiling for imprecise floats - ie C(20.001)=20.
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static double FloatCeiling(float f) {
        double mathCeiling = Math.Ceiling(f);
        double preciseCeiling = Math.Ceiling(f - PrecisionEpsilon);
        if (preciseCeiling < mathCeiling) return preciseCeiling;
        else return mathCeiling;
        //  a       r   m
        //  20.01   20  21 *
        //  20.5    21  21
        //  20.99   21  21
        // -20.01  -20 -20
        // -20.5   -20 -20
        // -20.99  -21 -20 *
    }
    
    /// <summary>
    /// Returns Ceiling for imprecise floats - ie C(20.001)=20.
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static double FloatFloor(float f) {
        double mathFloor = Math.Floor(f);
        double preciseFloor = Math.Floor(f + PrecisionEpsilon);
        if (preciseFloor > mathFloor) return preciseFloor;
        else return mathFloor;
        //  a       r   m
        //  20.01   19  20
        //  20.5    20  20
        //  20.99   21  20 *
        // -20.01  -20 -21 *
        // -20.5   -21 -21
        // -20.99  -21 -21
    }
    
    /// <summary>
    /// Snaps the current position to the nearest pixel based on velocity.
    /// </summary>
    /// <param name="curPos"></param>
    /// <param name="velocity"></param>
    /// <returns></returns>
    public static float ClampScalarPos(float curPos, float velocity) {
        return (float) (velocity > 0 ? FloatFloor(curPos) : FloatCeiling(curPos));
    }

    public static Vector2 ClampScalarPos(Vector2 curPos, Vector2 velocity) {
        return new Vector2(
            Util.ClampScalarPos(curPos.x, velocity.x), 
            Util.ClampScalarPos(curPos.y, velocity.y)
        );
    }
}