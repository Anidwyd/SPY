using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SlotDescriptor : MonoBehaviour
{
    public DoorPath path;

    public void IncrementOffset()
    {
        path.offset++;
        updateOffset();
    }
    
    public void DecrementOffset()
    {
        if (path.offset <= 0) return;
        path.offset--;
        updateOffset();
    }

    private void updateOffset()
    {
        transform.GetChild(2).GetChild(1).GetComponentInChildren<TMP_Text>().text = path.offset.ToString();
    }
}
