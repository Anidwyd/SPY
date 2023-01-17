using System.Linq;
using UnityEngine;
using FYFY;

/// <summary>
/// Manage Doors and Consoles => open/close doors depending on consoles state
/// </summary>
public class DoorAndConsoleManager : FSystem {

	private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family f_console = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position)));
	private Family f_consoleOn = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource), typeof(TurnedOn)));
	private Family f_consoleOff = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)), new NoneOfComponents(typeof(TurnedOn)));
	private Family f_doorPath = FamilyManager.getFamily(new AllOfComponents(typeof(DoorPath)));

	private Family f_newStep = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
	private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));

	private GameData gameData;

	public GameObject doorPathPrefab;
	public Color pathOff;

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		f_gameLoaded.addEntryCallback(connectDoorsAndConsoles);
		f_consoleOn.addEntryCallback(onNewConsoleTurnedOn); // Console will enter in this family when TurnedOn component will be added to console (see CurrentActionExecutor)
		f_consoleOff.addEntryCallback(onNewConsoleTurnedOff); // Console will enter in this family when TurnedOn component will be removed from console (see CurrentActionExecutor)
		
		f_newStep.addEntryCallback(delegate { onNewStep(); });
	}

	private void onNewConsoleTurnedOn(GameObject consoleGO)
    {
	    consoleGO.GetComponent<Activable>().currDoorPath = 0;
		updatePath(consoleGO, true);
	}

	private void onNewConsoleTurnedOff(GameObject consoleGO)
	{
		consoleGO.GetComponent<Activable>().currDoorPath = 0;
		updatePath(consoleGO, false);
	}

	private void updatePath(GameObject consoleGO, bool state)
	{
		Activable activable = consoleGO.GetComponent<Activable>();
		
		// parse all slots controled by this console
		foreach (int slotId in activable.slotID)
		{
			// parse all doors
			foreach (GameObject door in f_door)
			{
				int currDoorPath = activable.currDoorPath;
				updatePathColor(slotId, currDoorPath, state);
				
				if (!activable.lengths.TryGetValue(slotId, out var length))
					continue;

				// if slots are equals => activate / disable door
				if (door.GetComponent<ActivationSlot>().slotID != slotId || currDoorPath != length - 1)
					continue;
				
				// show / hide door
				Transform parent = door.transform.parent;
				parent.GetComponent<AudioSource>().Play();
				parent.GetComponent<Animator>().SetTrigger(state ? "Close" : "Open");
				parent.GetComponent<Animator>().speed = gameData.gameSpeed_current;
					
				// GameObjectManager.unbind(consoleGO);
			}
		}
	}

	private void updatePathColor(int slotId, int pathId, bool state)
    {
	    foreach (GameObject path in f_doorPath)
	    {
		    DoorPath doorPath = path.GetComponent<DoorPath>();
			if (doorPath.slotId == slotId && doorPath.pathId == pathId)
				foreach (SpriteRenderer sr in path.GetComponentsInChildren<SpriteRenderer>())
					sr.color = state ? doorPath.color : pathOff;
	    }
	  // DoorPath path = activable.paths[slotId][pathId];
	  // foreach (SpriteRenderer sr in path.GetComponentsInChildren<SpriteRenderer>())
		 //  sr.color = state ? path.color : pathOff;
    }

	private void connectDoorsAndConsoles(GameObject unused)
    {
		// Parse all doors
		foreach (GameObject door in f_door)
        {
			Position doorPos = door.GetComponent<Position>();
			ActivationSlot doorSlot = door.GetComponent<ActivationSlot>();
			
			// Parse all consoles
			foreach(GameObject console in f_console)
            {
				// Check if door is controlled by this console
				Activable consoleSlots = console.GetComponent<Activable>();
				bool isOn = console.GetComponent<TurnedOn>() != null;
				if (consoleSlots.slotID.Contains(doorSlot.slotID))
				{
					// Connect this console with this door
					int length = 0;
					Position consolePos = console.GetComponent<Position>();
					int xStep = consolePos.x < doorPos.x ? 1 : (consolePos.x == doorPos.x ? 0 : -1);
					int yStep = consolePos.y < doorPos.y ? 1 : (consolePos.y == doorPos.y ? 0 : -1);
					
					int x = 0;
					while (consolePos.x + x != doorPos.x)
					{
						GameObject path = Object.Instantiate<GameObject>(doorPathPrefab, gameData.LevelGO.transform.position + new Vector3(consolePos.y * 3, 3, (consolePos.x + x + xStep / 2f) * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);
						path.transform.Find("West").gameObject.SetActive(true);
						path.transform.Find("East").gameObject.SetActive(true);
						
						DoorPath doorPath = path.GetComponent<DoorPath>();
						doorPath.slotId = doorSlot.slotID;
						doorPath.pathId = length++;
						doorPath.color = doorSlot.color;
						
						foreach (SpriteRenderer sr in path.GetComponentsInChildren<SpriteRenderer>())
							sr.color = isOn ? doorPath.color : pathOff;
						
						GameObjectManager.bind(path);
						x += xStep;
					}
					
					int y = 0;
					while (consolePos.y + y != doorPos.y)
					{
						GameObject path = Object.Instantiate<GameObject>(doorPathPrefab, gameData.LevelGO.transform.position + new Vector3((consolePos.y + y + yStep / 2f) * 3, 3, (consolePos.x + x) * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);
						path.transform.Find("South").gameObject.SetActive(true);
						path.transform.Find("North").gameObject.SetActive(true);
						
						DoorPath doorPath = path.GetComponent<DoorPath>();
						doorPath.slotId = doorSlot.slotID;
						doorPath.pathId = length++;
						doorPath.color = doorSlot.color;

						foreach (SpriteRenderer sr in path.GetComponentsInChildren<SpriteRenderer>())
							sr.color = isOn ? doorPath.color : pathOff;
						
						GameObjectManager.bind(path);
						y += yStep;
					}

					consoleSlots.lengths.Add(doorSlot.slotID, length);
					Debug.Log("oui");
				}

            }
        }
    }
	
	private void onNewStep(){
		foreach (GameObject console in f_consoleOn)
		{
			console.GetComponent<Activable>().currDoorPath++;
			updatePath(console, true);
		}
		
		foreach (GameObject console in f_consoleOff)
		{
			console.GetComponent<Activable>().currDoorPath++;
			updatePath(console, false);
		}
	}
}