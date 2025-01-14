﻿using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

/// <summary>
/// Manage highlightable GameObjects (word object as robots, enemies, ground and UI object as current action executed or library items
/// </summary>
public class HighLightSystem : FSystem {
	private Family f_highlightable = FamilyManager.getFamily(new AnyOfComponents(typeof(Highlightable), typeof(LibraryItemRef))); //has to be defined before nonhighlightedGO because initBaseColor must be called before unHighLightItem
	private Family f_highlighted = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable), typeof(PointerOver)), new NoneOfComponents(typeof(LibraryItemRef)));
	private Family f_nonhighlighted = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable)), new NoneOfComponents(typeof(PointerOver), typeof(LibraryItemRef)));
	private Family f_highlightedAction = FamilyManager.getFamily(new AllOfComponents(typeof(LibraryItemRef)), new AnyOfComponents( typeof(CurrentAction), typeof(PointerOver)));
	private Family f_nonCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(LibraryItemRef)), new NoneOfComponents(typeof(CurrentAction), typeof(PointerOver)));

	private GameObject currentConsolePanel;
	
	public GameObject dialogPanel;
	public Button buttonExecute;

	protected override void onStart()
    {
		f_highlightable.addEntryCallback(initBaseColor);
		f_highlighted.addEntryCallback(highLightItem);
		f_nonhighlighted.addEntryCallback(unHighLightItem);
		f_highlightedAction.addEntryCallback(highLightItem);
		f_nonCurrentAction.addEntryCallback(unHighLightItem);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		GameObject highLightedItem = f_highlighted.First();
		if (!highLightedItem || !Input.GetMouseButtonUp(0) || dialogPanel.activeInHierarchy) return;
		
		// If click on highlighted item and item has a script, then show its script in the 2nd script window
		if (highLightedItem.GetComponent<ScriptRef>())
		{
			GameObject go = highLightedItem.GetComponent<ScriptRef>().executablePanel;
			GameObjectManager.setGameObjectState(go, !go.activeInHierarchy);
			MainLoop.instance.GetComponent<AudioSource>().Play();
		}
		
		// If click on highlighted console, then show its infos
		else if (highLightedItem.GetComponent<Actionable>() && buttonExecute.gameObject.activeInHierarchy)
		{
			GameObject newPanel = highLightedItem.GetComponent<Actionable>().panel;
			
			if (currentConsolePanel && currentConsolePanel != newPanel)
				GameObjectManager.setGameObjectState(currentConsolePanel, false);
				
			currentConsolePanel = newPanel;
			
			GameObjectManager.setGameObjectState(currentConsolePanel, !currentConsolePanel.activeInHierarchy);
			currentConsolePanel.AddComponent<NeedRefreshPlayButton>();
			MainLoop.instance.GetComponent<AudioSource>().Play();
		}
	}

	private void initBaseColor(GameObject go)
	{
		// check if it is a script instruction
		if ((go.GetComponent<BaseElement>() || go.GetComponent<BaseCondition>()) && go.GetComponent<Image>())
		{
			go.GetComponent<Highlightable>().baseColor = go.GetComponent<Image>().color;
		}
		// check if it is a word object (robot, ground...)
		if (go.GetComponentInChildren<Renderer>(true))
		{
			go.GetComponent<Highlightable>().baseColor = go.GetComponentInChildren<Renderer>(true).material.color;
			if (go.GetComponent<ScriptRef>())
			{
				Image img = go.GetComponent<ScriptRef>().executablePanel.transform.Find("Scroll View").GetComponent<Image>();
				img.GetComponent<Highlightable>().baseColor = img.color;
			}
		}
	}

	public void highLightItem(GameObject go){
		// first process currentAction in agents panels
		if(go.GetComponent<CurrentAction>())
		{
			go.GetComponent<Image>().color = MainLoop.instance.GetComponent<AgentColor>().currentActionColor;
			Transform parent = go.transform.parent;

			while (parent != null)
            {
				if (parent.GetComponent<ForControl>() || parent.GetComponent<ForeverControl>())
					parent.transform.GetComponentInChildren<Image>().color = MainLoop.instance.GetComponent<AgentColor>().currentActionColor;
				parent = parent.parent;
			}
		}
		
		// second manage sensitive UI inside editable panel
		else if(go.GetComponent<BaseElement>() && go.GetComponent<PointerOver>())
			go.GetComponent<Image>().color = go.GetComponent<BaseElement>().highlightedColor;
		
		// third sensitive UI inside library panel
		else if (go.GetComponent<ElementToDrag>() && go.GetComponent<PointerOver>())
			go.GetComponent<Image>().color = go.GetComponent<Highlightable>().highlightedColor;
		
		// then process world GameObjects (Walls, enemies, robots...)
		else if (go.GetComponentInChildren<Renderer>(true))
		{
			// go.GetComponentInChildren<Renderer>(true).material.color = go.GetComponent<Highlightable>().highlightedColor;
			Color highlightedColor = go.GetComponent<Highlightable>().highlightedColor;
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
				r.material.color = highlightedColor;

			if(go.GetComponent<ScriptRef>()){
				Image img = go.GetComponent<ScriptRef>().executablePanel.transform.Find("Scroll View").GetComponent<Image>();
				img.color = img.GetComponent<Highlightable>().highlightedColor;
			}
		}
	}

	public void unHighLightItem(GameObject go){
		// manage the case of script execution
        if (go.GetComponent<BaseElement>()) { 
			go.GetComponent<Image>().color = go.GetComponent<BaseElement>().baseColor;
			Transform parent = go.transform.parent;
			while (parent != null)
			{
				if (parent.GetComponent<ForControl>() || parent.GetComponent<ForeverControl>())
					parent.transform.GetChild(0).GetComponent<Image>().color = MainLoop.instance.GetComponent<AgentColor>().forBaseColor;
				parent = parent.parent;
			}
		}
        
		// the case of item inside library panel
		else if (go.GetComponent<ElementToDrag>())
			go.GetComponent<Image>().color = go.GetComponent<Highlightable>().baseColor;
        
		// the case of world GameObjects (robot, ground...)
		else if (go.GetComponentInChildren<Renderer>(true)){
			// go.GetComponentInChildren<Renderer>(true).material.color = go.GetComponent<Highlightable>().baseColor;
	        Color baseColor = go.GetComponent<Highlightable>().baseColor;
	        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
				r.material.color = baseColor;

			if(go.GetComponent<ScriptRef>()){
				Image img = go.GetComponent<ScriptRef>().executablePanel.transform.Find("Scroll View").GetComponent<Image>();
				img.color = img.GetComponent<Highlightable>().baseColor;
			}
		}
	}
}