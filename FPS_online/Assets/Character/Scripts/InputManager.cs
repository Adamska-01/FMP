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
    

    private void Update()
    {
        HandleInput();
    }
    
    protected void HandleInput()
    {
        forward = Input.GetAxis(m_forwardAxis);
        sideway = Input.GetAxis(m_sidewayAxis);
    } 
}
