using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public Transform[] tireTransforms; //wheel transforms in the car, in the following order: FrontLeft, FrontRight, BackLeft, BackRight.
    public float maxRotationSpeed = 100f;
    public float rotationLerpSpeed = 10f;
    public float steeringAngle = 30f;

    private float currentRotationSpeed = 0f;
    private float currentSteeringAngle = 0f;

    private void FixedUpdate()
{
    float speed = carRigidbody.velocity.magnitude;
    currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, speed * maxRotationSpeed, rotationLerpSpeed * Time.deltaTime);

    foreach (Transform tire in tireTransforms)
    {
        // Rotate around the Z-axis
        tire.Rotate(Vector3.forward, currentRotationSpeed * Time.deltaTime);
    }

    float horizontalInput = Input.GetAxis("Horizontal");
    currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, horizontalInput * steeringAngle, rotationLerpSpeed * Time.deltaTime);

    // Rotate around the Y-axis (left/right) and Z-axis (front/back)
    tireTransforms[0].localRotation = Quaternion.Euler(0f, currentSteeringAngle, -currentRotationSpeed);
    tireTransforms[1].localRotation = Quaternion.Euler(0f, currentSteeringAngle, -currentRotationSpeed);
}

}
