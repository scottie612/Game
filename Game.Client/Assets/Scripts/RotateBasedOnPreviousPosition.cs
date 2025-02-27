using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBasedOnPreviousPosition : MonoBehaviour
{

    private Vector3 _previousPosition;
    void Start()
    {
       _previousPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var currentPos = transform.position;
        var movementDirection = currentPos - _previousPosition;

        // Only rotate if we're actually moving
        if (movementDirection.magnitude > 0.001f)
        {
            // Calculate angle in degrees
            float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;

            // Set the Z rotation (for 2D)
            transform.rotation = Quaternion.Euler(0, 0, angle);

        }

        _previousPosition = transform.position;
    }
}
