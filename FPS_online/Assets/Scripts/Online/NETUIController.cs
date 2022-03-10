using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NETUIController : MonoBehaviour
{
    private IEnumerator co;
    public static NETUIController instance;
    private void Awake()
    {
        instance = this;
    }

    public enum WeaponSelected
    {
        PRIMARY,
        SECONDARY,
        KNIFE
    }

    public enum PanelType
    {
        HUD,
        DEATH,
        LEADERBOARD,
        END
    }

    [System.Serializable]
    public class WeaponIcons
    {
        public Image weaponSelected;
        public TMP_Text keyText;
    }

    public TMP_Text ammunitionText;
    public TMP_Text healthText;
    public TMP_Text armourText;
    public TMP_Text deathText;
    public WeaponIcons[] weapons;
    public GameObject leaderboard;
    public Leaderboard leaderboardPlayerDisplay;
    public GameObject endScreen;
    public TMP_Text timerText;

    [SerializeField] private Panel[] panels;

    private void Start()
    {
        OpenPanel(PanelType.HUD);
    }


    public void SelectWeapon(int _index)
    {
        //Stop previous corutine first
        if (co != null) StopCoroutine(co);

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
        while (alpha >= 0)
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
}
