using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPath : MonoBehaviour
{
    public int slotID;
    public int step;
    public int offset;
    public int length;

    public GameObject door;

    public List<PathUnit> units;
}
