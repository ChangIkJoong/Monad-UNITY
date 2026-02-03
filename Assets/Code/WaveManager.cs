using UnityEngine;

public class WaveManager : MonoBehaviour
{
    private int level;
    [SerializeField] private float timer;
    // public SpawnManager spawnManager;
    // List<Enemies> list; ??? maybe not necessary



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Test
    }

    // Update is called once per frame
    void Update()
    {
        checkWave(); 
    }
    
    private void checkWave()
    {
/*         if ( enemies dead)
        {
            start timer
            if timer is over then call nextWave();
        } */
    }
    public void increaseLevel()
    {
        level++;
    }

    public void nextWave()
    {
        debug.log("Next Wave beginns! Muhahaha");
    }
    // core-routine (youtube)
}
