using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoorPath : MonoBehaviour
{
    public List<PathUnit> units;
    public GameObject door;
    public float[] pointers;
    
    public int duration;
    public int length;
    public int delay;
    public float step;

    public GameObject descriptor;
}
