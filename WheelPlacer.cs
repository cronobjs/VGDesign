using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelPlacer : MonoBehaviour
{
    public GameObject[] wheels;
    public RaycastSuspension raycastSuspension;
    public float yOffset = 0.05f;

    void Update()
    {
        // Get the hit points of the corner points from the RaycastSuspension script
        Vector3[] hitPoints = raycastSuspension.GetCornerHitPoints();

        // Update the position of each wheel
        for (int i = 0; i < wheels.Length; i++)
        {
            Vector3 wheelPosition = hitPoints[i] + (Vector3.up * yOffset);
            wheels[i].transform.position = wheelPosition;
        }
    }
}