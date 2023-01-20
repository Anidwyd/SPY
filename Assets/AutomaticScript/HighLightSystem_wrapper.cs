using UnityEngine;
using FYFY;

public class HighLightSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject dialogPanel;
	public UnityEngine.UI.Button buttonExecute;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
	}

	public void highLightItem(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "highLightItem", go);
	}

	public void unHighLightItem(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "unHighLightItem", go);
	}

}
