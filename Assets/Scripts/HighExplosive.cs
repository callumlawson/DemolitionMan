using UnityEngine;

public class HighExplosive : MonoBehaviour
{
    public int Radius;
    public int ExplosionForce;
    public float DelaySeconds;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Invoke(nameof(Explode), DelaySeconds);
        }
    }

    void Explode()
    {
        var explosivePosition = gameObject.transform.position;
        //Not going to work well with multiple colliders per object!
        var affectedObjects = Physics.OverlapSphere(transform.position, Radius);
        foreach (var coll in affectedObjects)
        {
            var rb = coll.attachedRigidbody;
            if (rb != null)
            {
                var center = coll.bounds.center;
                var vectorToTarget = rb.position - explosivePosition;
                var distance = Vector3.Magnitude(vectorToTarget);
                var directionVector = Vector3.Normalize(vectorToTarget);
                rb.AddForceAtPosition(directionVector * ExplosionForce * 1 / distance, center);
            }
        }
    }
}
