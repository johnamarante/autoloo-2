using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

public class Deployment : MonoBehaviour
{
    public GameManager gameManager;
    public int _gold;
    public int gold
    {
        get { return _gold; }
        set
        {
            _gold = value;
            this?.OnGoldChanged(_gold);
        }
    }
    public Action<int> OnGoldChanged;
    public GameObject goDeploymentMarker;
    public GameObject goShopMarker;
    public List<DeploymentMarker> listLeftDeploymentMarkers;
    public List<DeploymentMarker> listRightDeploymentMarkers;
    public List<DeploymentShopMarker> listLeftDeploymentShopMarkers;
    public List<DeploymentShopMarker> listRightDeploymentShopMarkers;
    public Dictionary<int, Vector3> deploymentShopQueuePositions;
    public Dictionary<int, Vector3> deploymentQueuePositions;

    private bool writeToFriendPaste = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = (GameManager)FindObjectOfType(typeof(GameManager));
        deploymentShopQueuePositions = SetDrawnHandPositionLocations();
        SetCommandPointsDisplay();
        
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
        //setup shop queues and roll to populate shop
        foreach (var position in deploymentShopQueuePositions)
        {
            GameObject agoShopMarker = Instantiate(goShopMarker);
            agoShopMarker.transform.position = position.Value;
            var deplomentShopMarker = agoShopMarker.GetComponent<DeploymentShopMarker>();
            if (position.Key < 0)
            {
                deplomentShopMarker.side = "left";
                listLeftDeploymentShopMarkers.Add(deplomentShopMarker);
            }
            else
            {
                deplomentShopMarker.side = "right";
                listRightDeploymentShopMarkers.Add(deplomentShopMarker);
            }
            deplomentShopMarker.positionKey = position.Key;
            deplomentShopMarker.deployment = gameObject.GetComponent<Deployment>();
        }
        Roll(false);
    }

    private void SetCommandPointsDisplay()
    {
        var texMeshProComponent = Camera.main.gameObject.transform.GetComponentInChildren(typeof(TextMeshPro), true);
        var textCommandpoints = (TextMeshPro)texMeshProComponent;
        textCommandpoints.text = gold.ToString();
        OnGoldChanged += (e) => textCommandpoints.text = gold.ToString();
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

                //get data about opponent
                //write data to save between turns
                OpponentGeneration.Generate();

                var allUnits = FindObjectsOfType<Unit>().ToList();
                gameManager.LeftQueueUnits = allUnits.Where(y => y.side == "left" && y.Deployed).OrderByDescending(x => x.QueuePosition).ToList();
                gameManager.RightQueueUnits = allUnits.Where(y => y.side == "right").OrderBy(x => x.QueuePosition).ToList();
                
                writeToFriendPaste = true;

                gameManager.SetUpUnitsOnBattlefieldInOrder(ref gameManager.LeftQueueUnits, gameManager.fightQueuePositions);
                gameManager.SetUpUnitsOnBattlefieldInOrder(ref gameManager.RightQueueUnits, gameManager.fightQueuePositions);
                //StartCoroutine("WriteToFriendPaste");
            }
            //FREEZE UNIT
            if (gameManager.selectedUnit != null && !gameManager.selectedUnit.Deployed && GUI.Button(new Rect(150, Screen.height - 50, 50, 50), "reserve"))
            {
                gameManager.selectedUnit.Freezed = !gameManager.selectedUnit.Freezed;
            }
            //SELL UNIT
            if (gameManager.selectedUnit != null && gameManager.selectedUnit.Deployed && GUI.Button(new Rect(100, Screen.height - 50, 50, 50), "dismiss"))
            {
                gold = gold + 1 + gameManager.selectedUnit.CurrencyBumpBasedOnRank(gameManager.selectedUnit.Rank);
                Destroy(gameManager.selectedUnit.gameObject);
                gameManager.Deselect();
            }
            //START ROLL
            if (GUI.Button(new Rect(0, Screen.height - 50, 50, 50), "roll"))
            {
                Roll();
            }
        }
    }

    private async void WriteToFriendPaste()
    {
        var response = await FriendpasteClient.PostDataAsync("https://www.friendpaste.com/", $"autoloo test post {Guid.NewGuid()}", gameManager.LeftQueueUnits.Count.ToString());
        Debug.Log(response);
    }

    private void Roll(bool costOnePoint = true)
    {
        if (costOnePoint)
        {
            if (gold < 1)
            {
                return;
            }
            gold--;
        }
        if (gameManager.playerSide == "left")
        {
            GenerateShopQueueUnitsFromRoster(gameManager.LeftUnitRoster.OrderBy(x => x.Chance).ToList(), ref gameManager.LeftQueueUnits);
        }
        else //right
        {
            GenerateShopQueueUnitsFromRoster(gameManager.RightUnitRoster.OrderBy(x => x.Chance).ToList(), ref gameManager.RightQueueUnits);
        }
    }

    public void GenerateShopQueueUnitsFromRoster(List<Unit> roster, ref List<Unit> queueUnits)
    {
        var oldShop = FindObjectsOfType<Unit>().ToList().Where(x => x.side == gameManager.playerSide && !x.Deployed && !x.Freezed);
        foreach (GameObject old in oldShop.Select(x => x.gameObject))
        {
            Destroy(old.gameObject);
        }

        //chose a random element on the roster
        System.Random rnd = new System.Random();

        var shopMarkers = FindObjectsOfType<DeploymentShopMarker>().ToList().Where(x => x.side == gameManager.playerSide);
        var shopQueue = new List<Unit>();
        foreach (var shopMarker in shopMarkers)
        {
            Unit shopItem = null;
            if (!shopMarker.IsFrozenShopUnitAboveMe())
            {
                shopItem = Instantiate(roster[rnd.Next(roster.Count)]);
            }
            shopQueue.Add(shopItem);
        }

        gameManager.OrderUnitsInstantly(ref shopQueue, deploymentShopQueuePositions);
    }

    private Dictionary<int, Vector3> SetDeploymentQueuePositionLocations()
    {
        var dict = new Dictionary<int, Vector3>();
        dict.Add(-5, new Vector3(-96, 7, 2));
        dict.Add(-4, new Vector3(-88, 7, 2));
        dict.Add(-3, new Vector3(-80, 7, 2));
        dict.Add(-2, new Vector3(-72, 7, 2));
        dict.Add(-1, new Vector3(-64, 7, 2));
        dict.Add(1, new Vector3(64, 7, 2));
        dict.Add(2, new Vector3(72, 7, 2));
        dict.Add(3, new Vector3(80, 7, 2));
        dict.Add(4, new Vector3(88, 7, 2));
        dict.Add(5, new Vector3(96, 7, 2));
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
        //dict.Add(-5, new Vector3(-100, -18, 1));
        //dict.Add(-4, new Vector3(-92, -18, 1));
        dict.Add(-3, new Vector3(-96, -10, 0));
        dict.Add(-2, new Vector3(-88, -10, 0.1f));
        dict.Add(-1, new Vector3(-80, -10, 0.2f));
        dict.Add(1, new Vector3(80, -10, 1));
        dict.Add(2, new Vector3(88, -10, 1));
        dict.Add(3, new Vector3(96, -10, 1));
        //dict.Add(4, new Vector3(92, -18, 1));
        //dict.Add(5, new Vector3(100, -18, 1));
        return dict;
    }

    public void ShiftUnits(int shiftWithRespectToPosition, int targetPositionKey)
    {
        //var _isThereRoominTheDeployQueueAndQueuePosition = IsThereAnEmptySpaceToShiftTo(shiftWithRespectToPosition);
        //need to snap back if this is false
        //if (_isThereRoominTheDeployQueueAndQueuePosition.IsThereAnEmptySpaceToShiftTo)
        //{
        var userTargetedDeployMarker = listLeftDeploymentMarkers.Where(x => x.positionKey == shiftWithRespectToPosition).First();
        var vaccantDeployMarker = listLeftDeploymentMarkers.Where(x => x.positionKey == targetPositionKey).First();
        var diff = Math.Abs(targetPositionKey - shiftWithRespectToPosition);
        if (diff > 1)
        {
            var minPostion = Math.Min(targetPositionKey, shiftWithRespectToPosition);
            var maxPostion = Math.Max(targetPositionKey, shiftWithRespectToPosition);
            bool isDownShift = (shiftWithRespectToPosition > targetPositionKey);

            //need to determine up or down shift, who is involved in the shift
            var deployMarkersInShift = listLeftDeploymentMarkers.Where(x => x.positionKey >= minPostion && x.positionKey <= maxPostion).ToList();

            //downshift? start at the bottom. upshift? start at the op.
            if (isDownShift)
            {
                deployMarkersInShift = deployMarkersInShift.OrderBy(x => x.positionKey).ToList();
            }
            else
            {
                deployMarkersInShift = deployMarkersInShift.OrderByDescending(x => x.positionKey).ToList();
            }

            for (int i = 0; i < deployMarkersInShift.Count(); i++)
            {
                try
                {
                    deployMarkersInShift[i + 1].occupant.DeployAndSnapPositionToDeploymentMarker(deployMarkersInShift[i]);
                }
                catch (Exception)
                {
                    gameManager.selectedUnit.DeployAndSnapPositionToDeploymentMarker(deployMarkersInShift[i]);
                }
            }
        }
        else
        {
            userTargetedDeployMarker.occupant.DeployAndSnapPositionToDeploymentMarker(vaccantDeployMarker);
            gameManager.selectedUnit.DeployAndSnapPositionToDeploymentMarker(userTargetedDeployMarker);
        }
        //}
    }
    //TODO: refactor TrySnapToDeploymentQueueSpace
    public void TrySnapToDeploymentQueueSpace(Unit unit, GameObject belowGameObject, Vector3 startPosition)
    {
        if (belowGameObject != null 
            && belowGameObject.TryGetComponent(out DeploymentMarker deploymentMarker)
            && deploymentMarker != null 
            && (unit.CanAfford() || unit.Deployed))
        {
            var occupant = deploymentMarker.occupant;
            var _isThereRoominTheDeployQueueAndQueuePosition = IsThereAnEmptySpaceToShiftTo(deploymentMarker.positionKey);
            if (occupant == null)
            {
                unit.DeployAndSnapPositionToDeploymentMarker(deploymentMarker);
            }
            else if (occupant.spriteName == unit.spriteName && occupant.Rank < Unit.maxUnitRank)
            {
                occupant.RankUp(unit);
            }
            //per feedback from Thomas, when occupant and selected units are both already deployed and they are adjacent (queueposition diff is 1), then this should be a swap action.
            else if (unit.Deployed && Math.Abs(occupant.QueuePosition - unit.QueuePosition) == 1)
            {
                var pos1 = listLeftDeploymentMarkers.Where(x => x.positionKey == unit.QueuePosition && x.side == unit.side).First();
                var pos2 = deploymentMarker;
                unit.DeployAndSnapPositionToDeploymentMarker(pos2);
                occupant.DeployAndSnapPositionToDeploymentMarker(pos1);
            }
            else if (IsThereAnEmptySpaceToShiftTo(deploymentMarker.positionKey).IsThereAnEmptySpaceToShiftTo)
            {
                ShiftUnits(deploymentMarker.positionKey, _isThereRoominTheDeployQueueAndQueuePosition.PositionKey);
            }
            else {
                //snap back to start position
                unit.transform.position = startPosition;
            }
        }
        else {
            //snap back to start position
            unit.transform.position = startPosition;
        }
        gameManager.Deselect();
    }

    public (bool IsThereAnEmptySpaceToShiftTo, int PositionKey) IsThereAnEmptySpaceToShiftTo(int shiftWithRespectToPosition)
    {
        //is there even space to shift to?
        var relevantDeploymentMarkers = listLeftDeploymentMarkers;
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
                        //Debug.Log($"closest lesser neighbor that is vacant has positionkey {closestLesserNeighbor.positionKey}");
                        return (true, closestLesserNeighbor.positionKey);
                    }
                    else {
                        idxLesser--;
                    }
                }

                //failing that, try to get the closest greater neighbor
                var idxGreater = shiftWithRespectToPosition;
                while (idxGreater < 0)
                {
                    DeploymentMarker closestGreaterNeighbor = null;
                    try { closestGreaterNeighbor = relevantDeploymentMarkers.Where(y => y.positionKey > idxGreater && y.side == gameManager.playerSide).OrderBy(z => z.positionKey).First(); } catch (Exception ex) { Debug.Log(ex.Message); }
                    if (closestGreaterNeighbor != null && closestGreaterNeighbor.occupant == null)//if space exists and is vaccant
                    {
                        //Debug.Log($"closest greater neighbor that is vacant has positionkey {closestGreaterNeighbor.positionKey}");
                        return (true, closestGreaterNeighbor.positionKey);
                    }
                    else {
                        idxGreater++;
                    }
                }
            }
        }
        //there is not room
        return (false, 0);
    }

    public void SetDeployMarkerArrows(Unit selectedUnit)
    {
        if (selectedUnit != null && selectedUnit.CanAfford())
        {
            foreach (var deployMarker in listLeftDeploymentMarkers)
            { 
                if (deployMarker.occupant != null && selectedUnit.spriteName == deployMarker.occupant.spriteName)
                {
                    deployMarker.goCombine.SetActive(true);
                }
                else if (FindObjectsOfType<Unit>().Where(x => x.Deployed).Count() < 5 || selectedUnit.Deployed)
                {
                    deployMarker.goArrow.SetActive(true);
                }
            }
        }
        else 
        {
            foreach (var deployMarker in listLeftDeploymentMarkers)
            {
                deployMarker.goArrow.SetActive(false);
                deployMarker.goCombine.SetActive(false);
            }
        }
    }
}
