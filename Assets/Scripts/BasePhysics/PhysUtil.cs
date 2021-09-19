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

    public override String ToString() {
        return "Ret: " + StopCollision + "\nPushActors: " + PushActors + "\nRideActors: " + RideActors;
    }
}

public class PhysUtil {
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
            PhysUtil.ClampScalarPos(curPos.x, velocity.x), 
            PhysUtil.ClampScalarPos(curPos.y, velocity.y)
        );
    }
}
public class InputBuffer : MonoBehaviour {
    private AutoDecrease _inputTime;
    private AutoDecrease _validTime;

    public static InputBuffer MakeInputBuffer(GameObject go, double maxInputTime, double maxValidTime) {
        InputBuffer ib = go.AddComponent<InputBuffer>();
        ib.Setup(go, maxInputTime, maxValidTime);
        return ib;
    }

    public void Setup(GameObject go, double maxInputTime, double maxValidTime) {
        _inputTime = AutoDecrease.MakeAutoDecrease(go, maxInputTime);
        _validTime = AutoDecrease.MakeAutoDecrease(go, maxValidTime);
    }

    public void NowInput() {
        _inputTime.ResetVal();
    }

    public void NowValid() {
        _validTime.ResetVal();
    }
    
    /// <summary>
    /// Returns true if the input was pressed within the buffer and before it becomes invalid.
    /// </summary>
    /// <returns></returns>
    public bool InputCanExecute() {
        return _inputTime.Val > 0 && _validTime.Val > 0;
    }

    public void InputExecute() {
        _validTime.Val = 0;
    }
}