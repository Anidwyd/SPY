using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoorPath : MonoBehaviour
{
    public List<PathUnit> units;
    public GameObject door;
    public int duration;
    public int length;
    public int offset;
    public float step;
    public float pointer;

    public GameObject descriptor;

    public void incrementOffset()
    {
        if (!descriptor) return;
        offset++;
    }
    
    public void decrementOffset()
    {
        if (!descriptor) return;
        offset--;
    }
}
