using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastSuspension : MonoBehaviour
{
    public float suspensionLength = 0.2f;  // Length of suspension
    public float suspensionForce = 500.0f;  // Force to apply when suspension is compressed
    public float dampingCoefficient = 50.0f; // Damping coefficient to apply based on current velocity
    public float rotationalFriction = 100.0f;
    public LayerMask raycastLayerMask;  // Layer mask to use for raycasts
    public Transform[] cornerPoints;  // Array of corner point transforms
    public Color groundDetectedColor = Color.green; // Color of the ray when the ground is detected
    public Color groundNotDetectedColor = Color.red; // Color of the ray when the ground is not detected
    public float stopTime = 3f;
    private Rigidbody rb;  // Rigidbody of the game object
    private Vector3[] prevVelocities; // Array to store the previous velocities of the corner points
    public bool hasStoppedMoving; // Flag to indicate if the corner points have stopped moving
    public float frictionCoefficient = 0.8f; // Coefficient of friction to apply when the car is on the ground

    public WheelPlacer wheelPlacer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();  // Get the rigidbody of the game object
        prevVelocities = new Vector3[cornerPoints.Length]; // Initialize the array to store the previous velocities
        hasStoppedMoving = false; // Initialize the flag
    }

//private bool hasStoppedMoving = false; // Whether the car has stopped moving

public void FixedUpdate()
{
    bool isOnGround = IsOnGround();

    for (int i = 0; i < cornerPoints.Length; i++)
    {
        Transform cornerPoint = cornerPoints[i];

        RaycastHit hit;
        Vector3 raycastOrigin = cornerPoint.position;
        Vector3 raycastDirection = -cornerPoint.up;
        float raycastDistance = suspensionLength;

        // Perform the raycast
        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, raycastDistance, raycastLayerMask))
        {
            // Calculate the compression distance
            float compressionDistance = suspensionLength - hit.distance;

            // Calculate the force to apply
            Vector3 suspensionForceVector = cornerPoint.up * suspensionForce * compressionDistance;

            // Calculate damping force based on current velocity
            Vector3 dampingForce = -rb.GetPointVelocity(cornerPoint.position) * dampingCoefficient;

            // Get the current Y-axis velocity of the rigidbody
            Vector3 currentVelocity = rb.velocity;
            float currentYVelocity = currentVelocity.y;

            // Apply the force to the rigidbody's Y-axis velocity only
            rb.AddForceAtPosition(new Vector3(0f, suspensionForceVector.y + dampingForce.y - currentYVelocity, 0f), cornerPoint.position);

            // Calculate regular friction force
            Vector3 contactPointVelocity = rb.GetPointVelocity(hit.point);
            Vector3 contactPointForwardVelocity = Vector3.Project(contactPointVelocity, cornerPoint.forward);
            Vector3 frictionDirection = -contactPointForwardVelocity.normalized;
            Vector3 frictionForce = frictionDirection * frictionCoefficient * suspensionForce * compressionDistance;

            // Apply regular friction force to rigidbody
            rb.AddForceAtPosition(frictionForce, cornerPoint.position);

            // Draw a debug ray and change its color
            Debug.DrawRay(raycastOrigin, raycastDirection * hit.distance, groundDetectedColor);
        }
        else
        {
            // Draw a debug ray and change its color
            Debug.DrawRay(raycastOrigin, raycastDirection * raycastDistance, groundNotDetectedColor);
        }
    }

    if (isOnGround)
    {
        // Check if the car has stopped moving
        if (rb.velocity.magnitude < 0.01f && rb.angularVelocity.magnitude < 0.01f && Time.time > stopTime + 3f)
        {
            // Freeze all constraints on the rigidbody
            // Set all velocities to zero
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            // Unfreeze all constraints on the rigidbody
            rb.constraints = RigidbodyConstraints.None;

            // Calculate the rotational friction torque
            Vector3 angularVelocity = rb.angularVelocity;
            Vector3 frictionTorque = -angularVelocity.normalized * rotationalFriction;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            Vector3 worldFrictionTorque = frictionTorque;

            // Convert the friction torque to world space
            Quaternion rotation = rb.rotation;
            worldFrictionTorque = rotationMatrix.MultiplyVector(worldFrictionTorque);

            // Apply the rotational friction torque to the rigidbody
            rb.AddTorque(worldFrictionTorque, ForceMode.Impulse);
        }
    }
}

public Vector3[] GetCornerHitPoints()
{
    Vector3[] hitPoints = new Vector3[cornerPoints.Length];

    for (int i = 0; i < cornerPoints.Length; i++)
    {
        Transform cornerPoint = cornerPoints[i];

        RaycastHit hit;
        Vector3 raycastOrigin = cornerPoint.position;
        Vector3 raycastDirection = -cornerPoint.up;
        float raycastDistance = suspensionLength;

        // Perform the raycast
        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, raycastDistance, raycastLayerMask))
        {
            hitPoints[i] = hit.point;
        }
        else
        {
            hitPoints[i] = raycastOrigin - (raycastDirection * -raycastDistance);
        }
    }

    return hitPoints;
}




// Coroutine to stop the car after a delay
private IEnumerator StopCar()
{
    yield return new WaitForSeconds(3f);

    // Constrain the rigidbody and set all velocities to zero

    rb.velocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;

    hasStoppedMoving = true;
}

public bool IsOnGround()
{
    for (int i = 0; i < cornerPoints.Length; i++)
    {
        Transform cornerPoint = cornerPoints[i];

        RaycastHit hit;
        Vector3 raycastOrigin = cornerPoint.position;
        Vector3 raycastDirection = -cornerPoint.up;
        float raycastDistance = suspensionLength;

        // Perform the raycast
        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, raycastDistance, raycastLayerMask))
        {
            return true;
        }
    }

    return false;
}


}