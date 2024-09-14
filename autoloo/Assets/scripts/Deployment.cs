using Newtonsoft.Json.Linq;
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
    public int _coin;
    public int coin
    {
        get { return _coin; }
        set
        {
            _coin = value;
            this?.OnCoinChanged(_coin);
        }
    }
    public Action<int> OnCoinChanged;
    public GameObject goDeploymentMarker;
    public GameObject goShopMarker;
    public List<DeploymentMarker> listLeftDeploymentMarkers;
    public List<DeploymentMarker> listRightDeploymentMarkers;
    public List<DeploymentShopMarker> listLeftDeploymentShopMarkers;
    public List<DeploymentShopMarker> listRightDeploymentShopMarkers;
    public Dictionary<int, Vector3> deploymentShopQueuePositions;
    public Dictionary<int, Vector3> deploymentQueuePositions;
    public GUIStyle defaultGuiStyle;
    public Texture btnEndTurn;
    public Texture btnFreeze;
    public Texture btnSell;
    public Texture btnRoll;
    public JToken opponentDraftData;
    private bool checkOpponentGenerationCompleted = false;
    private bool endTurnButtonClicked = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = (GameManager)FindObjectOfType(typeof(GameManager));
        deploymentShopQueuePositions = SetDrawnHandPositionLocations();
        SetResourcePointsDisplay();
        deploymentQueuePositions = SetDeploymentQueuePositionLocations();
        defaultGuiStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleLeft,
            margin = new RectOffset(2, 2, 2, 2),
            padding = new RectOffset(2, 2, 2, 2),
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };
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
        StartCoroutine(GetOpponentDraftData());
    }

    private void Update()
    {
        if (checkOpponentGenerationCompleted)
        {
            var completionFlag = GameObject.Find("OpponentGenerationCompleted");
            if (completionFlag != null)
            {
                SetupBattle();
                Destroy(completionFlag);
                checkOpponentGenerationCompleted = false;
                GUI.enabled = true;
            }
            else
            {
                Debug.Log("waiting on opponent data for battle");
            }
        }
    }

    private void SetResourcePointsDisplay()
    {
        var texMeshProComponent = Camera.main.gameObject.transform.GetComponentInChildren(typeof(TextMeshPro), true);
        var textResourcePoints = (TextMeshPro)texMeshProComponent;
        textResourcePoints.text = coin.ToString();
        OnCoinChanged += (e) => textResourcePoints.text = coin.ToString();
    }

    private void OnGUI()
    {
        if (gameManager.InBattleModeAndNotDeploymentMode == false)
        {
            //END TURN start fight
            if (!endTurnButtonClicked && GUI.Button(new Rect(Screen.width - 250, Screen.height - 150, 253, 175), btnEndTurn, defaultGuiStyle))
            {
                var playerDeployedUnitsCount = FindObjectsOfType<Unit>().Where(y => y.side == "left" && y.Deployed).Count();
                if (playerDeployedUnitsCount > 0)
                {
                    endTurnButtonClicked = true;
                    checkOpponentGenerationCompleted = true;
                    OpponentGeneration.GenerateFromDraftData(gameManager, opponentDraftData);
                    opponentDraftData = null;
                    ClearScoutReport();
                }
                else
                {
                    //TODO: Call Notifications class (TODO: actually make a notifications class)
                    gameManager.notificationExpireTime = Time.time + 5; //5 seconds
                    gameManager.notification += "*Please deploy at least one unit*";
                    if (coin < 3)
                    {
                        coin = 3;
                        Roll(false);
                    }
                }

            }
            //FREEZE UNIT
            if (gameManager.selectedUnit != null && !gameManager.selectedUnit.Deployed && GUI.Button(new Rect(250, Screen.height - 165, 250, 150), btnFreeze, defaultGuiStyle))
            {
                gameManager.selectedUnit.Freezed = !gameManager.selectedUnit.Freezed;
            }
            //SELL UNIT
            if (gameManager.selectedUnit != null && gameManager.selectedUnit.Deployed && GUI.Button(new Rect(250, Screen.height - 165, 250, 150), btnSell, defaultGuiStyle))
            {
                coin = coin + 1 + gameManager.selectedUnit.CurrencyBumpBasedOnRank(gameManager.selectedUnit.Rank);
                Destroy(gameManager.selectedUnit.gameObject);
                gameManager.Deselect();
            }
            //START ROLL
            if (GUI.Button(new Rect(0, Screen.height - 165, 250, 150), btnRoll, defaultGuiStyle))
            {
                Roll();
            }
            if (endTurnButtonClicked && !checkOpponentGenerationCompleted)
            {
                endTurnButtonClicked = false;
            }
        }
    }

    private static void ClearScoutReport()
    {
        var report = Camera.main.gameObject.transform.Find("ScoutReport");
        for (int i = 0; i < report.childCount; i++)
        {
            report.GetChild(i).GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    public IEnumerator GetOpponentDraftData()
    {
        if (opponentDraftData == null)
        {
            var draftDataTask = OpponentGeneration.GetDraftDataAsync(gameManager.autolooPlayerData.PlayerName, gameManager.roundNumber, gameManager.WIN, gameManager.LOSS);
            yield return new WaitUntil(() => draftDataTask.IsCompleted);
            opponentDraftData = draftDataTask.Result;
        }
    }

    private void SetupBattle()
    {
        gameManager.Deselect();
        StartCoroutine(gameManager.cameraControl.FadeOutMoveFadeIn(gameManager.cameraPositions[0]));
        gameManager.InBattleModeAndNotDeploymentMode = true;
        gameManager.actionTime = Time.time + gameManager.period;

        PopulateQueues();

        foreach (var unit in gameManager.LeftQueueUnits)
        {
            gameManager.autolooPlayerData.unitDetails.Add(unit.GetDetail());
        }

        StoreAndLoadArmyDetails.Store(gameManager.autolooPlayerData, gameManager.roundNumber, gameManager.WIN, gameManager.LOSS);

        gameManager.ArrangeUnitsOnBattlefield(ref gameManager.LeftQueueUnits, gameManager.fightQueuePositions);
        gameManager.ArrangeUnitsOnBattlefield(ref gameManager.RightQueueUnits, gameManager.fightQueuePositions);
    }

    private void PopulateQueues()
    {
        var allUnits = FindObjectsOfType<Unit>().ToList();
        gameManager.LeftQueueUnits = allUnits.Where(y => y.side == "left" && y.Deployed).OrderByDescending(x => x.QueuePosition).ToList();
        gameManager.RightQueueUnits = allUnits.Where(y => y.side == "right").OrderBy(x => x.QueuePosition).ToList();
    }

    public void Roll(bool costOneCoin = true)
    {
        if (costOneCoin)
        {
            if (coin < 1)
            {
                return;
            }
            coin--;
        }
        GenerateShopQueueUnitsFromRoster(gameManager.LeftUnitRoster.OrderBy(x => x.Chance).ToList());
    }

    public void GenerateShopQueueUnitsFromRoster(List<Unit> roster)
    {
        System.Random rnd = new System.Random();
        var oldShop = FindObjectsOfType<Unit>().ToList().Where(x => x.side == gameManager.playerSide && !x.Deployed && !x.Freezed);
        foreach (GameObject old in oldShop.Select(x => x.gameObject))
        {
            Destroy(old.gameObject);
        }

        var shopMarkers = FindObjectsOfType<DeploymentShopMarker>().ToList().Where(x => x.side == gameManager.playerSide);
        var shopQueue = new List<Unit>();
        var unitsAvailableInThisTier = roster.Where(x => x.tier <= gameManager.roundNumber).ToList();

        // Calculate the total chance sum
        int totalChance = unitsAvailableInThisTier.Sum(x => x.Chance);

        foreach (var shopMarker in shopMarkers)
        {
            Unit shopItem = null;
            if (!shopMarker.IsFrozenShopUnitAboveMe())
            {
                // Weighted random selection
                int roll = rnd.Next(0, totalChance);
                int cumulative = 0;
                foreach (var unit in unitsAvailableInThisTier)
                {
                    cumulative += unit.Chance;
                    if (roll < cumulative)
                    {
                        shopItem = Instantiate(unit);
                        break;
                    }
                }
            }
            shopQueue.Add(shopItem);
        }

        gameManager.ArrangeUnitsInstantly(ref shopQueue, deploymentShopQueuePositions);
    }

    private Dictionary<int, Vector3> SetDeploymentQueuePositionLocations()
    {
        var dict = new Dictionary<int, Vector3>();
        dict.Add(-5, new Vector3(-130, 7, 2));
        dict.Add(-4, new Vector3(-120, 7, 2));
        dict.Add(-3, new Vector3(-110, 7, 2));
        dict.Add(-2, new Vector3(-100, 7, 2));
        dict.Add(-1, new Vector3(-90, 7, 2));
        dict.Add(1, new Vector3(64, 7, 2));
        dict.Add(2, new Vector3(72, 7, 2));
        dict.Add(3, new Vector3(80, 7, 2));
        dict.Add(4, new Vector3(88, 7, 2));
        dict.Add(5, new Vector3(96, 7, 2));
        return dict;
    }

    public int ComputeTotalChance(List<Unit> roster)
    {
        return roster.Sum(unit => unit.Chance);
    }

    private Dictionary<int, Vector3> SetDrawnHandPositionLocations()
    {
        var dict = new Dictionary<int, Vector3>();
        //dict.Add(-5, new Vector3(-100, -18, 1));
        //dict.Add(-4, new Vector3(-92, -18, 1));
        dict.Add(-3, new Vector3(-125, -10, 0.8f));
        dict.Add(-2, new Vector3(-115, -10, 0.9f));
        dict.Add(-1, new Vector3(-105, -10, 1));
        dict.Add(1, new Vector3(80, -10, 1));
        dict.Add(2, new Vector3(88, -10, 1));
        dict.Add(3, new Vector3(96, -10, 1));
        //dict.Add(4, new Vector3(92, -18, 1));
        //dict.Add(5, new Vector3(100, -18, 1));
        return dict;
    }

    public void ShiftUnits(int shiftWithRespectToPosition, int targetPositionKey)
    {
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
    }
    //TODO: refactor TrySnapToDeploymentQueueSpace
    public void TrySnapToDeploymentQueueSpace(Unit unit, DeploymentMarker belowDeploymentMarker, Vector3 startPosition)
    {

        if (unit.CanAfford() || unit.Deployed)
        {
            var occupant = belowDeploymentMarker.occupant;
            if (occupant == null)
            {
                unit.DeployAndSnapPositionToDeploymentMarker(belowDeploymentMarker);
            }
            else if (occupant.spriteName == unit.spriteName && occupant.Rank < occupant.MaxUnitRank)
            {
                occupant.RankUp(unit);
            }
            //per feedback from Thomas, when occupant and selected units are both already deployed and they are adjacent (queueposition diff is 1), then this should be a swap action.
            else if (unit.Deployed && Math.Abs(occupant.QueuePosition - unit.QueuePosition) == 1)
            {
                var pos1 = listLeftDeploymentMarkers.Where(x => x.positionKey == unit.QueuePosition && x.side == unit.side).First();
                var pos2 = belowDeploymentMarker;
                unit.DeployAndSnapPositionToDeploymentMarker(pos2);
                occupant.DeployAndSnapPositionToDeploymentMarker(pos1);
            }
            else if (IsThereAnEmptySpaceToShiftTo(belowDeploymentMarker.positionKey, out int ShiftPositionKey))
            {
                ShiftUnits(belowDeploymentMarker.positionKey, ShiftPositionKey);
            }
            else
            {
                //snap back to start position
                //TODO need a message for the user "sell units to make more room"
                unit.transform.position = startPosition;
            }
        }
        else
        {
            //snap back to start position
            //TODO: need a message for the user containing a message if CanAfford() == false
            unit.transform.position = startPosition;
        }
        gameManager.Deselect();
    }

    public bool IsThereAnEmptySpaceToShiftTo(int shiftWithRespectToPosition, out int ShiftPositionKey)
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
                        ShiftPositionKey = closestLesserNeighbor.positionKey;
                        return true;
                    }
                    else
                    {
                        idxLesser--;
                    }
                }
                //if the previous steps in this function found no closest lesser neighbor, then try to get the closest greater neighbor
                var idxGreater = shiftWithRespectToPosition;
                while (idxGreater < 0)
                {
                    DeploymentMarker closestGreaterNeighbor = null;
                    try { closestGreaterNeighbor = relevantDeploymentMarkers.Where(y => y.positionKey > idxGreater && y.side == gameManager.playerSide).OrderBy(z => z.positionKey).First(); } catch (Exception ex) { Debug.Log(ex.Message); }
                    if (closestGreaterNeighbor != null && closestGreaterNeighbor.occupant == null)//if space exists and is vaccant
                    {
                        ShiftPositionKey = closestGreaterNeighbor.positionKey;
                        return true;
                    }
                    else
                    {
                        idxGreater++;
                    }
                }
            }
        }
        //there is not room
        ShiftPositionKey = 0;
        return false;
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
