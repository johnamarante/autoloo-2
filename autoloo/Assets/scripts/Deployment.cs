using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Deployment : MonoBehaviour
{
    public GameManager gameManager;
    public int _commandPoints;
    public int CommandPoints
    {
        get { return _commandPoints; }
        set
        {
            _commandPoints = value;
            this?.OnCommandPointsChanged(_commandPoints);
        }
    }
    public Action<int> OnCommandPointsChanged;
    public GameObject goDeploymentMarker;
    public List<DeploymentMarker> listLeftDeploymentMarkers;
    public List<DeploymentMarker> listRightDeploymentMarkers;
    public Unit unit1;
    public Unit unit2;
    public Unit unit3;
    public Dictionary<int, Vector3> drawnHandQueuePositions;
    public Dictionary<int, Vector3> deploymentQueuePositions;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = (GameManager)FindObjectOfType(typeof(GameManager));
        drawnHandQueuePositions = SetDrawnHandPositionLocations();
        SetCommandPointsDisplay();
        Roll(false);
        deploymentQueuePositions = SetDeploymentQueuePositionLocations();

        //setup deployment queues
        foreach (var position in deploymentQueuePositions)
        {
            GameObject agoDeploymentMarker = Instantiate(goDeploymentMarker);
            agoDeploymentMarker.transform.position = position.Value;
            var deplomentMarker = agoDeploymentMarker.GetComponent<DeploymentMarker>();
            if (position.Key < 0)
            {
                listLeftDeploymentMarkers.Add(deplomentMarker);
            }
            else
            {
                listRightDeploymentMarkers.Add(deplomentMarker);
            }
            deplomentMarker.positionKey = position.Key;
            deplomentMarker.deployment = gameObject.GetComponent<Deployment>();
        }
    }

    private void SetCommandPointsDisplay()
    {
        var texMeshProComponent = Camera.main.gameObject.transform.GetComponentInChildren(typeof(TextMeshPro), true);
        var textCommandpoints = (TextMeshPro)texMeshProComponent;
        textCommandpoints.text = CommandPoints.ToString();
        OnCommandPointsChanged += (e) => textCommandpoints.text = CommandPoints.ToString();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnGUI()
    {
        if (gameManager.InBattleModeAndNotDeploymentMode == false)
        {
            //START FIGHT
            if (GUI.Button(new Rect(Screen.width - 50, Screen.height - 50, 50, 50), "start"))
            {
                gameManager.Deselect();
                Camera.main.GetComponent<CameraControl>().Move(gameManager.cameraPositions[0]);
                gameManager.InBattleModeAndNotDeploymentMode = true;
                gameManager.actionTime = Time.time;

                var allUnits = FindObjectsOfType<Unit>().ToList();
                gameManager.LeftQueueUnits = allUnits.Where(y => y.side == "left" && y.Deployed).OrderByDescending(x => x.QueuePosition).ToList();
                gameManager.RightQueueUnits = allUnits.Where(y => y.side == "right" &&  y.Deployed).OrderBy(x => x.QueuePosition).ToList();

                gameManager.SetUpUnitsOnBattlefieldInOrder(ref gameManager.LeftQueueUnits, gameManager.fightQueuePositions);
                gameManager.SetUpUnitsOnBattlefieldInOrder(ref gameManager.RightQueueUnits, gameManager.fightQueuePositions);
            }
            //SELL UNIT
            if (gameManager.selectedUnit != null && gameManager.selectedUnit.Deployed && GUI.Button(new Rect(100, Screen.height - 50, 50, 50), "sell"))
            {
                Destroy(gameManager.selectedUnit.gameObject);
                gameManager.Deselect();
                CommandPoints++;
            }
            //START ROLL
            if (GUI.Button(new Rect(0, Screen.height - 50, 50, 50), "roll"))
            {
                Roll();
            }
        }
    }

    private void Roll(bool costOnePoint = true)
    {
        if (costOnePoint)
        {
            if (CommandPoints < 1)
            {
                return;
            }
            CommandPoints--;
        }
        if (gameManager.playerSide == "left")
        {
            DrawHand(gameManager.LeftUnitRoster.OrderBy(x => x.Chance).ToList(), ref gameManager.LeftQueueUnits);
        }
        else //right
        {
            DrawHand(gameManager.RightUnitRoster.OrderBy(x => x.Chance).ToList(), ref gameManager.RightQueueUnits);
        }
    }

    public void DrawHand(List<Unit> roster, ref List<Unit> queueUnits)
    {
        var oldHand = FindObjectsOfType<Unit>().ToList().Where(x => x.side == gameManager.playerSide && (!x.Deployed));
        foreach (GameObject old in oldHand.Select(x => x.gameObject))
        {
            Destroy(old.gameObject);
        }

        //chose a random element on the roster
        System.Random rnd = new System.Random();

        var hand1 = Instantiate(roster[rnd.Next(roster.Count)]);
        var hand2 = Instantiate(roster[rnd.Next(roster.Count)]);
        var hand3 = Instantiate(roster[rnd.Next(roster.Count)]);
        var handQueue = new List<Unit>();
        handQueue.Add(hand1);
        handQueue.Add(hand2);
        handQueue.Add(hand3);
        //gameManager.QueueUnits(handQueue);
        gameManager.OrderUnitsInstantly(ref handQueue, drawnHandQueuePositions);
    }

    private Dictionary<int, Vector3> SetDeploymentQueuePositionLocations()
    {
        var dict = new Dictionary<int, Vector3>();
        dict.Add(-5, new Vector3(-96, -4, 2));
        dict.Add(-4, new Vector3(-88, -4, 2));
        dict.Add(-3, new Vector3(-80, -4, 2));
        dict.Add(-2, new Vector3(-72, -4, 2));
        dict.Add(-1, new Vector3(-64, -4, 2));
        dict.Add(1, new Vector3(64, -4, 2));
        dict.Add(2, new Vector3(72, -4, 2));
        dict.Add(3, new Vector3(80, -4, 2));
        dict.Add(4, new Vector3(88, -4, 2));
        dict.Add(5, new Vector3(96, -4, 2));
        return dict;
    }

    public int ComputeTotalChance(List<Unit> roster)
    {
        var totalChance = 0;
        foreach (var unit in roster)
        {
            totalChance += unit.Chance;
        }
        return totalChance;
    }

    private Dictionary<int, Vector3> SetDrawnHandPositionLocations()
    {
        var dict = new Dictionary<int, Vector3>();
        dict.Add(-5, new Vector3(-100, -18, 1));
        dict.Add(-4, new Vector3(-92, -18, 1));
        dict.Add(-3, new Vector3(-84, -18, 1));
        dict.Add(-2, new Vector3(-76, -18, 1));
        dict.Add(-1, new Vector3(-68, -18, 1));
        dict.Add(1, new Vector3(68, -18, 1));
        dict.Add(2, new Vector3(76, -18, 1));
        dict.Add(3, new Vector3(84, -18, 1));
        dict.Add(4, new Vector3(92, -18, 1));
        dict.Add(5, new Vector3(100, -18, 1));
        return dict;
    }

    public void ShiftUnits(int shiftWithRespectToPosition)
    {
        //shiftWithRespectToPosition is the positionkey of an occupied deploymentmarker that someone is trying to barge into
        var _isThereRoominTheDeployQueueAndQueuePosition = IsThereAnEmptySpaceToShiftTo(shiftWithRespectToPosition);
        if (_isThereRoominTheDeployQueueAndQueuePosition.IsThereAnEmptySpaceToShiftTo)
        {
            Debug.Log($"There IS room in the deploy queue! space: {_isThereRoominTheDeployQueueAndQueuePosition.PositionKey}");
            var isDownShift = (_isThereRoominTheDeployQueueAndQueuePosition.PositionKey < shiftWithRespectToPosition);
            var deploymentMarkersInShift = FindObjectsOfType<DeploymentMarker>().Where(x => x.positionKey >= Math.Min(shiftWithRespectToPosition, _isThereRoominTheDeployQueueAndQueuePosition.PositionKey) && x.positionKey <= Math.Max(shiftWithRespectToPosition, _isThereRoominTheDeployQueueAndQueuePosition.PositionKey)).ToList();
            var unitsInShift = FindObjectsOfType<Unit>().Where(x => deploymentMarkersInShift.Select(y => y.positionKey).ToList<int>().Contains(x.QueuePosition) && x.Deployed).ToList();
            gameManager.selectedUnit.QueuePosition = shiftWithRespectToPosition;
            unitsInShift.Add(gameManager.selectedUnit);
            unitsInShift = unitsInShift.Distinct().ToList();
            if (isDownShift)
            {
                deploymentMarkersInShift = deploymentMarkersInShift.OrderBy(x => x.positionKey).ToList();
                unitsInShift = unitsInShift.OrderBy(y => y.QueuePosition).ToList();
            }
            else
            {
                deploymentMarkersInShift = deploymentMarkersInShift.OrderByDescending(x => x.positionKey).ToList();
                unitsInShift = unitsInShift.OrderByDescending(y => y.QueuePosition).ToList();
            }

            //unitsInShift.Add(gameManager.selectedUnit);
            //var unitsInShiftNoDupes = unitsInShift.Distinct().ToList();
            foreach (var (aUnit, idx) in unitsInShift.Select((value, i) => (value, i)))
            {
                aUnit.DeployAndSnapToDeploymentQueue(deploymentMarkersInShift[idx]);
            }
            gameManager.Deselect();
            return;
        }
        Debug.Log("There is NOT room in the deploy queue!");
    }

    public void TrySnapToDeploymentQueueSpace(Unit unit, GameObject belowGameObject, Vector3 startPosition)
    {
        if (belowGameObject != null 
            && belowGameObject.TryGetComponent(out DeploymentMarker deploymentMarker)
            && deploymentMarker != null 
            && (unit.CanAfford() || unit.Deployed))
        {
            if (deploymentMarker.occupant == null)
            {
                //either there is no one in the space, or...
                unit.DeployAndSnapToDeploymentQueue(deploymentMarker);
            }
            else if (deploymentMarker.occupant.GetUnitSpriteName() == unit.GetUnitSpriteName())
            {
                deploymentMarker.occupant.RankUp(unit);
            }
            else 
            {
                //there is someone in the space, but there might be room to shift so that the selected unit can occupy this space
                //Mar 8 2023 330pm: if a unit is dragged to an occupied space and mouseup'd then, contraty to the previous expectation, it will not snap back but remain in place
                //the next step is to make shift units work. 
                ShiftUnits(deploymentMarker.positionKey);
            }
        }
        else
        {
            //snap back
            unit.transform.position = startPosition;
        }
    }

    //possibly make this bool function a tuple (bool, int); which is (isPositionAvailable, positionKey)
    private (bool IsThereAnEmptySpaceToShiftTo, int PositionKey) IsThereAnEmptySpaceToShiftTo(int shiftWithRespectToPosition)
    {
        //is there even space to shift to?
        var relevantDeploymentMarkers = FindObjectsOfType<DeploymentMarker>().ToList();
        if (shiftWithRespectToPosition < 0)
        {
            if (relevantDeploymentMarkers.Where(x => x.positionKey < 0 && x.occupant == null).ToList().Count() > 0)
            {
                //(LEFT/FRANCE) there is rooom
                //try to get the closest lesser neighbor
                var idxLesser = shiftWithRespectToPosition;
                while (idxLesser > -7)
                {
                    DeploymentMarker closestLesserNeighbor = null;
                    try { closestLesserNeighbor = relevantDeploymentMarkers.Where(y => y.positionKey < idxLesser && y.side == gameManager.playerSide).OrderByDescending(z => z.positionKey).First(); } catch (Exception ex) { Debug.Log(ex.Message); }
                    if (closestLesserNeighbor != null && closestLesserNeighbor.occupant == null)//if space exists and is vaccant
                    {
                        Debug.Log($"closest lesser neighbor that is vacant has positionkey {closestLesserNeighbor.positionKey}");
                        return (true, closestLesserNeighbor.positionKey);
                    }
                    else
                    {
                        idxLesser--;
                    }
                }
                Debug.Log($"No lesser neighbor that is vacant was found.");

                //failing that, try to get the closest greater neighbor
                var idxGreater = shiftWithRespectToPosition;
                while (idxGreater < 0)
                {
                    DeploymentMarker closestGreaterNeighbor = null;
                    try { closestGreaterNeighbor = relevantDeploymentMarkers.Where(y => y.positionKey > idxGreater && y.side == gameManager.playerSide).OrderBy(z => z.positionKey).First(); } catch (Exception ex) { Debug.Log(ex.Message); }
                    if (closestGreaterNeighbor != null && closestGreaterNeighbor.occupant == null)//if space exists and is vaccant
                    {
                        Debug.Log($"closest greater neighbor that is vacant has positionkey {closestGreaterNeighbor.positionKey}");
                        return (true, closestGreaterNeighbor.positionKey);
                    }
                    else {
                        idxGreater++;
                    }
                }
                Debug.Log($"No greater neighbor that is vacant was found.");
            }
        }
        else
        {
            if (relevantDeploymentMarkers.Where(x => x.positionKey > 0 && x.occupant == null).ToList().Count() > 0)
            {
                //(RIGHT/ALLIES) there is rooom
                return (true, 0);
            }
        }
        //there is not room
        return (false, 0);
    }

    public void SetDeployMarkerArrows(Unit selectedUnit)
    {
        if (selectedUnit != null)
        {
            foreach (var deployMarker in listLeftDeploymentMarkers)
            {
                //get child arrow and activate
                deployMarker.gameObject.transform.Find("arrow").gameObject.SetActive(true);
            }
        }
        else 
        {
            foreach (var deployMarker in listLeftDeploymentMarkers)
            {
                //get child arrow and activate
                deployMarker.gameObject.transform.Find("arrow").gameObject.SetActive(false);
            }
        }
    }
}
