using UnityEngine;
using System.Collections.Generic;

public class Activable : MonoBehaviour {
	public List<int> slotID; // target slot this component control
	public int currDoorPath;

	public Dictionary<int, int> lengths = new Dictionary<int, int>();
}