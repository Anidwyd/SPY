using FYFY;
using UnityEngine;

public class ConsolePanel : MonoBehaviour
{
    public void closePanel()
    {
        GameObject go = gameObject;
        
        go.AddComponent<NeedRefreshPlayButton>();
        GameObjectManager.setGameObjectState(go, false);
        MainLoop.instance.GetComponent<AudioSource>().Play();
    }
}
