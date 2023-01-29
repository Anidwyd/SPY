using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager 
{
    public static int nbLevelsCompleted = 0;

    public static Dictionary<string, string> difficultyMap = new Dictionary<string, string>(){
                    {"x", "easy" },
                    {"y",  "medium"},
                    {"z",  "hard"}
                };

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
