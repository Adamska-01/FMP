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
    [Header("Camera Rotation")]
    [SerializeField] private Transform bodyTarget = default;
    [Range(-89, 0)] [SerializeField] private float _maxAngleUp = -50f;
    [Range(0, 89)] [SerializeField] private float _maxAngleDown = 70f;
    [Range(-89f, 89f)] private float bodyOffsetAngle = 45f;
    private float currentBodyAngle;
    [SerializeField] private Transform m_headEffectorNeutral = default;
    [SerializeField] private Transform m_headEffectorUp = default;
    [SerializeField] private Transform m_headEffectorDown = default;
    [SerializeField] private float rotateSpeed = 7.0f;

    [Header("Input")] 
    [SerializeField] private InputManager inputManager;


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //Disable IKs to update them manually 
        headLookAtIK.enabled = false;
        bodyLookAtIK.enabled = false;
        rightArmIK.enabled = false;
        leftArmIK.enabled = false;
        fbbIK.enabled = false;

        currentBodyAngle = bodyOffsetAngle;
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
    {//Override animation
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
        //Camera Rotation
        UpdateLookTargetPos();
        //Character rotation
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.LookRotation(new Vector3(cameraTransf.transform.forward.x, 0f,
            cameraTransf.transform.forward.z)), Time.smoothDeltaTime * rotateSpeed);
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

    //Camera Rotation
    private void UpdateLookTargetPos()
    {
        Vector3 targetForward = Quaternion.LookRotation(new Vector3(cameraTransf.transform.forward.x, 0f, cameraTransf.transform.forward.z)) * Vector3.forward;
        //Get angle between the camera look direction and the forward direction of the charater
        float angle = Vector3.SignedAngle(targetForward, cameraTransf.forward, cameraTransf.right);
        float percent;
        float maxY = 100f;
        float minY = -100f;
        //Clamp angle
        if (angle < 0)
        {
            percent = Mathf.Clamp01(angle / _maxAngleUp);
            if (percent >= 1f) 
                maxY = 0f;

            //Update HeadEffector
            headEffector.position = Vector3.Lerp(m_headEffectorNeutral.position, m_headEffectorUp.position, percent);
        }
        else
        {
            percent = Mathf.Clamp01(angle / _maxAngleDown);
            if (percent >= 1f) 
                minY = 0f;

            //Update HeadEffector
            headEffector.position = Vector3.Lerp(m_headEffectorNeutral.position,m_headEffectorDown.position, percent);
        }

        Vector3 offset = cameraTransf.right * inputManager.XLookAxis + cameraTransf.up * Mathf.Clamp(inputManager.YLookAxis, minY, maxY);
        offset += headTarget.transform.position;
        Vector3 projectedPoint = (offset - cameraTransf.position).normalized * 20f + cameraTransf.position;
        currentBodyAngle = Mathf.Lerp(bodyOffsetAngle, 0, percent);
        //Get Body angle
        headTarget.transform.position = projectedPoint;
        bodyTarget.transform.position = GetPosFromAngle(projectedPoint, currentBodyAngle, transform.right);
    }

    private Vector3 GetPosFromAngle(Vector3 projectedPoint, float angle, Vector3 axis)
    {
        float dist = (projectedPoint - transform.position).magnitude * Mathf.Tan(angle * Mathf.Deg2Rad);
        return projectedPoint + (dist * axis);
    }
}
