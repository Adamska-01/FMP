using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public enum MenuType
    {
        LOADING,
        TITLE,
        MULTIPLAYER,
        NICKNAME,
        CREATE_ROOM,
        FIND_ROOM,
        ROOM_LIST,
        ROOM,
        ERROR
    }

    public static MenuManager Instance;
    [SerializeField] private Menu[] menus;

    void Awake()
    {
        Instance = this;    
    }

    public void OpenMenu(MenuType _menuName)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if(menus[i] != null)
            {
                if (menus[i].type == _menuName)
                    menus[i].Open();
                else if (menus[i].isOpen)
                    CloseMenu(menus[i]);
            }
        }
    }

    //Used by buttons
    public void OpenMenu(Menu _menu)
    {
        //Close the menus we currently have open first 
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i] != null)
            {
                if (menus[i].isOpen)
                    CloseMenu(menus[i]);
            }
        }

        //Open current menu
        _menu?.Open();
    } 

    public void CloseMenu(Menu _menu)
    {
        _menu.Close(); 
    }
}
