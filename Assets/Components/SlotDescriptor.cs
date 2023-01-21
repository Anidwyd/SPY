using FYFY;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotDescriptor : MonoBehaviour
{
    public DoorPath path;

    public void IncrementDelay()
    {
        updateDelay(1);
    }
    
    public void DecrementDelay()
    {
        if (path.delay > 0) updateDelay(-1);
    }

    private void updateDelay(int v)
    {
        path.delay += v;
        transform.GetChild(2).GetChild(1).GetComponentInChildren<TMP_Text>().text = path.delay.ToString();
        MainLoop.instance.GetComponent<AudioSource>().Play();
    }
    
    public void updateDelayButtons(bool state)
    {
        Transform delayContainer = gameObject.transform.Find("Delay");
        delayContainer.GetChild(0).GetComponent<Button>().interactable = state;
        delayContainer.GetChild(2).GetComponent<Button>().interactable = state;
    }
}
