using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartAndStopTest : MonoBehaviour
{
    public bool isStart;
    public GameObject otherButton;
    public Image myButton;
    public ShootingRange shootingRange;
    public GameObject root;

    public void StartOrStopTest()
    {
        StartCoroutine(StartOrStop());
    }

    IEnumerator StartOrStop()
    {
        myButton.color = isStart ? Color.green : Color.red;
       
        yield return new WaitForSeconds(2.0f);

        //Start test
        shootingRange.StartCoroutine(shootingRange.RunTest());

        //Set deafult color
        myButton.color = Color.white;
        
        //Set buttons
        otherButton.SetActive(true);
        root.SetActive(false);
    }
}
