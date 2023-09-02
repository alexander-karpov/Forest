using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class CreatureController : MonoBehaviour
{
    [SerializeField]
    public ContactFilter2D GroundFilter;

    [SerializeField]
    public float Speed = 5f;

    public Vector2 GroundNormal { get; private set; } = Vector2.up;

    // bool isFacingRight = true;
    // Start is called before the first frame update
    void Start()
    {
        Assert.AreNotEqual(0, GroundFilter.layerMask, "Задан слой для земли");
    }

    void Update()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");

        Move(horizontal);
        Flip(horizontal);
    }


    void Move(float horizontal)
    {
        if (horizontal == 0)
        {
            return;
        }

        var directionAlongGround = Vector2.Perpendicular(GroundNormal) * -horizontal;
        var movement = directionAlongGround * Speed * Time.deltaTime;

        var (normal, point) = CastToGround(movement + (Vector2)transform.position);
        GroundNormal = normal;

        transform.position = point;
        transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, GroundNormal));
    }

    void Flip(float horizontal)
    {
        var isFacingRight = horizontal > 0;
        var isFacingLeft = horizontal < 0;

        if (
            isFacingRight && transform.localScale.x < 0 ||
            isFacingLeft && transform.localScale.x > 0
        )
        {
            var s = transform.localScale;
            s.x *= -1;
            transform.localScale = s;
        }
    }

    (Vector2 normal, Vector2 point) CastToGround(Vector2 position)
    {
        // Если не поднять точку, то она пропустит землю,
        // если мы в неё немного провалились
        var pointAbove = position + GroundNormal;
        var collisions = new RaycastHit2D[1];

        int count =
            Physics2D
                .RaycastNonAlloc(pointAbove,
                -GroundNormal,
                collisions,
                // Расстояние обязательно, без него не работает маска
                // https://answers.unity.com/questions/1699320/why-wont-physicsraycastnonalloc-mask-layers-proper.html
                100f,
                GroundFilter.layerMask);

        if (count != 0)
        {
            return (collisions[0].normal, collisions[0].point);
        }

        return (GroundNormal, position);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, GroundNormal);
    }
}
