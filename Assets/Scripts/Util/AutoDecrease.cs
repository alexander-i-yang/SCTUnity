using System;
using UnityEngine;

public class AutoDecrease : MonoBehaviour {
    [NonSerialized] public double Val;
    [NonSerialized] public double Max;

    public static AutoDecrease MakeAutoDecrease(GameObject go, double m) {
        AutoDecrease ad = go.AddComponent<AutoDecrease>();
        ad.Setup(m);
        return ad;
    }

    public void Setup(double m) {
        Val = 0;
        Max = m;
    }

    public void ResetVal() {
        Val = Max;
    }

    void FixedUpdate() {
        Val = Math.Max(0, Val - Time.fixedDeltaTime);
    }
}