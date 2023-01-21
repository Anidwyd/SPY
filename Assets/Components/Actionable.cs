using UnityEngine;
using System.Collections.Generic;

public class Actionable : MonoBehaviour {
	public List<int> slotsID; // target slot this component control
	public Dictionary<int, DoorPath> paths = new Dictionary<int, DoorPath>();
	
	public int[] sinceActivation;
	public bool[] isConnected;
	public int keepSignal;

	public GameObject panel;
}