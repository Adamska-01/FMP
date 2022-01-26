using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] private Menu[] menus;

    void Awake()
    {
        Instance = this;    
    }

    public void OpenMenu(string _menuName)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == _menuName)
                menus[i].Open();
            else if (menus[i].isOpen)
                CloseMenu(menus[i]);
        }
    }

    //Used by buttons
    public void OpenMenu(Menu _menu)
    {
        //Close the menus we currently have open first 
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].isOpen)
                CloseMenu(menus[i]);
        }

        //Open current menu
        _menu.Open();
    } 

    public void CloseMenu(Menu _menu)
    {
        _menu.Close(); 
    }
}
