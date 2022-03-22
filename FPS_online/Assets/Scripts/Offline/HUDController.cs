using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    private IEnumerator co;
    public static HUDController instance;
    private InputManager inputManager;
    private void Awake()
    {
        instance = this;
        inputManager = FindObjectOfType<InputManager>();
    }

    public enum WeaponSelected
    {
        PRIMARY, 
        SECONDARY,
        KNIFE
    }

    [System.Serializable] public class WeaponIcons
    {
        public Image weaponSelected;
        public TMP_Text keyText;
    }

    public TMP_Text ammunitionText;
    public TMP_Text healthText;
    public TMP_Text armourText; 
    public WeaponIcons[] weapons;

    [SerializeField] private Panel[] panels;

    [HideInInspector] public bool isPaused;

    void Start()
    {
        isPaused = false;
        OpenPanel(PanelType.HUD);
    }

    void Update()
    {
        if (inputManager.Pause)
        {
            OpenClosePause();
        }
        //Debug.Log(inputManager.VerticalUI);
        //if ((inputManager.VerticalUI != 0.0f || inputManager.HorizontalUI != 0.0f) && (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.activeInHierarchy))
        //{
        //    EventSystem.current.SetSelectedGameObject(null);
        //    EventSystem.current.SetSelectedGameObject(GetActivePannel().firstselected);
            
        //}
    }


    public void SelectWeapon(int _index)
    {
        //Stop previous corutine first
        if(co != null) StopCoroutine(co);

        //Start corutine
        co = FadeInAndOutUI(_index);
        StartCoroutine(co); 
    }

    IEnumerator FadeInAndOutUI(int _index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            float a = i == _index ? 1.0f : 0.4f;
            weapons[i].weaponSelected.color = new Color(1.0f, 1.0f, 1.0f, a);
            weapons[i].keyText.color = new Color(1.0f, 1.0f, 1.0f, a);
        }

        yield return new WaitForSeconds(2.0f);

        //Fade
        float alpha = 1.0f;
        while(alpha >= 0)
        {
            alpha -= Time.deltaTime * 0.3f;
            for (int i = 0; i < weapons.Length; i++)
            {
                float a = weapons[i].weaponSelected.color.a * alpha;
                weapons[i].weaponSelected.color = new Color(1.0f, 1.0f, 1.0f, a);
                weapons[i].keyText.color = new Color(1.0f, 1.0f, 1.0f, a);
            }
            yield return null;
        } 
    }

    public void OpenClosePause()
    {
        if (!isPaused)
        {
            OpenPanel(PanelType.PAUSE);
             
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            

            Time.timeScale = 0;

            isPaused = true;
        }
        else
        {
            OpenPanel(PanelType.HUD);

            Time.timeScale = 1;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            isPaused = false;
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1;
        var roomMngr = FindObjectOfType<RoomManager>().gameObject;
        if(roomMngr != null)
            Destroy(FindObjectOfType<RoomManager>().gameObject);
        
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene(0);
        } 
        else 
            SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenPanel(PanelType _panelName)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                if (panels[i].type == _panelName)
                    panels[i].Open();
                else if (panels[i].isOpen)
                    ClosePanel(panels[i]);
            }
        }
    }

    //Used by buttons
    public void OpenPanel(Panel _panel)
    {
        //Close the menus we currently have open first 
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                if (panels[i].isOpen)
                {
                    ClosePanel(panels[i]);
                }
            }
        }

        //Open current menu
        _panel?.Open();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_panel.firstselected);
    }

    public void OpenPanelWithoutClosing(PanelType _panelName)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                if (panels[i].type == _panelName)
                    panels[i].Open();
            }
        }
    }

    public void ClosePanel(Panel _panel)
    {
        _panel.Close();
    }

    public void CloseAllPanels()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            ClosePanel(panels[i]);
        }
    }

    public Panel GetPannel(PanelType _type)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].type == _type)
            {
                return panels[i];
            }
        }
        return null;
    }

    public Panel GetActivePannel()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].gameObject.activeInHierarchy)
            {
                return panels[i];
            }
        }
        return null;
    }

    IEnumerator SetSelected(GameObject _selected)
    {
        EventSystem.current.SetSelectedGameObject(null);

        yield return new WaitForSeconds(Time.deltaTime);
         
        EventSystem.current.SetSelectedGameObject(_selected);
    }
}
