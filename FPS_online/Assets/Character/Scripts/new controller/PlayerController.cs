using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationController animController;
    private Vector3 movementDir;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider col;
    [SerializeField] private float offsetFloorY = 0.4f;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float speedMultiplier = 1.6f;
    [SerializeField] private float xAxisSensitivity = 0.2f;
    [SerializeField] private float yAxisSensitivity = 0.2f;
    private bool isGrounded;
    public bool IsGrounded { get { return isGrounded; } }
    [SerializeField] private float jumpForce = 200.0f;
    public float fallMultiplier;
    private Vector3 gravity;
    public bool IsRunning { get { return (!inputManager.Crouch && !inputManager.Back && inputManager.Run); } }

    private float verticalLookRotation = 0.0f;
    [SerializeField] private Transform gunTarget;
    [SerializeField] private Transform cameraTarget;
    private bool setTarget = false;

    //States 
    public bool isFiring;
    public bool isFiringSingleShot;
    public bool isReloading;
    public bool isAiming;
    public bool canReload;

    [SerializeField] private UpperBodyIK ik;

    //Guns
    [SerializeField] Item[] items;
    int itemIndex;
    int previousItemIndex = -1;

    private void Start()
    {
        animator = GetComponent<Animator>();
        isReloading = false;
        EquipItem(0); 
    }

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
        if (inputManager.Jump && !inputManager.Crouch && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void UpdateWeapon()
    {
        isAiming = inputManager.IsAiming;
        if (!isReloading)
        {
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
            //Switch guns with numbers
            if (inputManager.SwitchWeaponDown)
            {
                if (itemIndex <= 0)
                    EquipItem(items.Length - 1);
                else
                    EquipItem(itemIndex - 1);
            }
        }

        //Fire
        FireWeapon();
        TryToReloadWeapon();
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
            foreach (var item in items[previousItemIndex].itemObject)
            {
                item.SetActive(false);
            } 
        }
        previousItemIndex = itemIndex;

        //Change left arm target
        ik.StartCoroutine(ik.ChangeLeftArmTarget(((GunInfo)items[itemIndex].itemInfo).leftHandTarget));

        //if (pv.IsMine)
        //{
        //    Hashtable hash = new Hashtable();
        //    hash.Add("itemIndex", itemIndex);
        //    PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        //}
    }

    private void FireWeapon()
    {
        if(inputManager.FireSingleShot && (items[itemIndex].TryGetComponent<SingleShotGun>(out var ssg) || items[itemIndex].TryGetComponent<MeleeWeapon>(out var melee)))
        {
            if (items[itemIndex].Use())  
                animator.SetTrigger(animController.FireHash);
        }
        else if (inputManager.AutomaticShot && items[itemIndex].TryGetComponent<AutomaticGun>(out var ag))
        {
            
            if(items[itemIndex].Use())
                animator.SetTrigger(animController.FireHash);
        }
        else
        {
            isFiring = false;
            isFiringSingleShot = false;
        }
    }

    private void TryToReloadWeapon()
    {
        if (!isReloading && inputManager.Reload && items[itemIndex].CanReload())
        {
            isReloading = true;
            animator.SetTrigger(animController.ReloadHash);
        }
    }

    public void ReloadWeapon()
    {
        isReloading = false;
        items[itemIndex].Reload();
    }

    private void UpdatePhysics()
    {  
        if(rb.velocity.y < 0.0f) 
            gravity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime; 
        else 
            gravity = Vector3.zero; 

        //Update velocity
        rb.velocity = (movementDir * (IsRunning ? movementSpeed * speedMultiplier : movementSpeed )) + gravity; 
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

    public void SetGrounded(bool _grnd)
    {
        isGrounded = _grnd;
    } 
}
