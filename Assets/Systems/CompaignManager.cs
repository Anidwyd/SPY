using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompaignManager : MonoBehaviour
{

    public static int nbLevelsCompleted = 0;
    public static Dictionary<string, Timer> compaignTimers = new Dictionary<string, Timer>();
    public static Dictionary<string, int> totalScores = new Dictionary<string, int>();
    public static Dictionary<string, int> totalLevels = new Dictionary<string, int>();
    public static Dictionary<string, int> totalSurrends = new Dictionary<string, int>();
    public static Dictionary<string, int> totalTrials = new Dictionary<string, int>();
    public static string currentCompaign = "";





    public static double getTotalSurrends(string c)
    {
        if (totalSurrends.ContainsKey(c))
        {
            return totalSurrends[c];
        }
        else
        {
            return 0;
        }
    }
    public static double getCompaignTimers(string c)
    {
        if (compaignTimers.ContainsKey(c))
        {
            return compaignTimers[c].Elapsed.TotalSeconds;
        }
        else
        {
            return 0;
        }
    }

    public static string getCurrentCompaign()
    {
        return currentCompaign;
    }

    public static int getTotalScores(string c)
    {
        if (totalScores.ContainsKey(c))
        {
            return totalScores[c];
        }
        else
        {
            return 0;
        }
    }

    public static int getTotalLevels(string c)
    {
        if (totalLevels.ContainsKey(c))
        {
            return totalLevels[c];
        }
        else
        {
            return 0;
        }
    }

    public static int getTotalTrials(string c)
    {
        if (totalTrials.ContainsKey(c))
        {
            return totalTrials[c];
        }
        else
        {
            return 0;
        }
    }


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
