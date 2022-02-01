using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class UpperBodyIK : MonoBehaviour
{ 
    [Header("Final IK Modules")]
    [SerializeField] private LookAtIK headLookAtIK = default;
    [SerializeField] private LookAtIK bodyLookAtIK = default;
    [SerializeField] private ArmIK leftArmIK = default;
    [SerializeField] private ArmIK rightArmIK = default;
    [SerializeField] private FullBodyBipedIK fbbIK = default;
    [Header("LookAt Settings")]
    [SerializeField] private Transform cameraTransf = default;
    [SerializeField] private Transform headTarget = default;
    [Header("Head Effector Settings")]
    [SerializeField] private Transform headEffector = default;
    [Header("Arms Settings")]
    [SerializeField] private Transform rightHandTarget = default;
    [SerializeField] private float rightHandPosSpeed = 1.0f;
    [SerializeField] private float rightHandRotSpeed = 1.0f;
    [Header("ADS Settings")]
    [SerializeField] private Transform rightHandADSOff = default;
    [SerializeField] private Transform rightHandADSOn = default;
    [SerializeField] private float adsTransitionTime = 1.0f;
    private float transitionADS;
    private Vector3 rightHandFollow;
    private Quaternion rightHandFollowRot;
    private Vector3 refRightHandFollow;
    [Header("FOV")] 
    [SerializeField] private Camera mainCamera = default;
    [SerializeField] private float adsOffFov = 60.0f;
    [SerializeField] private float adsOnFov = 40.0f;

    [Header("Input")] 
    [SerializeField] private InputManager inputManager;


    void Start()
    {
        //Disable IKs to update them manually 
        headLookAtIK.enabled = false;
        bodyLookAtIK.enabled = false;
        rightArmIK.enabled = false;
        leftArmIK.enabled = false;
        fbbIK.enabled = false; 
    }

    void Update()
    {
        bodyLookAtIK.solver.FixTransforms();
        headLookAtIK.solver.FixTransforms();
        fbbIK.solver.FixTransforms();
        rightArmIK.solver.FixTransforms();
        leftArmIK.solver.FixTransforms();
    }

    void LateUpdate()
    {//Override
        LookAtIKUpdate();
        FBBIKUpdate();
        ArmsIKUpdate();
    }


    private void LookAtIKUpdate()
    { 
        //Right order
        bodyLookAtIK.solver.Update();
        headLookAtIK.solver.Update();
    }
    private void ArmsIKUpdate()
    {
        AimDownSightUpdate();
        rightArmIK.solver.Update();
        leftArmIK.solver.Update();
    }
    private void FBBIKUpdate()
    {
        fbbIK.solver.Update();
        //Set targets
        cameraTransf.LookAt(headTarget);
        headEffector.LookAt(headTarget);
    }
    private void AimDownSightUpdate()
    {
        //Check ADS state
        if (!inputManager.IsAiming)
        {
            transitionADS = Mathf.Lerp(transitionADS, 0, Time.smoothDeltaTime * adsTransitionTime);
            rightHandTarget.rotation = rightHandADSOff.rotation;
        }
        else
        {
            transitionADS = Mathf.Lerp(transitionADS, 1, Time.smoothDeltaTime * adsTransitionTime);
            rightHandTarget.rotation = rightHandADSOn.rotation;
        }

        //ADS on/off Update
        rightHandFollow = Vector3.Lerp(rightHandADSOff.position, rightHandADSOn.position, transitionADS);
        rightHandFollowRot = Quaternion.Lerp(rightHandADSOff.rotation, rightHandADSOn.rotation, transitionADS);
        //FOV
        mainCamera.fieldOfView = Mathf.Lerp(adsOffFov, adsOnFov, transitionADS);
        //Lerp
        rightHandTarget.position = Vector3.SmoothDamp(rightHandTarget.position, rightHandFollow, ref refRightHandFollow, rightHandPosSpeed * Time.smoothDeltaTime);
        rightHandTarget.rotation = Quaternion.Lerp(rightHandTarget.rotation, rightHandFollowRot, Time.smoothDeltaTime * rightHandRotSpeed);
    }
}
