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
    public Dictionary<int, Vector3> fightQueuePositions;
    public Dictionary<int, Vector3> cameraPositions;
    public float actionTime;
    public float period = 1f;
    public int guiFontSize = 20;
    public bool multiplayer = false;
    public string playerSide;
    //public Unit selectedUnit;
    public Deployment deployment;
    public Sprite[] rankSprites;

    public bool InBattleModeAndNotDeploymentMode = false;
    
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
    public ResultPopup resultPopup;

    // Start is called before the first frame update
    void Start()
    {
        fightQueuePositions = SetFightQueuePositionLocations();
        cameraPositions = SetCameraPositionLocations();
        deployment = Instantiate(deployment);
        OnSelectedUnitChanged += (e) => { Debug.Log($"selected unit is {e}"); deployment.SetDeployMarkerArrows(e); };
    }

    private Dictionary<int, Vector3> SetFightQueuePositionLocations()
    {
        var dict = new Dictionary<int, Vector3>();
        dict.Add(-5, new Vector3(-36, -4, 1));
        dict.Add(-4, new Vector3(-28, -4, 1));
        dict.Add(-3, new Vector3(-20, -4, 1));
        dict.Add(-2, new Vector3(-12, -4, 1));
        dict.Add(-1, new Vector3(-4, -4, 1));
        dict.Add(1, new Vector3(4, -4, 1));
        dict.Add(2, new Vector3(12, -4, 1));
        dict.Add(3, new Vector3(20, -4, 1));
        dict.Add(4, new Vector3(28, -4, 1));
        dict.Add(5, new Vector3(36, -4, 1));
        return dict;
    }

    private Dictionary<int, Vector3> SetCameraPositionLocations()
    {
        var dict = new Dictionary<int, Vector3>();
        dict.Add(-1, new Vector3(-60, 0, -10));
        dict.Add(0, new Vector3(0, 0, -10));
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
        //need spring loaded unit quese to keep units moving to center of screen for fight
        if (Time.time > (actionTime + period)  && InBattleModeAndNotDeploymentMode)
        { 
            //Single Player mode
            Fight(ref LeftQueueUnits, ref RightQueueUnits);
            var leftEliminatedIndecies = EliminateUnitsWithZeroHitPoints(ref LeftQueueUnits);
            var rightEliminatedIndecies = EliminateUnitsWithZeroHitPoints(ref RightQueueUnits);

            //cleanup and reorder queues
            foreach (int i in leftEliminatedIndecies)
            {
                LeftQueueUnits.RemoveAt(i);
            }
            foreach (int i in rightEliminatedIndecies)
            {
                RightQueueUnits.RemoveAt(i);
            }
            SetUpUnitsOnBattlefieldInOrder(ref LeftQueueUnits, fightQueuePositions);
            SetUpUnitsOnBattlefieldInOrder(ref RightQueueUnits, fightQueuePositions);

            actionTime += period;

            if (LeftQueueUnits.Count == 0 || LeftQueueUnits.Count == 0)
            {
                //1. give result
                Debug.Log("result is " + ((LeftQueueUnits.Count == 0) ? "LOSS" : "WIN!"));
                var result = Instantiate(resultPopup);
                result.displayText = ((LeftQueueUnits.Count == 0) ? "LOSS" : "WIN!");
                result.gameManager = this;
                //2. cleanup battlefield
                CleanupBattlefield();
                //3. reset player or end the game
                InBattleModeAndNotDeploymentMode = false;
                Camera.main.GetComponent<CameraControl>().Move(cameraPositions[-1]);
            }
        }
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
        var screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        screenPoint.z = 1f; //distance of the plane from the camera
        Camera.main.ScreenToWorldPoint(screenPoint);
        GUI.Label(new Rect(10, 10, 300, 300), screenPoint.ToString(), new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = guiFontSize });
        GUI.Label(new Rect(10, 30, 300, 300), $"W: {Screen.width} H: {Screen.height}", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = guiFontSize });
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
    }
}
