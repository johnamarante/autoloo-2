using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Unit> LeftQueueUnits;
    public List<Unit> RightQueueUnits;
    public List<Unit> LeftUnitRoster;
    public List<Unit> RightUnitRoster;
    public List<Unit> FrenchUnitRoster;
    public List<Unit> BritishUnitRoster;
    public Dictionary<int, Vector3> fightQueuePositions;
    public Dictionary<int, Vector3> cameraPositions;
    public float actionTime;
    public float period = 1f;
    public int realFPS = 60;
    public string playerSide = "left";
    public Deployment deployment;
    public NumberFloating floatyNumber;
    public Sprite[] rankSprites;
    public bool _InBattleModeAndNotDeploymentMode;
    public bool InBattleModeAndNotDeploymentMode
    {
        get { return _InBattleModeAndNotDeploymentMode; }
        set
        {
            _InBattleModeAndNotDeploymentMode = value;
            this?.InBattleModeAndNotDeploymentModeChanged(_InBattleModeAndNotDeploymentMode);
        }
    }
    public bool preBattlePhaseFired = false;
    public bool preBattlePhaseProcessAndCleanupFired = false;
    public bool preBattlePhaseProcessAndCleanupCompleted = false;
    public bool battlePhaseFired = false;
    public bool battlePhaseProcessAndCleanupFired = false;
    public bool battlePhaseProcessAndCleanupCompleted = false;
    public AudioClip melee8;
    public AudioClip[] distBattle1AudioClips;
    public AudioClip[] cannonFire;
    public AudioClip[] cannonballHits;
    private int currentClipIndex;
    public AudioSource generalAudioSource;
    public CameraControl cameraControl;
    public Unit _selectedUnit;
    public Unit selectedUnit
    {
        get { return _selectedUnit; }
        set
        {
            _selectedUnit = value;
            this?.OnSelectedUnitChanged(_selectedUnit);
        }
    }
    public Action<Unit> OnSelectedUnitChanged;
    public Action<bool> InBattleModeAndNotDeploymentModeChanged;
    public ResultPopup resultPopup;
    public AutolooPlayerData autolooPlayerData;
    public int _roundNumber = 1;
    public int roundNumber
    {
        get { return _roundNumber; }
        set
        {
            _roundNumber = value;
            this?.OnRoundChanged(_roundNumber);
        }
    }
    public int roundCycle = 1;
    public int _win = 0;
    public int win
    {
        get { return _win; }
        set
        {
            _win = value;
            this?.OnWinChanged(_win);
        }
    }
    public int _loss;
    public int loss
    {
        get { return _loss; }
        set
        {
            _loss = value;
            this?.OnLossChanged(_loss);
        }
    }
    public int playerHearts = 5;
    public Action<int> OnRoundChanged;
    public Action<int> OnLossChanged;
    public Action<int> OnWinChanged;
    private int leftUnitTakeDamage = 0;
    private int rightUnitTakeDamage = 0;
    public string developerMessage;
    public float notificationExpireTime = 0f;
    public float unitLerpSpeed = 90.0F;
    public float postRoundWaitTime = 1.0f;
    public float fadeHandlerWaitTime = 1.0f;
    public string notification;
    public BattleMusicController battleMusicController;
    public AudioClip cymbal;
    public CombatEffect leftCombatEffect;
    public CombatEffect rightCombatEffect;
    public ConfettiBurst leftConfetti;
    public ConfettiBurst rightConfetti;

    // Start is called before the first frame update
    void Start()
    {
        fightQueuePositions = GetFightQueuePositionLocations();
        cameraPositions = SetCameraPositionLocations();
        generalAudioSource = Camera.main.gameObject.AddComponent<AudioSource>();
        cameraControl = Camera.main.GetComponent<CameraControl>();
        generalAudioSource.volume = 0.5f;
        generalAudioSource.loop = false;
        autolooPlayerData = FindObjectOfType<AutolooPlayerData>();
        switch (autolooPlayerData.RosterName)
        {
            case "France":
                PlayAsFrance();
                break;
            case "Britain":
                PlayAsBritian();
                break;
            default:
                Console.WriteLine("Roster not found");
                break;
        }
        deployment = Instantiate(deployment);
        floatyNumber = this.gameObject.GetComponent<NumberFloating>();
        OnSelectedUnitChanged += (e) => { Debug.Log($"selected unit is {e}"); deployment.SetDeployMarkerArrows(e); };
        SetHeartsAndWinsDisplay();
        InBattleModeAndNotDeploymentModeChanged += (e) => { 
            if (InBattleModeAndNotDeploymentMode)
            {
                generalAudioSource.Stop();
                generalAudioSource.clip = distBattle1AudioClips[0];
                generalAudioSource.volume = 0.7f;
                generalAudioSource.loop = true;
                generalAudioSource.Play();
            }
            else
            {
                generalAudioSource.Stop();
                generalAudioSource.volume = 0.5f;
                generalAudioSource.loop=false;
                generalAudioSource.clip = distBattle1AudioClips[0];
                generalAudioSource.Play();
                RemoveAudioSourcesOnGameManager();
            }
            cameraControl.SetHUD();
        };
        Screen.SetResolution(1920, 1080, true);
    }

    private void SetHeartsAndWinsDisplay()
    {
        var texMeshProComponents = Camera.main.gameObject.transform.GetComponentsInChildren(typeof(TextMeshPro), true);
        TextMeshPro textHearts;
        TextMeshPro textWins;
        TextMeshPro textTurn;
        for (int i = 0; i < texMeshProComponents.Length; i++)
        {
            if (texMeshProComponents[i].name == "Hearts")
            {
                textHearts = texMeshProComponents[i].GetComponent<TextMeshPro>();
                textHearts.text = (playerHearts - loss).ToString();
                OnLossChanged += (e) => textHearts.text = (playerHearts - loss).ToString();
                textHearts = (TextMeshPro)texMeshProComponents[i];
            }

            if (texMeshProComponents[i].name == "Wins")
            {
                textWins = texMeshProComponents[i].GetComponent<TextMeshPro>();
                textWins.text = win.ToString();
                OnWinChanged += (e) => textWins.text = win.ToString();
                textWins = (TextMeshPro)texMeshProComponents[i];
            }

            if (texMeshProComponents[i].name == "Turn")
            {
                textTurn = texMeshProComponents[i].GetComponent<TextMeshPro>();
                textTurn.text = roundNumber.ToString();
                OnRoundChanged += (e) => textTurn.text = roundNumber.ToString();
                textTurn = (TextMeshPro)texMeshProComponents[i];
            }
        }
    }

    private void RemoveAudioSourcesOnGameManager()
    {
        //This method is used to clear off any audio sources added to the enduring gamemanager object
        //by non-enduring objects like units or cannonballs in the battle phase.
        //This enduring objet gamemanager is used for the audio sources because without it the audio sources will 
        //not complete playing to the end of their respective tracks, which is a disruptive effect for the player.
        AudioSource[] componentsToRemove = this.gameObject.GetComponents<AudioSource>();
        foreach (AudioSource component in componentsToRemove)
        {
            Destroy(component);
        }
    }

    private void PlayAsBritian()
    {
        LeftUnitRoster = BritishUnitRoster;
        foreach (var unit in LeftUnitRoster)
        {
            unit.side = "left";
        }
        RightUnitRoster = FrenchUnitRoster;
        foreach (var unit in RightUnitRoster)
        {
            unit.side = "right";
        }
        battleMusicController.SetBritainMusic();
    }

    private void PlayAsFrance()
    {
        LeftUnitRoster = FrenchUnitRoster;
        foreach (var unit in LeftUnitRoster)
        {
            unit.side = "left";
        }
        RightUnitRoster = BritishUnitRoster;
        foreach (var unit in RightUnitRoster)
        {
            unit.side = "right";
        }
        battleMusicController.SetFranceMusic();
    }

    private Dictionary<int, Vector3> GetFightQueuePositionLocations()
    {
        var dict = new Dictionary<int, Vector3>();
        dict.Add(-5, new Vector3(-50, 7, 0.6f));
        dict.Add(-4, new Vector3(-40, 7, 0.7f));
        dict.Add(-3, new Vector3(-30, 7, 0.8f));
        dict.Add(-2, new Vector3(-20, 7, 0.9f));
        dict.Add(-1, new Vector3(-10, 7, 1));
        dict.Add(1, new Vector3(10, 7, 1));
        dict.Add(2, new Vector3(20, 7, 0.9f));
        dict.Add(3, new Vector3(30, 7, 0.8f));
        dict.Add(4, new Vector3(40, 7, 0.7f));
        dict.Add(5, new Vector3(50, 7, 0.6f));
        return dict;
    }

    private Dictionary<int, Vector3> SetCameraPositionLocations()
    {
        var dict = new Dictionary<int, Vector3>();
        dict.Add(-1, new Vector3(-90, 3, -10));
        dict.Add(0, new Vector3(0, 16, -10));
        dict.Add(1, new Vector3(60, 0, -10));
        return dict;
    }

    // Update is called once per frame
    void Update()
    {
        realFPS = (int)(1.0f / Time.deltaTime);
        if (InBattleModeAndNotDeploymentMode)
        {
            if (Time.time > (actionTime + (period / 8)) && !preBattlePhaseFired)
            {
                Debug.Log("A");
                battlePhaseProcessAndCleanupCompleted = false;
                SkirmishCheck();
                SquareChecks();
                preBattlePhaseFired = true;
            }
            if (Time.time > (actionTime + (period / 4)) && preBattlePhaseFired && !preBattlePhaseProcessAndCleanupFired)
            {
                Debug.Log("B");
                //ARTILLERY (FIRE BALL OR PREPARE GRAPE)
                ArtilleryPhase();
                preBattlePhaseProcessAndCleanupFired = true;
            }
            if (Time.time > (actionTime + ((period / 4) + (period / 8))) && preBattlePhaseFired && preBattlePhaseProcessAndCleanupFired && !preBattlePhaseProcessAndCleanupCompleted)
            {
                Debug.Log("C");
                //PRE BATTLE PHASE CLEANUP (FIRST CLEANUP)
                GrenadiersCheck();
                PostCycleCleanup();
                if (CheckForAndHandleBattleResult(out string potentialResult) && potentialResult != null)
                {
                    ShowResultPopup(potentialResult);
                    battleMusicController.PlaySingleClipAndStop(cymbal);
                    InBattleModeAndNotDeploymentMode = false;
                    StartCoroutine(FadeOutAndMoveAndFadeInHandler(cameraPositions[-1]));
                    StartCoroutine(PostRoundCleanup(potentialResult));
                }
                var activeCannonballsCount = FindObjectsOfType<Cannonball>().Length;
                preBattlePhaseProcessAndCleanupCompleted = (activeCannonballsCount == 0);
            }
            if (Time.time > (actionTime + ((period / 2))) && preBattlePhaseFired && preBattlePhaseProcessAndCleanupFired && preBattlePhaseProcessAndCleanupCompleted && !battlePhaseFired)
            {
                Debug.Log("D");
                //MAIN START
                ComputeAttackStrengths();
                AdvanceBattlefieldQueues();
                battlePhaseFired = true;
                //force into E and not F
                battlePhaseProcessAndCleanupFired = false;
                leftCombatEffect.isEnabled = true;
                rightCombatEffect.isEnabled = true;
            }
            if (Time.time > (actionTime + ((period / 2) + (period / 8))) && preBattlePhaseFired && preBattlePhaseProcessAndCleanupFired && preBattlePhaseProcessAndCleanupCompleted && battlePhaseFired && !battlePhaseProcessAndCleanupFired)
            {
                Debug.Log("E");
                Fight(ref LeftQueueUnits, ref RightQueueUnits);
                GrenadiersCheck();
                PostCycleCleanup();
                if (CheckForAndHandleBattleResult(out string potentialResult) && potentialResult != null)
                {
                    ShowResultPopup(potentialResult);
                    battleMusicController.PlaySingleClipAndStop(cymbal);
                    InBattleModeAndNotDeploymentMode = false;
                    StartCoroutine(FadeOutAndMoveAndFadeInHandler(cameraPositions[-1]));
                    StartCoroutine(PostRoundCleanup(potentialResult));

                }
                leftCombatEffect.isEnabled = false;
                rightCombatEffect.isEnabled = false;
                roundCycle++;
                battlePhaseProcessAndCleanupFired = true;
            }
            if (Time.time > (actionTime + ((period / 2) + (period / 4))) && preBattlePhaseFired && preBattlePhaseProcessAndCleanupFired && preBattlePhaseProcessAndCleanupCompleted && battlePhaseFired && battlePhaseProcessAndCleanupFired && !battlePhaseProcessAndCleanupCompleted)
            {
                AdvanceBattlefieldQueues();
                BattleModeBoolSwitchesReset();
            }
        }
        else
        {
            if (selectedUnit != null)
            {
                AdjustSelectedUnitPosition();
            }
            if (!generalAudioSource.isPlaying)
            {
                // Move to the next audio clip
                currentClipIndex = (currentClipIndex + 1) % distBattle1AudioClips.Length;
                generalAudioSource.clip = distBattle1AudioClips[currentClipIndex];
                generalAudioSource.Play();
            }
        }
    }

    private void GrenadiersCheck()
    {
        List<Unit> allUnits = GetFrontRankBaseUnitsThatCanDeployGrenadiers(LeftQueueUnits);
        allUnits.AddRange(GetFrontRankBaseUnitsThatCanDeployGrenadiers(RightQueueUnits));
        foreach (var unit in allUnits)
        {
            if (unit.HitPoints <= 0)
            {
                unit.GetComponent<GrenadierAttack>().DeployGrenadiers();
            }
        }
    }

    private void BattleModeBoolSwitchesReset()
    {
        actionTime += period;
        battlePhaseProcessAndCleanupCompleted = true;
        preBattlePhaseFired = false;
        preBattlePhaseProcessAndCleanupFired = false;
        preBattlePhaseProcessAndCleanupCompleted = false;
        battlePhaseFired = false;
        battlePhaseProcessAndCleanupFired = false;
        Debug.Log("F");
    }

    private void ComputeAttackStrengths()
    {
        if (LeftQueueUnits.Count > 0 && RightQueueUnits.Count > 0)
        {
            // Cavalry attacking in a square case
            if (LeftQueueUnits[0].Squared 
                && RightQueueUnits[0].isCavalry 
                && !(RightQueueUnits[0].cycle == 1 && RightQueueUnits[0].GetComponent<Cavalry>().isLancer))
            {
                leftUnitTakeDamage = 1;
                rightUnitTakeDamage = LeftQueueUnits[0].Attack + LeftQueueUnits[0].AttackBonus;
            }
            else if (LeftQueueUnits[0].isCavalry 
                && RightQueueUnits[0].Squared 
                && !(LeftQueueUnits[0].cycle == 1 && LeftQueueUnits[0].GetComponent<Cavalry>().isLancer))
            {

                leftUnitTakeDamage = RightQueueUnits[0].Attack + RightQueueUnits[0].AttackBonus;
                rightUnitTakeDamage = 1;
            }
            else
            {
                // Normative case
                leftUnitTakeDamage = RightQueueUnits[0].Attack + RightQueueUnits[0].AttackBonus;
                rightUnitTakeDamage = LeftQueueUnits[0].Attack + LeftQueueUnits[0].AttackBonus;
            }
        }
    }

    private void ArtilleryPhase()
    {
        var leftArtillery = GetArtilleryFromQueue(LeftQueueUnits);
        var rightArtillery = GetArtilleryFromQueue(RightQueueUnits);
        var allArtilleryUnits = new List<Unit>();
        foreach (var item in leftArtillery)
        {
            allArtilleryUnits.Add(item);
        }
        foreach (var item in rightArtillery)
        {
            allArtilleryUnits.Add(item);
        }
        allArtilleryUnits.OrderBy(item => Math.Abs(item.QueuePosition)).ToList();
        foreach (var item in allArtilleryUnits)
        {
            if (item.side == "left")
            {
                item.gameObject.GetComponent<Artillery>().Fire(RightQueueUnits[0]);
            }
            else
            {
                item.gameObject.GetComponent<Artillery>().Fire(LeftQueueUnits[0]);
            }
        }
    }

    private void SquareChecks()
    {
        SquareCheck(LeftQueueUnits[0], RightQueueUnits[0]);
        SquareCheck(RightQueueUnits[0], LeftQueueUnits[0]);
    }

    public void SquareCheck(Unit subjectUnit, Unit subjectOpponentUnit)
    {
        //Avoid firing the change event if there is no actual change
        //there is a round, within a round several cycles, and to each unit, several cycles, the first starting when that unit is at the front
        bool newSquaredValue = (roundCycle <= 1 || subjectUnit.cycle > 1)
                               && subjectUnit.canFormSquare && !subjectUnit.isSkirmisher
                               && subjectOpponentUnit.isCavalry;

        DeployFootDragoons mountedDragoon = subjectOpponentUnit.GetComponent<DeployFootDragoons>();
        if (mountedDragoon != null && newSquaredValue)
        {
            mountedDragoon.Dismount();
            subjectOpponentUnit.HitPoints = 0;
            DisableAllSpriteRenderers(subjectOpponentUnit.gameObject);
            PostCycleCleanup();
        }
        else if (subjectUnit.Squared != newSquaredValue)
        {
            subjectUnit.Squared = newSquaredValue;
        }
    }

    public void SkirmishCheck()
    {
        var leftSkirmishers = GetFrontRankBaseUnitsThatCanDeploySkirmishers(LeftQueueUnits);
        var rightSkirmishers = GetFrontRankBaseUnitsThatCanDeploySkirmishers(RightQueueUnits);
        foreach (var unit in leftSkirmishers)
        {
            //Units with skirmishers should not deploy skirmishers when faced with cavalry.
            //Rather, they should form a square.
            if (!RightQueueUnits[0].isCavalry)
            {
                var skirmisherComponent = unit.GetComponent<Skirmish>();
                skirmisherComponent.DeploySkirmishers("left", LeftQueueUnits);
                skirmisherComponent.enabled = false;
            }
        }
        foreach (var unit in rightSkirmishers)
        {
            if (!LeftQueueUnits[0].isCavalry)
            {
                var skirmisherComponent = unit.GetComponent<Skirmish>();
                skirmisherComponent.DeploySkirmishers("right", RightQueueUnits);
                skirmisherComponent.enabled = false;
            }
        }
    }

    List<Unit> GetArtilleryFromQueue(List<Unit> units)
    {
        var artilleryUnits = new List<Unit>();
        foreach (Unit unit in units)
        {
            if (unit.gameObject.GetComponent<Artillery>() && unit.Deployed)
            {
                artilleryUnits.Add(unit);
            }
        }
        return artilleryUnits;
    }

    List<Unit> GetFrontRankBaseUnitsThatCanDeploySkirmishers(List<Unit> units)
    {
        var skirmisherUnits = new List<Unit>();
        foreach (Unit unit in units)
        {
            if (unit.gameObject.GetComponent<Skirmish>() && unit.gameObject.GetComponent<Skirmish>().enabled && unit.Deployed && Math.Abs(unit.QueuePosition) == 1)
            {
                skirmisherUnits.Add(unit);
            }
        }
        return skirmisherUnits;
    }

    List<Unit> GetFrontRankBaseUnitsThatCanDeployGrenadiers(List<Unit> units)
    {
        var grenadierUnits = new List<Unit>();
        foreach (Unit unit in units)
        {
            if (unit.gameObject.GetComponent<GrenadierAttack>() && unit.gameObject.GetComponent<GrenadierAttack>().enabled && unit.Deployed && Math.Abs(unit.QueuePosition) == 1)
            {
                grenadierUnits.Add(unit);
            }
        }
        return grenadierUnits;
    }

    public void PostCycleCleanup()
    {
        List<int> leftEliminatedIndices = EliminateUnitsWithZeroHitPoints(ref LeftQueueUnits);
        List<int> rightEliminatedIndices = EliminateUnitsWithZeroHitPoints(ref RightQueueUnits);

        CleanupEliminatedUnits(leftEliminatedIndices, ref LeftQueueUnits);
        CleanupEliminatedUnits(rightEliminatedIndices, ref RightQueueUnits);
    }

    public void AdvanceBattlefieldQueues()
    {
        ArrangeUnitsOnBattlefield(ref LeftQueueUnits, fightQueuePositions);
        ArrangeUnitsOnBattlefield(ref RightQueueUnits, fightQueuePositions);
    }

    void CleanupEliminatedUnits(List<int> indices, ref List<Unit> queueUnits)
    {
        indices.Sort((a, b) => b.CompareTo(a)); // Sort indices in descending order
        foreach (int index in indices)
        {
            queueUnits.RemoveAt(index);
        }
    }

    public bool CheckForAndHandleBattleResult(out string result)
    {
        if (LeftQueueUnits.Count == 0 || RightQueueUnits.Count == 0)
        {
            result = (LeftQueueUnits.Count == 0) ? (RightQueueUnits.Count == 0 ? "DRAW" : "LOSS") : "WIN!";
            rightCombatEffect.isEnabled = false;
            leftCombatEffect.isEnabled = false;
            return true;

        }
        result = null;
        return false;
    }

    public IEnumerator FadeOutAndMoveAndFadeInHandler(Vector3 dest)
    {
        yield return new WaitForSeconds(fadeHandlerWaitTime);
        StartCoroutine(cameraControl.FadeOutMoveFadeIn(dest,true));
    }

    public IEnumerator PostRoundCleanup(string resultText)
    {
        yield return new WaitForSeconds(postRoundWaitTime);
        autolooPlayerData.ClearUnitDetails();
        CleanupBattlefield();
        StoreAndLoadArmyDetails.Load(LeftUnitRoster, this);
        roundNumber++;
        deployment.Roll(false);
        deployment.coin = 10;
        IncrementResult(resultText);
        //reset the cycle
        roundCycle = 0;
        BattleModeBoolSwitchesReset();
        StartCoroutine(deployment.GetOpponentDraftData());
    }

    private void IncrementResult(string resultText)
    {
        switch (resultText[0])
        {
            case 'W':
                win++;
                break;
            case 'L':
                loss++;
                break;
            default:
                //draw
                break;
        }
    }

    void ShowResultPopup(string resultText)
    {
        Debug.Log("Result is " + resultText);
        var result = Instantiate(resultPopup);
        result.displayText = resultText;
        result.gameManager = this;
        result.delayBeforeSelfDestruct = postRoundWaitTime;
    }

    void AdjustSelectedUnitPosition()
    {
        selectedUnit.transform.position = new Vector3(selectedUnit.transform.position.x, selectedUnit.transform.position.y, 0);
    }

    public void CleanupBattlefield()
    {
        foreach (var rightUnit in RightQueueUnits)
            Destroy(rightUnit.gameObject);
        foreach (var leftUnit in LeftQueueUnits)
            Destroy(leftUnit.gameObject);
    }

    private void OnGUI()
    {
        if (playerSide.Length > 0)
        {
            var screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            screenPoint.z = 1f; //distance of the plane from the camera
            Camera.main.ScreenToWorldPoint(screenPoint);
            developerMessage = (selectedUnit != null) ? selectedUnit.spriteName : "no unit is selected";
            developerMessage += $" real FPS: {realFPS}";
            GUI.Label(new Rect(10, Screen.height - 30, 30, Screen.width / 2), developerMessage, new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    textColor = Color.green
                },
                fontSize = 20
            });
        }
        if (notificationExpireTime > Time.time)
        {
            GUI.Label(new Rect(Screen.width / 2, Screen.height - 30, 30, Screen.width / 2), notification, new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                },
                fontSize = 30
            });
        }
    }

    private void Fight(ref List<Unit> leftUnits, ref List<Unit> rightUnits)
    {
        if (leftUnits.Count > 0 && rightUnits.Count > 0)
        {
            // Apply damage to HitPoints
            leftUnits[0].HitPoints -= leftUnitTakeDamage;
            Debug.Log($"fight AV effect for {rightUnits[0].name}"); 
            rightUnits[0].HitPoints -= rightUnitTakeDamage;
            Debug.Log($"fight AV effect for {leftUnits[0].name}");
            floatyNumber.SpawnFloatingNumber(-leftUnitTakeDamage, leftUnits[0].transform.position);
            floatyNumber.SpawnFloatingNumber(-rightUnitTakeDamage, rightUnits[0].transform.position);
            leftUnits[0].cycle++;
            rightUnits[0].cycle++;
        }
    }

    public void ArrangeUnitsOnBattlefield(ref List<Unit> units, Dictionary<int, Vector3> queuePositions)
    {
        int index = 1;
        foreach (Unit unit in units)
        {
            int indexModifier = (unit.side == "left") ? -1 : 1;
            unit.QueuePosition = indexModifier * index;
            unit.SetIntoMotion(queuePositions[unit.QueuePosition]);
            index++;
        }
    }

    public void ArrangeUnitsInstantly(ref List<Unit> units, Dictionary<int, Vector3> queuePositions)
    {
        int idx = 1;
        //go through queue
        foreach (Unit unit in units)
        {
            if (unit != null) //sometimes nulls are placed in the queue as placehoollders
            {
                var indexModifier = 1;
                if (unit.side == "left")
                {
                    indexModifier = -1;
                }
                unit.QueuePosition = indexModifier * idx;
                unit.transform.position = queuePositions[indexModifier * idx];
            }
            idx++;
        }
    }

    private List<int> EliminateUnitsWithZeroHitPoints(ref List<Unit> units)
    {
        var eliminatedIndecies = new List<int>();
        var index = 0;
        foreach (Unit unit in units)
        {
            if (unit.HitPoints <= 0 && unit != null)
            {
                if (unit.side == "left")
                {
                    leftConfetti.BurstConfetti(unit.burstColor1, unit.burstColor2, unit.burstColor3);
                }
                else 
                {
                    rightConfetti.BurstConfetti(unit.burstColor1, unit.burstColor2, unit.burstColor3);
                }
                Destroy(unit.gameObject);
                eliminatedIndecies.Add(index);
            }
            index++;
        }
        return eliminatedIndecies;
    }

    public void Deselect()
    {
        if (selectedUnit != null)
        {
            selectedUnit.ShowSelectionIndicator(false);
            selectedUnit = null;
        }
        foreach (var deploymarker in deployment.listLeftDeploymentMarkers)
        {
            deploymarker.ShowHoverIndicator(false);
        }
    }

    public void PlayCannonballFire()
    {
        System.Random random = new System.Random();
        int randomIndex = random.Next(0, cannonFire.Length);
        PlayTransientAudioClip(cannonFire[randomIndex]);
    }

    public void PlayCannonballHit()
    {
        System.Random random = new System.Random();
        int randomIndex = random.Next(0, cannonballHits.Length);
        PlayTransientAudioClip(cannonballHits[randomIndex]);
    }

    public void PlayTransientAudioClip(AudioClip transientAudioClip)
    {
        var boomAudio = this.gameObject.AddComponent<AudioSource>();
        boomAudio.loop = false;
        boomAudio.clip = transientAudioClip;
        boomAudio.Play();
    }

    public void DisableAllSpriteRenderers(GameObject obj)
    {
        // Get all SpriteRenderer components in the object and its children
        SpriteRenderer[] spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();

        // Disable each SpriteRenderer
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.enabled = false;
        }
    }
}
