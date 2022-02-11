using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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
    [SerializeField] private Transform gunTarget;
    [SerializeField] private Transform cameraTarget;  
    private bool setTarget = false;

    //Guns
    [SerializeField] Item[] items;
    int itemIndex;
    int previousItemIndex = -1;

    void Update()
    {
        UpdateMovementInput();
        UpdateWeapon();
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

    private void UpdateWeapon()
    {
        //Switch guns with numbers
        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }
        //Switch guns with scroll wheel
        if (inputManager.SwitchWeaponUp)
        {
            if (itemIndex >= (items.Length - 1))
                EquipItem(0);
            else
                EquipItem(itemIndex + 1);
        }
        if (inputManager.SwitchWeaponDown)
        {
            if (itemIndex <= 0)
                EquipItem(items.Length - 1);
            else
                EquipItem(itemIndex - 1);
        }

        //Fire
        FireWeapon();
    }

    private void EquipItem(int _index)
    {
        if (_index == previousItemIndex) //safe check 
            return;

        //Set current index
        itemIndex = _index;

        //Set current gun to true
        foreach (var item in items[itemIndex].itemObject)
        {
            item.SetActive(true); 
        }

        //Set the previous gun to false
        if (previousItemIndex != -1)
        {
            foreach (var item in items[itemIndex].itemObject)
            {
                item.SetActive(false);
            } 
        }
        previousItemIndex = itemIndex;

        //if (pv.IsMine)
        //{
        //    Hashtable hash = new Hashtable();
        //    hash.Add("itemIndex", itemIndex);
        //    PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        //}
    }

    private void FireWeapon()
    {
        if(inputManager.FireSingleShot)
        {
            items[itemIndex].Use();
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

        if (!setTarget)
        {
            cameraHolder.transform.rotation = Quaternion.Euler(Vector3.zero);
            gunTarget.SetParent(cameraHolder.transform); 
            setTarget = true;
        }
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
