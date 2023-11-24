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
    public int guiFontSize = 20;
    public bool multiplayer = false;
    public string playerSide = "left";
    //public Unit selectedUnit;
    public Deployment deployment;
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
    public bool preBattlePhaseCleanupFired = false;
    public AudioClip melee8;
    public AudioClip[] distBattle1AudioClips;
    public AudioClip[] cannonFire;
    public AudioClip[] cannonballHits;
    private int currentClipIndex;
    AudioSource generalAudioSource;

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
    public int cycle = 1;
    public int WIN = 0;
    public int LOSS = 0;
    private int frameCountFromStartOfLastPrebattlePhase = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        fightQueuePositions = SetFightQueuePositionLocations();
        cameraPositions = SetCameraPositionLocations();
        generalAudioSource = Camera.main.gameObject.AddComponent<AudioSource>();
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
        OnSelectedUnitChanged += (e) => { Debug.Log($"selected unit is {e}"); deployment.SetDeployMarkerArrows(e); };
        InBattleModeAndNotDeploymentModeChanged += (e) => { 
            string mode = InBattleModeAndNotDeploymentMode ? "in battle mode" : "in deployment mode";  
            Debug.Log($"{mode}");
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

    private Dictionary<int, Vector3> SetFightQueuePositionLocations()
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
            if (Time.time > (actionTime + (period / 4)) && !preBattlePhaseFired)
            {
                //ALL PRE BATTLE PHASE ACTIONS MUST BE FIXED TO COMPLETE IN LESS THAN 22 FRAMES
                PreBattlePhase();
                preBattlePhaseFired = true;
                cleanupFrame = (int)(realFPS / 2);
            }
            if (preBattlePhaseFired)
            {
                frameCountFromStartOfLastPrebattlePhase++;
                if (frameCountFromStartOfLastPrebattlePhase == cleanupFrame)
                {
                    CleanupAndMove();
                    CheckForAndHandleBattleResult();
                    preBattlePhaseCleanupFired = true;
                    FightEffects(ref LeftQueueUnits, ref RightQueueUnits);
                }
            }
            if (Time.time > (actionTime + 3*(period / 4)))
            {
                preBattlePhaseFired = false;
                preBattlePhaseCleanupFired = false;
                frameCountFromStartOfLastPrebattlePhase = 0;
                BattlePhase();
                actionTime += period;
                cycle++;
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

    void PreBattlePhase()
    {
        //square check
        if (cycle <= 1)
        {
            if (LeftQueueUnits[0].canFormSquare && RightQueueUnits[0].isCavalry)
            {
                LeftQueueUnits[0].Squared = true;
            }
            else
            {
                LeftQueueUnits[0].Squared = false;
            }
            if (RightQueueUnits[0].canFormSquare && LeftQueueUnits[0].isCavalry)
            {
                RightQueueUnits[0].Squared = true;
            }
            else
            {
                RightQueueUnits[0].Squared = false;
            }
        }
        if (cycle > 1)
        {
            if (LeftQueueUnits[0].cycle > 1 && LeftQueueUnits[0].canFormSquare && RightQueueUnits[0].isCavalry)
            {
                LeftQueueUnits[0].Squared = true;
            }
            else
            {
                LeftQueueUnits[0].Squared = false;
            }
            if (RightQueueUnits[0].cycle > 1 && RightQueueUnits[0].canFormSquare && LeftQueueUnits[0].isCavalry)
            {
                RightQueueUnits[0].Squared = true;
            }
            else
            {
                RightQueueUnits[0].Squared = false;
            }
        }

        //get all units with a prebattle event and fire those prebattle events in the order Artillery, position
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
            Debug.Log(item.name);
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

    void BattlePhase()
    {
        Fight(ref LeftQueueUnits, ref RightQueueUnits);
        CleanupAndMove();
        CheckForAndHandleBattleResult();
    }

    public void CleanupAndMove()
    {
        List<int> leftEliminatedIndices = EliminateUnitsWithZeroHitPoints(ref LeftQueueUnits);
        List<int> rightEliminatedIndices = EliminateUnitsWithZeroHitPoints(ref RightQueueUnits);

        CleanupEliminatedUnits(leftEliminatedIndices, ref LeftQueueUnits);
        CleanupEliminatedUnits(rightEliminatedIndices, ref RightQueueUnits);

        SetUpUnitsOnBattlefieldInArrangement(ref LeftQueueUnits, fightQueuePositions);
        SetUpUnitsOnBattlefieldInArrangement(ref RightQueueUnits, fightQueuePositions);
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
            Debug.Log("Result is " + resultText);
            ShowResultPopup(resultText);
            autolooPlayerData.ClearUnitDetails();
            CleanupBattlefield();
            InBattleModeAndNotDeploymentMode = false;
            Camera.main.GetComponent<CameraControl>().Move(cameraPositions[-1]);
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
            cycle = 0;
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
            //GUI.Label(new Rect(10, 10, 300, 300), screenPoint.ToString(), new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = guiFontSize });
            //GUI.Label(new Rect(10, 30, 300, 300), $"W: {Screen.width} H: {Screen.height}", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = guiFontSize });
            var developerMessage = (selectedUnit != null) ? selectedUnit.spriteName : "no unit is selected";
            developerMessage += $" real FPS: {realFPS}";
            GUI.Label(new Rect(10, Screen.height - 30, 30, 1000), developerMessage, new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = guiFontSize });
        }
    }

    private void Fight(ref List<Unit> leftUnits, ref List<Unit> rightUnits)
    {
        if (leftUnits.Count > 0 && rightUnits.Count > 0)
        {
            //Cavalry attacking in a square case
            if (leftUnits[0].Squared && rightUnits[0].isCavalry)
            {
                leftUnits[0].HitPoints -= 1;
                rightUnits[0].HitPoints -= (leftUnits[0].Attack + rightUnits[0].AttackBonus + 3);
            }
            else if (leftUnits[0].isCavalry && rightUnits[0].Squared)
            {
                leftUnits[0].HitPoints -= (rightUnits[0].Attack + rightUnits[0].AttackBonus + 3);
                rightUnits[0].HitPoints -= 1;
            }
            else
            {
                //Normative case
                leftUnits[0].HitPoints -= (rightUnits[0].Attack + rightUnits[0].AttackBonus);
                rightUnits[0].HitPoints -= (leftUnits[0].Attack + leftUnits[0].AttackBonus);
            }
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

    public void SetUpUnitsOnBattlefieldInArrangement(ref List<Unit> units, Dictionary<int, Vector3> queuePositions)
    {
        int idx = 1;
        //go through queue
        foreach (Unit unit in units)
        {
            var indexModifier = 1;
            if (unit.side == "left")
            {
                indexModifier = -1;
            }
            unit.QueuePosition = indexModifier * idx;
            unit.SetIntoMotion(queuePositions[indexModifier * idx]);
            idx++;
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
