using UnityEngine;
using System.Collections.Generic;

public class Actionable : MonoBehaviour {
	// public List<int> slotsID; // target slot this component control
	public Dictionary<int, DoorPath> paths = new Dictionary<int, DoorPath>();
	
	public int currStep;
	
	public int[] sinceStateActivated;
	public bool[] isStateActive;
	public int keepActive;

	public GameObject panel;
	
	public bool keepLimitReached(int offset=0)
	{
		return keepActive > 0 && gameObject.GetComponent<TurnedOn>() &&
		       sinceStateActivated[1] >= keepActive - offset && !isStateActive[0];
	}
}