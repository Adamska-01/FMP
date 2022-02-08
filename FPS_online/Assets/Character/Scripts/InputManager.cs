using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{ 
    [Header("Movement Axis")]
    [SerializeField] private string m_forwardAxis = "Vertical";
    [SerializeField] private string m_sidewayAxis = "Horizontal"; 
    protected float forward;
    public float Forward { get { return forward; } }
    protected float sideway; 
    public float Sideway { get { return sideway; } }

    [Header("Weapon Keys")]
    private KeyCode aimKey = KeyCode.Mouse1;
    protected bool isAiming;
    public bool IsAiming { get { return isAiming; } }

    [Header("Camera Axis")]
    private string verticalLookAxis = "Mouse Y";
    private string horizontalLookAxis = "Mouse X";
    private float xAxisSensitivity = 0.2f;
    private float yAxisSensitivity = 0.2f;
    protected float xAxis;
    public float XLookAxis { get { return xAxis; } }
    protected float yAxis;
    public float YLookAxis { get { return yAxis; } }


    private void Update()
    {
        HandleInput();
    }
    

    protected void HandleInput()
    {
        //Movement
        forward = Input.GetAxis(m_forwardAxis);
        sideway = Input.GetAxis(m_sidewayAxis);
        //ADS
        isAiming = Input.GetKey(aimKey);
        //Camera rotate
        xAxis = Input.GetAxis(horizontalLookAxis) * xAxisSensitivity;
        yAxis = Input.GetAxis(verticalLookAxis) * yAxisSensitivity;
    }
}
