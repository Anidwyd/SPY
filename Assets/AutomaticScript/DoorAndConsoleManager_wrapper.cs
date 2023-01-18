using UnityEngine;
using FYFY;

public class DoorAndConsoleManager_wrapper : BaseWrapper
{
	public GameObject pathUnitPrefab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "pathUnitPrefab", pathUnitPrefab);
	}

}
