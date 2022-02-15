using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty
{
    EASY,
    MEDIUM,
    HARD
}

public class ShootingRange : MonoBehaviour
{ 
    private Difficulty difficulty; 

    public List<Difficulty> difficulties = new List<Difficulty>();
    public List<GameObject> selections = new List<GameObject>();
    public Dictionary<Difficulty, GameObject> diffSelect = new Dictionary<Difficulty, GameObject>();

    public GameObject startButton;
    public GameObject stopButton;

    public SpawnObject spawner;
    private bool isSpawning;
    private float spawnTime;
    public float easySpawnTime;
    public float mediumSpawnTime;
    public float hardSpawnTime;

    //Timer
    private const float TEST_TIME = 30.0f;
    private float timeRemaining;
    public bool timerIsRunning; 



    void Start()
    {
        //Populate the dictionary
        for (int i = 0; i < difficulties.Count; i++)
        {
            diffSelect.Add(difficulties[i], selections[i]);
        }

        //Select difficulty
        difficulty = Difficulty.EASY;
        DifficultySelect(difficulty);

        //Set only start button to true 
        startButton.SetActive(true);
        stopButton.SetActive(false);

        //Reset timer
        timerIsRunning = false; //Do not start automatically 
        timeRemaining = TEST_TIME;
    }

    void Update()
    {
        
    }


    public void DifficultySelect(Difficulty _diff)
    {
        //Set all to false first
        foreach (var item in diffSelect)
        {
            item.Value.SetActive(false);
        }

        //Set to true the selected one
        diffSelect[_diff].SetActive(true);

        //Assign current difficulty
        difficulty = _diff;
    } 

    public IEnumerator RunTest()
    { 
        //Select difficulty (spawn time)
        switch(difficulty)
        {
            case Difficulty.EASY:
                spawnTime = easySpawnTime;
                break;
            case Difficulty.MEDIUM:
                spawnTime = mediumSpawnTime;
                break;
            case Difficulty.HARD:
                spawnTime = hardSpawnTime;
                break;
        }

        //Start Timer
        timerIsRunning = true;

        //Start test
        while (timeRemaining > 0.0f && !startButton.activeSelf)
        {
            if (timerIsRunning)
            {
                if (timeRemaining > 0.0f)
                {
                    //RUN test
                    if (!isSpawning)
                        StartCoroutine(SpawnDummy()); 

                    timeRemaining -= Time.deltaTime;
                    DisplayTime(timeRemaining);
                    yield return null; //Test is over
                }
                else
                {
                    Debug.Log("Time has run out!"); 
                     
                    break;
                }
            } 
        }

        //Reset timer
        timerIsRunning = false;
        timeRemaining = TEST_TIME;

        //Set the start button again
        startButton.SetActive(true);
        stopButton.SetActive(false);
    }

    private IEnumerator SpawnDummy()
    {
        //Start Test
        isSpawning = true;
        GameObject dummy = spawner.SpawnObjectFacingPlayer();

        yield return new WaitForSeconds(spawnTime); //Test is over

        //TODO: add points

        Destroy(dummy);

        isSpawning = false;
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        Debug.Log(string.Format("{0:00}:{1:00}", minutes, seconds));
    }
}
