using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    public PanelType type;
    public GameObject firstselected;
    [HideInInspector] public bool isOpen;

    void Awake()
    {
        //Set isOpen info at the start
        isOpen = gameObject.activeSelf;
    }

    public void Open()
    {
        isOpen = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        isOpen = false;
        gameObject?.SetActive(false);
    }
}
