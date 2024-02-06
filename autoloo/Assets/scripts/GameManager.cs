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
    public int cleanupFrame = 30;
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
    public int roundNumber = 1;
    public int roundCycle = 1;
    public int WIN = 0;
    public int LOSS = 0;
    private int leftUnitTakeDamage = 0;
    private int rightUnitTakeDamage = 0;
    public string developerMessage;
    public float notificationExpireTime = 0f;
    public string notification;

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
        };
        Screen.SetResolution(1920, 1080, true);
    }

    private void RemoveAudioSourcesOnGameManager()
    {
        //This method is used to clear off any audio sources added to the enduring gamemanager object
        //by non-enduring objects like units or cannonballs in the battle phase.
        //This enduring objet gamemanager is used for the audio sources because without it the audio sources will 
        //not complete playing to the end of theri respective tracks, which is a disruptive effect for the player.
        AudioSource[] componentsToRemove = this.gameObject.GetComponents<AudioSource>();
        foreach (AudioSource component in componentsToRemove)
        {
            Destroy(component);
        }
    }

    private void PlayAsBritian()
    {
        LeftUnitRoster = BritishUnitRoster;
        foreach (var un in LeftUnitRoster)
        {
            un.side = "left";
        }
        RightUnitRoster = FrenchUnitRoster;
        foreach (var un in RightUnitRoster)
        {
            un.side = "right";
        }
    }
    private void PlayAsFrance()
    {
        LeftUnitRoster = FrenchUnitRoster;
        foreach (var un in LeftUnitRoster)
        {
            un.side = "left";
        }
        RightUnitRoster = BritishUnitRoster;
        foreach (var un in RightUnitRoster)
        {
            un.side = "right";
        }
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
        dict.Add(-1, new Vector3(-60, 3, -10));
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
                //DEPLOY SKIRMISHERS
                //CHECK FOR SQUARE FORMATION
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
                Cleanup();
                CheckForAndHandleBattleResult();
                var activeCannonballsCount = FindObjectsOfType<Cannonball>().Length;
                preBattlePhaseProcessAndCleanupCompleted = activeCannonballsCount == 0;
            }
            if (Time.time > (actionTime + ((period / 2))) && preBattlePhaseFired && preBattlePhaseProcessAndCleanupFired && preBattlePhaseProcessAndCleanupCompleted && !battlePhaseFired)
            {
                Debug.Log("D");
                //MAIN START
                ComputeDamages();
                Move();
                battlePhaseFired = true;
            }
            if (Time.time > (actionTime + ((period / 2) + (period / 8))) && preBattlePhaseFired && preBattlePhaseProcessAndCleanupFired && preBattlePhaseProcessAndCleanupCompleted && battlePhaseFired && !battlePhaseProcessAndCleanupFired)
            {
                Debug.Log("E");
                Fight(ref LeftQueueUnits, ref RightQueueUnits);
                Cleanup();
                CheckForAndHandleBattleResult();
                roundCycle++;
                battlePhaseProcessAndCleanupFired = true;
            }
            if (Time.time > (actionTime + ((period / 2) + (period / 4))) && preBattlePhaseFired && preBattlePhaseProcessAndCleanupFired && preBattlePhaseProcessAndCleanupCompleted && battlePhaseFired && battlePhaseProcessAndCleanupFired && !battlePhaseProcessAndCleanupCompleted)
            {
                Debug.Log("F");
                Move();
                actionTime += period;
                battlePhaseProcessAndCleanupCompleted = true;
                preBattlePhaseFired = false;
                preBattlePhaseProcessAndCleanupFired = false;
                preBattlePhaseProcessAndCleanupCompleted = false;
                battlePhaseFired = false;
                battlePhaseProcessAndCleanupFired = false;
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

    private void ComputeDamages()
    {
        if (LeftQueueUnits.Count > 0 && RightQueueUnits.Count > 0)
        {
            // Cavalry attacking in a square case
            if (LeftQueueUnits[0].Squared && RightQueueUnits[0].isCavalry)
            {
                leftUnitTakeDamage = 1;
                rightUnitTakeDamage = LeftQueueUnits[0].Attack + LeftQueueUnits[0].AttackBonus;
            }
            else if (LeftQueueUnits[0].isCavalry && RightQueueUnits[0].Squared)
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

    public void SquareCheck(Unit unitA, Unit unitB)
    {
        //Avoid firing the change event if there is no actual change
        bool newSquaredValue = (roundCycle <= 1 || unitA.cycle > 1)
                               && unitA.canFormSquare && !unitA.SkirmishMode
                               && unitB.isCavalry;
        if (unitA.Squared != newSquaredValue)
        {
            unitA.Squared = newSquaredValue;
        }
    }

    public void SkirmishCheck()
    {
        var leftSkirmishers = GetSkirmisherFromQueue(LeftQueueUnits);
        var rightSkirmishers = GetSkirmisherFromQueue(RightQueueUnits);
        List<Unit> combinedList = leftSkirmishers.Concat(rightSkirmishers).ToList();
        foreach (var unit in combinedList)
        {
            unit.GetComponent<Skirmish>().DeploySkirmishers();
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

    List<Unit> GetSkirmisherFromQueue(List<Unit> units)
    {
        var skirmisherUnits = new List<Unit>();
        foreach (Unit unit in units)
        {
            if (unit.gameObject.GetComponent<Skirmish>() && unit.Deployed && unit.SkirmishMode && Math.Abs(unit.QueuePosition) == 1)
            {
                skirmisherUnits.Add(unit);
            }
        }
        return skirmisherUnits;
    }

    public void Cleanup()
    {
        List<int> leftEliminatedIndices = EliminateUnitsWithZeroHitPoints(ref LeftQueueUnits);
        List<int> rightEliminatedIndices = EliminateUnitsWithZeroHitPoints(ref RightQueueUnits);

        CleanupEliminatedUnits(leftEliminatedIndices, ref LeftQueueUnits);
        CleanupEliminatedUnits(rightEliminatedIndices, ref RightQueueUnits);
    }

    public void Move()
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

    public void CheckForAndHandleBattleResult()
    {
        if (LeftQueueUnits.Count == 0 || RightQueueUnits.Count == 0)
        {
            string resultText = (LeftQueueUnits.Count == 0) ? "LOSS" : "WIN!";
            preBattlePhaseFired = false;
            ShowResultPopup(resultText);
            autolooPlayerData.ClearUnitDetails();
            CleanupBattlefield();
            InBattleModeAndNotDeploymentMode = false;
            cameraControl.Move(cameraPositions[-1]);
            StoreAndLoadArmyDetails.Load(LeftUnitRoster, this);
            roundNumber++;
            deployment.Roll(false);
            deployment.coin = 10;
            if (resultText.StartsWith('W'))
            {
                WIN++;
            }
            else
            {
                LOSS++;
            }
            //reset the cycle
            roundCycle = 0;
        }
    }

    void ShowResultPopup(string resultText)
    {
        Debug.Log("Result is " + resultText);
        var result = Instantiate(resultPopup);
        result.displayText = resultText;
        result.gameManager = this;
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
            rightUnits[0].HitPoints -= rightUnitTakeDamage;
            leftUnits[0].cycle++;
            rightUnits[0].cycle++;
        }
    }

    private void FightEffects(ref List<Unit> leftUnits, ref List<Unit> rightUnits)
    {
        if (leftUnits.Count == 0 || rightUnits.Count == 0) return;

        ApplyEffect(leftUnits[0]);
        ApplyEffect(rightUnits[0]);
    }

    private void ApplyEffect(Unit unit)
    {
        if (unit.GetComponent<Artillery>())
        {
            unit.GetComponent<Artillery>().ShowEffect();
        }
        else
        {
            PlayTransientAudioClip(unit.acAttackSFX);
            if (!unit.GetComponent<Artillery>())
            {
                unit.showEffect = true;
            }
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
                var hupu = unit.name;
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
}
