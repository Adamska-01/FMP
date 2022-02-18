using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    private IEnumerator co;
    public static HUDController instance;
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

    [System.Serializable] public class WeaponIcons
    {
        public Image weaponSelected;
        public TMP_Text keyText;
    }

    public TMP_Text ammunitionText;
    public TMP_Text healthText;
    public TMP_Text armourText; 
    public WeaponIcons[] weapons; 
     

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
        for (float alpha = 1.0f; alpha >= 0; alpha -= 0.01f)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                float a = weapons[i].weaponSelected.color.a * alpha;
                weapons[i].weaponSelected.color = new Color(1.0f, 1.0f, 1.0f, a);
                weapons[i].keyText.color = new Color(1.0f, 1.0f, 1.0f, a);
            }
            yield return null;
        }
    }
}
