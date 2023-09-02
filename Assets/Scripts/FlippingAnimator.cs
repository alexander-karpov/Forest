using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FlippingAnimator : MonoBehaviour
{
    public GameObject Anchor;
    public CreatureController Creature;

    Bounds bounds;
    float flipDistanceThreshold;
    Vector3 lastFlipPosition;
    int flipsCount;

    // Start is called before the first frame update
    void Start()
    {
        bounds = GetBounds();
        flipDistanceThreshold = CalcFlipDistanceThreshold(bounds);
        transform.position = Anchor.transform.position;
        lastFlipPosition = Anchor.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var sqrDistanceFromLastFlip = (Anchor.transform.position - lastFlipPosition).sqrMagnitude;

        if (sqrDistanceFromLastFlip > flipDistanceThreshold * flipDistanceThreshold)
        {
            flipsCount++;
            lastFlipPosition = Anchor.transform.position;

            var anchorAngles = Anchor.transform.rotation.eulerAngles;

            transform.rotation = Quaternion.Euler(90f * flipsCount - anchorAngles.z, 90, 0);

            transform.position = Anchor.transform.position;

            // if (flipsCount % 4 == 2)
            // {
            //     transform.position += (Vector3)Creature.GroundNormal * bounds.size.y;
            // }

            var normal = Creature.GroundNormal;
            var perp = Vector2.Perpendicular(normal);

            var offsets = new Vector2[] {
                new Vector2(0, 0),
        new Vector2(bounds.size.y / 2,  bounds.size.z / 2),
                new Vector2(0, bounds.size.y),
              new Vector2(-bounds.size.y / 2,  bounds.size.z / 2),
            };

            var offs = offsets[flipsCount % 4];
            var x = offs.x;
            var y = offs.y;


            transform.position = Anchor.transform.position + (Vector3)(
              perp * x + normal * y
            );
        }

    }

    private float CalcFlipDistanceThreshold(Bounds bounds)
    {
        return bounds.size.y / 2 + bounds.size.z / 2;
    }

    private Bounds GetBounds()
    {
        var meshFilter = GetComponent<MeshFilter>();
        Assert.IsNotNull(meshFilter);

        var mesh = meshFilter.mesh;
        Assert.IsNotNull(mesh);

        return mesh.bounds;
    }
}
