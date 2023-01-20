using FYFY;
using TMPro;
using UnityEngine;

public class SlotDescriptor : MonoBehaviour
{
    public DoorPath path;

    public void IncrementOffset()
    {
        updateOffset(1);
    }
    
    public void DecrementOffset()
    {
        if (path.offset > 0)
            updateOffset(-1);
    }

    private void updateOffset(int v)
    {
        path.offset += v;
        transform.GetChild(2).GetChild(1).GetComponentInChildren<TMP_Text>().text = path.offset.ToString();
        MainLoop.instance.GetComponent<AudioSource>().Play();
    }
}
