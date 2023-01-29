using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStatsManager
{

    public static Dictionary<string, int> levelTrials = new Dictionary<string, int>();
    public static Dictionary<string, int> levelScores = new Dictionary<string, int>();
    public static Dictionary<string, double> levelCompletionTimes = new Dictionary<string, double>();
    public static string currentLevel = "";
    public static double currentLevelStartTime = 0;
    public static double latestFinishedDate = 0;
    public static Dictionary<string, Timer> levelTimers = new Dictionary<string, Timer>();



    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static string getCurrentLevelTime()
    {
        return levelTimers[currentLevel].Elapsed.TotalSeconds.ToString();
    }

    public static void incrementTrials()
    {
        levelTrials[currentLevel]++;
    }
}
