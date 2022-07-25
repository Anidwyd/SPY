using UnityEngine;
using FYFY;
using TMPro;

/// <summary>
/// This system manages blocs limitation in inventory
/// </summary>
public class BlocLimitationManager : FSystem {
	private Family droppedActions = FamilyManager.getFamily(new AllOfComponents(typeof(Dropped), typeof(LibraryItemRef)));
	private Family deletedActions = FamilyManager.getFamily(new AllOfComponents(typeof(AddOne)));
	private Family draggableElement = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)));
	private GameData gameData;

	protected override void onStart()
	{
		GameObject gd = GameObject.Find("GameData");
		if (gd != null)
		{
			gameData = gd.GetComponent<GameData>();
			// init limitation counters for each draggable elements
			// Initialisation des block � afficher dans l'inventaire
			foreach (GameObject go in draggableElement)
			{
				Debug.Log("Draggeble Element : " + go.name);
				// get prefab associated to this draggable element
				// On r�cup�re le pr�fab de l'�l�ment
				GameObject prefab = go.GetComponent<ElementToDrag>().actionPrefab;
                // get action key depending on prefab type
				string key = getActionKey(prefab.GetComponent<Highlightable>());
				// default => hide go
				GameObjectManager.setGameObjectState(go, false);
				Debug.Log("Draggeble Element afer change state: " + go.name);
				// update counter et active les block necessaire
				updateBlocLimit(key, go);
			}
		}
		droppedActions.addEntryCallback(useAction);
		deletedActions.addEntryCallback(unuseAction);
	}

	// Retourne l'action key du bloc
	private string getActionKey(Highlightable action){
		if (action is BasicAction)
			return ((BasicAction)action).actionType.ToString();
		else if (action is IfControl)
			if(action is IfElseControl)
				return "Else";
			else
				return "If";
		else if (action is ForControl)
			if (action is WhileControl)
				return "While";
			else
				return "For";
		else if (action is ForeverControl)
			return "Forever";
		else if (action is BaseOperator)
			return ((BaseOperator)action).operatorType.ToString();
		else if (action is BaseCaptor)
			return ((BaseCaptor)action).captorType.ToString();
		else
			return null;
	}

	private GameObject getDraggableElement (string name){
		Debug.Log("Get draggable Element : " + name);
		foreach(GameObject go in draggableElement){
			Debug.Log("Go : " + go.name);
			if (go.name.Equals(name)){
				return go;
			}
		}
		return null;
	}

	// Met � jour la limite du nombre de fois ou l'on peux utiliser un bloc (si il y a une limite)
	// Le d�sactive si la limite est atteinte
	// Met � jour le compteur
	private void updateBlocLimit(string keyName, GameObject draggableGO){
		Debug.Log("Update limite bloc : " + keyName);
		Debug.Log("Draggable GO name : " + draggableGO.name);
		if (gameData.actionBlocLimit.ContainsKey(keyName))
		{
			bool isActive = gameData.actionBlocLimit[keyName] != 0; // negative means no limit
			if(draggableGO != null)
            {
				GameObjectManager.setGameObjectState(draggableGO, isActive);
			}
			if (isActive)
			{
				if (gameData.actionBlocLimit[keyName] < 0)
					// unlimited action => hide counter
					GameObjectManager.setGameObjectState(draggableGO.transform.GetChild(1).gameObject, false);
				else
				{
					// limited action => init and show counter
					GameObject counterText = draggableGO.transform.GetChild(1).gameObject;
					counterText.GetComponent<TextMeshProUGUI>().text = "Reste " + gameData.actionBlocLimit[keyName].ToString();
					GameObjectManager.setGameObjectState(counterText, true);
				}
			}
		}
	}

	private void useAction(GameObject go){
		string actionKey = null;
		// Base element, base condition
		if (go.GetComponent<BaseElement>())
        {
			actionKey = getActionKey(go.GetComponent<BaseElement>());
		}
		else if (go.GetComponent<BaseCondition>())
		{
			actionKey = getActionKey(go.GetComponent<BaseCondition>());
		}
		Debug.Log("useAction activate action key: " + actionKey);
		if (actionKey != null){
			gameData.actionBlocLimit[actionKey] -= 1;
			GameObject draggableModel = getDraggableElement(actionKey);
			updateBlocLimit(actionKey, draggableModel);		
		}
		GameObjectManager.removeComponent<Dropped>(go);
	}
	
	private void unuseAction(GameObject go){
		BaseElement action;
		if(go.GetComponent<ElementToDrag>())
			action = go.GetComponent<ElementToDrag>().actionPrefab.GetComponent<BaseElement>();
		else
			action = go.GetComponent<BaseElement>();

		string actionKey = getActionKey(action);

		AddOne[] addOnes =  go.GetComponents<AddOne>();
		if(actionKey != null){
			gameData.actionBlocLimit[actionKey] += addOnes.Length;
			GameObject draggableModel = getDraggableElement(actionKey);
			updateBlocLimit(actionKey, draggableModel);
		}
	}
}