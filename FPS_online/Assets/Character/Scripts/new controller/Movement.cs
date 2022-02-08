using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameObject cameraHolder;
    private Vector3 movementDir;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider col;
    [SerializeField] private float offsetFloorY = 0.4f;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float xAxisSensitivity = 0.2f;
    [SerializeField] private float yAxisSensitivity = 0.2f;
    private Vector3 raycastFloorPos;
    private Vector3 combinedRaycast;
    private Vector3 gravity;
    private Vector3 floorMovement;
    private float groundRayLenght;
    private bool isGrounded;
    private bool IsGrounded { get { return gravity.y <= 0.0f; } }
    [SerializeField] private float jumpForce = 200.0f;

    private float verticalLookRotation = 0.0f;


    void Update()
    {
        UpdateMovementInput();
    }

    void FixedUpdate()
    { 
        UpdatePhysics();
    }
     

    private void UpdateMovementInput()
    {
        Vector3 forward = inputManager.Forward ? transform.forward : inputManager.Back ? -1 * transform.forward : Vector3.zero;
        Vector3 sideway = inputManager.Left ? -1 * transform.right : inputManager.Right ? transform.right : Vector3.zero;
        Vector3 combinedInput = (forward + sideway).normalized;
         
        movementDir = new Vector3(combinedInput.x, 0f, combinedInput.z);

        Look();
        if (inputManager.Jump && !inputManager.Crouch && IsGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Acceleration);
        }
    }

    private void UpdatePhysics()
    {
        //Set the raycast length to half collider + custom offset
        groundRayLenght = (col.height * 0.5f) + offsetFloorY;
        if (FloorRaycasts(0, 0, groundRayLenght).transform == null)
        {
            gravity += (Vector3.up * Physics.gravity.y * Time.fixedDeltaTime);
        }

        //Update velocity
        rb.velocity = (movementDir * movementSpeed) + gravity;

        
        //Adjust rigid body position so that the player is at the correct height
        floorMovement = new Vector3(rb.position.x, FindFloor().y, rb.position.z);
        if (FloorRaycasts(0, 0, groundRayLenght).transform != null && floorMovement != rb.position)
        {
            rb.MovePosition(floorMovement);
            gravity.y = 0;
        }
    }

    private void Look()
    {
        //Rotate player 
        transform.Rotate(Vector3.up * inputManager.XLookAxis);

        //Rotate camera
        verticalLookRotation += inputManager.YLookAxis * yAxisSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -70.0f, 70f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    private Vector3 FindFloor()
    {
        //Determine the average point of the floor between the 5 raycasts
        float raycastWidth = 0.25f;
        int floorAverage = 1;
        combinedRaycast = FloorRaycasts(0, 0, groundRayLenght).point;
        floorAverage += (GetFloorAverage(raycastWidth, 0) + GetFloorAverage(-raycastWidth, 0) + GetFloorAverage(0, raycastWidth) + GetFloorAverage(0, -raycastWidth));
        return combinedRaycast / floorAverage;
    }

    private RaycastHit FloorRaycasts(float t_offsetx, float t_offsetz, float t_raycastLength)
    {
        RaycastHit hit;

        raycastFloorPos = transform.TransformPoint(0.0f + t_offsetx, col.center.y, 0.0f + t_offsetz);

        Debug.DrawRay(raycastFloorPos, Vector3.down * groundRayLenght, Color.magenta);

        Physics.Raycast(raycastFloorPos, -Vector3.up, out hit, t_raycastLength);

        return hit;
    }

    private int GetFloorAverage(float t_offsetx, float t_offsetz)
    {
        if (FloorRaycasts(t_offsetx, t_offsetz, groundRayLenght).transform != null)
        {
            combinedRaycast += FloorRaycasts(t_offsetx, t_offsetz, groundRayLenght).point;
            return 1;
        }
        else
            return 0;
    }
}
