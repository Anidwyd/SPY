using System;
using UnityEngine;
using System.Linq;
using FYFY;
using Object = UnityEngine.Object;

/// <summary>
/// Manage Doors and Consoles => open/close doors depending on consoles state
/// </summary>
public class DoorAndConsoleManager : FSystem
{
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
        resetConsole(actionable);
        updatePath(actionable, isOn);
    }

    private void resetConsole(Actionable console)
    {
        console.connected = true;
        console.sinceActivation = 0;
        foreach (DoorPath path in console.paths.Values)
            path.pointer = 0;
    }

    private void updatePath(Actionable actionable, bool isOn)
    {
        // parse all slots controled by this console
        foreach (int slotId in actionable.paths.Keys)
        {
            DoorPath path = actionable.paths[slotId];

            int sinceActivation = actionable.sinceActivation;

            // check if propagation can start or continue
            if (sinceActivation < path.offset || sinceActivation >= path.duration + path.offset) continue;

            int lastPos = (int)path.pointer;
            path.pointer += path.step;

            // update path color
            for (int i = lastPos; i < (int)path.pointer; i++)
                updateUnitColor(path.units[i], isOn);

            // check if signal has reached the door
            if (path.door == null || path.pointer < path.length)
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
        SpriteRenderer[] renderers = unit.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer r in renderers)
            r.color = isOn ? unit.colorOn : unit.colorOff;
    }

    private void connectDoorsAndConsoles(GameObject unused)
    {
        string[] xDirs = { "West", "East" }, yDirs = { "South", "North" };

        // Parse all consoles
        foreach (GameObject console in f_console)
        {
            Position consolePos = console.GetComponent<Position>();
            Actionable actionable = console.GetComponent<Actionable>();
            bool isOn = console.GetComponent<TurnedOn>() != null;

            // Parse all doors
            foreach (GameObject door in f_door)
            {
                Position doorPos = door.GetComponent<Position>();
                ActivationSlot doorSlot = door.GetComponent<ActivationSlot>();
                int slotID = doorSlot.slotID;
                Color color = doorSlot.color;

                // Check if door is controlled by this console
                if (!actionable.paths.ContainsKey(slotID)) continue;

                // Build the path
                DoorPath path = actionable.paths[slotID];
                path.door = door;

                // Connect this console with this door
                int xStep = consolePos.x < doorPos.x ? 1 : consolePos.x == doorPos.x ? 0 : -1;
                int yStep = consolePos.y < doorPos.y ? 1 : consolePos.y == doorPos.y ? 0 : -1;
                int x, y;

                for (x = 0; consolePos.x + x != doorPos.x; x += xStep)
                    createPathUnit(path, color, consolePos.y, consolePos.x + x + xStep / 2f, xDirs, isOn);

                for (y = 0; consolePos.y + y != doorPos.y; y += yStep)
                    createPathUnit(path, color, consolePos.y + y + yStep / 2f, consolePos.x + x, yDirs, isOn);

                path.length = path.units.Count;
                path.step = (float)path.length / path.duration;
                
                // Update tooltip
                string colorTag = $"<#{ColorUtility.ToHtmlStringRGBA(color)}>";
                TooltipContent tooltip = console.GetComponentInChildren<TooltipContent>();
                tooltip.text += $"\n{colorTag}duration {path.duration} - offset {path.offset}</color>";
            }
        }

        slotsConnected = true;
    }

    private void createPathUnit(DoorPath path, Color color, float gridX, float gridY, string[] dirs, bool state)
    {
        Vector3 pos = gameData.LevelGO.transform.position + new Vector3(gridX * 3, 3, gridY * 3);

        GameObject pathUnit = Object.Instantiate(pathUnitPrefab, pos, Quaternion.identity, path.transform);
        pathUnit.transform.Find(dirs[0]).gameObject.SetActive(true);
        pathUnit.transform.Find(dirs[1]).gameObject.SetActive(true);

        PathUnit unit = pathUnit.GetComponent<PathUnit>();
        unit.colorOn = color;
        Color.RGBToHSV(color, out var h, out var s, out _);
        unit.colorOff = Color.HSVToRGB(h, s - 0.2f, 0.2f);

        updateUnitColor(unit, state);

        path.units.Add(unit);
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