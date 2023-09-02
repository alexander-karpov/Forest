using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FlippingAnimator : MonoBehaviour
{
    public GameObject Anchor;

    float _flipDistanceThreshold;
    Vector3 _lastFlipAnchorPosition;
    float _lastFlipAnchorAngle;
    float _lastFlipTime;

    /// <summary>
    /// Фаза вращения от 0 до 3
    /// </summary>
    int _flipIndex;

    /// <summary>
    /// Значения для корректировки позиции при вращении
    /// Массив из четырёх элементов. Индекс – flipsCount % 4
    /// </summary>
    Tuple<float, float>[] _flippedPositionCorrections;

    // Start is called before the first frame update
    void Start()
    {
        var bounds = GetBounds();
        _flipDistanceThreshold = CalcFlipDistanceThreshold(bounds);
        transform.position = Anchor.transform.position;
        _lastFlipAnchorPosition = Anchor.transform.position;

        _flippedPositionCorrections = new Tuple<float, float>[] {
            new(0, 0),
            new(bounds.size.y / 2, bounds.size.z / 2),
            new(0, bounds.size.y),
            new(-bounds.size.y / 2, bounds.size.z / 2),
        };
    }

    // Update is called once per frame
    void Update()
    {
        FlipUpIfNotMove();
        Flip();
    }

    private void FlipUpIfNotMove()
    {
        var timeSinceLastFlip = Time.timeSinceLevelLoad - _lastFlipTime;

        if (_flipIndex != 0 && timeSinceLastFlip > 1f)
        {
            _flipIndex = 0;
            _lastFlipTime = Time.timeSinceLevelLoad;

            UpdatePositionAndRotation();
        }
    }

    private void Flip()
    {
        if (!IsTimeToFlip())
        {
            return;
        }

        IncreaseFlipIndex();

        _lastFlipAnchorPosition = Anchor.transform.position;
        _lastFlipAnchorAngle = Anchor.transform.rotation.eulerAngles.z;
        _lastFlipTime = Time.timeSinceLevelLoad;

        UpdatePositionAndRotation();
        SyncScaleX();
    }

    private void UpdatePositionAndRotation()
    {
        var (x, y) = _flippedPositionCorrections[_flipIndex];
        var normal = (Vector2)(Anchor.transform.rotation * Vector3.up);
        var perp = Vector2.Perpendicular(normal);

        transform.SetPositionAndRotation(
            _lastFlipAnchorPosition + (Vector3)(perp * x + normal * y),
            Quaternion.Euler(0, 0, -90f * _flipIndex + _lastFlipAnchorAngle)
        );
    }

    private bool IsTimeToFlip()
    {
        return (Anchor.transform.position - _lastFlipAnchorPosition).sqrMagnitude > _flipDistanceThreshold * _flipDistanceThreshold;
    }

    private void IncreaseFlipIndex()
    {
        if (Anchor.transform.localScale.x > 0)
        {
            _flipIndex++;

            if (_flipIndex == 4)
            {
                _flipIndex = 0;
            }
        }
        else
        {
            _flipIndex--;

            if (_flipIndex == -1)
            {
                _flipIndex = 3;
            }
        }
    }

    private void SyncScaleX()
    {
        if (Mathf.Sign(Anchor.transform.localScale.x) != Mathf.Sign(transform.localScale.x))
        {
            var scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private float CalcFlipDistanceThreshold(Bounds bounds)
    {
        return bounds.size.y / 2 + bounds.size.z / 2;
    }

    private Bounds GetBounds()
    {
        var meshFilter = GetComponentInChildren<MeshFilter>();
        Assert.IsNotNull(meshFilter, "Ожидает вложенный компонент с MeshFilter");

        var mesh = meshFilter.mesh;
        Assert.IsNotNull(mesh);

        return mesh.bounds;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
