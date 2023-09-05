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
    public int guiFontSize = 20;
    public bool multiplayer = false;
    public string playerSide = "left";
    //public Unit selectedUnit;
    public Deployment deployment;
    public Sprite[] rankSprites;
    public Texture playAsFrance;
    public Texture playAsGreatBritain;
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

    public AudioClip melee8;
    public AudioClip[] distBattle1AudioClips;
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
    

    // Start is called before the first frame update
    void Start()
    {
        fightQueuePositions = SetFightQueuePositionLocations();
        cameraPositions = SetCameraPositionLocations();
        generalAudioSource = Camera.main.GetComponent<AudioSource>();
        generalAudioSource.volume = 0.5f;
        generalAudioSource.loop = false;
        string sRoster = FindObjectOfType<AutolooUserGameData>().PlayerRoster;
        switch (sRoster)
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
                generalAudioSource.clip = melee8;
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
            }
        };
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
        dict.Add(-5, new Vector3(-45, 7, 0.6f));
        dict.Add(-4, new Vector3(-35, 7, 0.7f));
        dict.Add(-3, new Vector3(-25, 7, 0.8f));
        dict.Add(-2, new Vector3(-15, 7, 0.9f));
        dict.Add(-1, new Vector3(-5, 7, 1));
        dict.Add(1, new Vector3(5, 7, 1));
        dict.Add(2, new Vector3(15, 7, 1));
        dict.Add(3, new Vector3(25, 7, 1));
        dict.Add(4, new Vector3(35, 7, 1));
        dict.Add(5, new Vector3(45, 7, 1));
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

    public void QueueUnits()
    {
        var units = FindObjectsOfType<Unit>().ToList();
        LeftQueueUnits = units.Where(x => x.side == "left").ToList();
        RightQueueUnits = units.Where(x => x.side == "right").ToList();
    }

    // Update is called once per frame
    void Update()
    {
        if (InBattleModeAndNotDeploymentMode)
        {
            if (Time.time > (actionTime + (period/2)))
            {
                PreBattlePhase();
            }
            if (Time.time > (actionTime + period))
            {
                BattlePhase();
                actionTime += period;
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
                if (!generalAudioSource.isPlaying)
                {
                    // Move to the next audio clip
                    currentClipIndex = (currentClipIndex + 1) % distBattle1AudioClips.Length;
                    generalAudioSource.clip = distBattle1AudioClips[currentClipIndex];
                    generalAudioSource.Play();
                }
            }
        }
    }

    void PreBattlePhase()
    {
        //get all units with a prebattle event
        //fire those prebattle events in the order Artillery, position
    }

    void BattlePhase()
    {
        Fight(ref LeftQueueUnits, ref RightQueueUnits);

        List<int> leftEliminatedIndices = EliminateUnitsWithZeroHitPoints(ref LeftQueueUnits);
        List<int> rightEliminatedIndices = EliminateUnitsWithZeroHitPoints(ref RightQueueUnits);

        CleanupEliminatedUnits(leftEliminatedIndices, ref LeftQueueUnits);
        CleanupEliminatedUnits(rightEliminatedIndices, ref RightQueueUnits);

        SetUpUnitsOnBattlefieldInOrder(ref LeftQueueUnits, fightQueuePositions);
        SetUpUnitsOnBattlefieldInOrder(ref RightQueueUnits, fightQueuePositions);

        if (LeftQueueUnits.Count == 0 || RightQueueUnits.Count == 0)
        {
            HandleBattleResult();
        }
    }

    void CleanupEliminatedUnits(List<int> indices, ref List<Unit> queueUnits)
    {
        indices.Sort((a, b) => b.CompareTo(a)); // Sort indices in descending order
        foreach (int index in indices)
        {
            queueUnits.RemoveAt(index);
        }
    }

    void HandleBattleResult()
    {
        string resultText = (LeftQueueUnits.Count == 0) ? "LOSS" : "WIN!";
        Debug.Log("Result is " + resultText);

        ShowResultPopup(resultText);

        CleanupBattlefield();
        InBattleModeAndNotDeploymentMode = false;
        Camera.main.GetComponent<CameraControl>().Move(cameraPositions[-1]);
        StoreAndLoadArmyDetails.Load(LeftUnitRoster, this);
        deployment.Roll(false);
        deployment.coin = 10;
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
            var selectionText = (selectedUnit != null) ? selectedUnit.spriteName : "no unit is selected";
            GUI.Label(new Rect(10, Screen.height - 30, 30, 1000), selectionText, new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = guiFontSize });
        }
    }

    private void Fight(ref List<Unit> leftUnits, ref List<Unit> rightUnits)
    {
        if (leftUnits.Count > 0 && rightUnits.Count > 0)
        {
            leftUnits[0].HitPoints -= rightUnits[0].Attack;
            rightUnits[0].HitPoints -= leftUnits[0].Attack;
        }
    }

    public void SetUpUnitsOnBattlefieldInOrder(ref List<Unit> units, Dictionary<int, Vector3> queuePositions)
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

    public void OrderUnitsInstantly(ref List<Unit> units, Dictionary<int, Vector3> queuePositions)
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
}
