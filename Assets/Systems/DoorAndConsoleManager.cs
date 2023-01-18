using UnityEngine;
using System.Linq;
using FYFY;

/// <summary>
/// Manage Doors and Consoles => open/close doors depending on consoles state
/// </summary>
public class DoorAndConsoleManager : FSystem {

	private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family f_console = FamilyManager.getFamily(new AllOfComponents(typeof(Actionable), typeof(Position)));
	private Family f_consoleOn = FamilyManager.getFamily(new AllOfComponents(typeof(Actionable), typeof(Position), typeof(AudioSource), typeof(TurnedOn)));
	private Family f_consoleOff = FamilyManager.getFamily(new AllOfComponents(typeof(Actionable), typeof(Position), typeof(AudioSource)), new NoneOfComponents(typeof(TurnedOn)));

	private Family f_newStep = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
	private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));

	private GameData gameData;
	
	private bool slotsConnected;

	public GameObject pathUnitPrefab;


	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		
		f_consoleOn.addEntryCallback(onNewConsoleTurnedOn); // Console will enter in this family when TurnedOn component will be added to console (see CurrentActionExecutor)
		f_consoleOff.addEntryCallback(onNewConsoleTurnedOff); // Console will enter in this family when TurnedOn component will be removed from console (see CurrentActionExecutor)
		f_gameLoaded.addEntryCallback(connectDoorsAndConsoles);
		
		f_newStep.addEntryCallback(delegate { onNewStep(); });
	}

	private void onNewConsoleTurnedOn(GameObject console)
	{
		onConsoleStateChanged(console, true);
	}

	private void onNewConsoleTurnedOff(GameObject console)
	{
		onConsoleStateChanged(console, false);
	}

	private void onConsoleStateChanged(GameObject console, bool isOn)
	{
		if (!slotsConnected) return;
		
		Actionable actionable = console.GetComponent<Actionable>();
		actionable.connected = true;
		actionable.sinceActivation = 0;
		updatePath(actionable, isOn);
	}
	
	private void updatePath(Actionable actionable, bool isOn)
	{
		// parse all slots controled by this console
		foreach (int slotId in actionable.paths.Keys)
		{
			DoorPath path = actionable.paths[slotId];
			
			int sinceActivation = actionable.sinceActivation;
			int unitID = sinceActivation - path.offset;
			
			if (unitID < 0 || sinceActivation >= path.duration + path.offset)
				continue;
			
			updateUnitColor(path.units[unitID], isOn);
			
			// check if signal arrived to door slot
			if (path.door == null || sinceActivation != path.duration + path.offset - 1)
				continue;
			
			// show / hide door
			Transform parent = path.door.transform.parent;
			parent.GetComponent<AudioSource>().Play();
			parent.GetComponent<Animator>().SetTrigger(isOn ? "Close" : "Open");
			parent.GetComponent<Animator>().speed = gameData.gameSpeed_current;
		}
	}
	
	private void updateUnitColor(PathUnit unit, bool isOn)
	{
		foreach (SpriteRenderer sr in unit.GetComponentsInChildren<SpriteRenderer>()) 
			sr.color = isOn ? unit.colorOn : unit.colorOff;
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
				Actionable actionable = console.GetComponent<Actionable>();
				bool isOn = console.GetComponent<TurnedOn>() != null;

				if (!actionable.paths.ContainsKey(doorSlot.slotID)) continue;
				
				DoorPath doorPath = actionable.paths[doorSlot.slotID];
				doorPath.door = door;
					
				// Connect this console with this door
				Position consolePos = console.GetComponent<Position>();
				int xStep = consolePos.x < doorPos.x ? 1 : consolePos.x == doorPos.x ? 0 : -1;
				int yStep = consolePos.y < doorPos.y ? 1 : consolePos.y == doorPos.y ? 0 : -1;

				int x, y;
				string[] xDirs = { "West", "East" }, yDirs = { "South", "North" };
				
				for (x = 0; consolePos.x + x != doorPos.x; x += xStep)
					createPathUnit(doorPath, doorSlot.color, consolePos.y, consolePos.x + x + xStep / 2f, xDirs, isOn);

				for (y = 0; consolePos.y + y != doorPos.y; y += yStep)
					createPathUnit(doorPath, doorSlot.color, consolePos.y + y + yStep / 2f, consolePos.x + x, yDirs, isOn);
            }
        }

		slotsConnected = true;
    }

	private void createPathUnit(DoorPath path, Color color, float gridX, float gridY, string[] dirs, bool state)
	{
		Vector3 pos = gameData.LevelGO.transform.position + new Vector3(gridX * 3, 3, gridY * 3);
		
		GameObject pathGo = Object.Instantiate(pathUnitPrefab, pos, Quaternion.identity, path.transform);
		pathGo.transform.Find(dirs[0]).gameObject.SetActive(true);
		pathGo.transform.Find(dirs[1]).gameObject.SetActive(true);
		
		PathUnit pathUnit = pathGo.GetComponent<PathUnit>();
		pathUnit.colorOn = color;
		Color.RGBToHSV(color, out var h, out var s, out _);
		pathUnit.colorOff = Color.HSVToRGB(h, s-0.2f, 0.2f);

		updateUnitColor(pathUnit, state);
								
		path.units.Add(pathUnit);
	}
	
	private void onNewStep()
	{
		foreach (GameObject console in f_console)
		{
			Actionable actionable = console.GetComponent<Actionable>();

			if (!actionable.connected) continue;

			actionable.sinceActivation++;
			updatePath(actionable, f_consoleOn.Contains(console));
		}
	}
}