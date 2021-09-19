using UnityEngine;

public class Box : MovablePhysObj {
    public override bool OnCollide(StaticPhysObj collideWith, Vector2 direction) {
        return true;
    }
}