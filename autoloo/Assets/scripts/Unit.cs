using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public GameManager gameManager;
    public int Chance = 0;
    public int _cost;
    public int Cost
    {
        get { return _cost; }
        set
        {
            _cost = value;
            this?.OnCostChanged(_cost);
        }
    }
    public Action<int> OnCostChanged;

    public int _attack;
    public int Attack
    {
        get { return _attack; }
        set
        {
            _attack = value;
            this?.OnAttackChanged(_attack);
        }
    }
    public Action<int> OnAttackChanged;

    public int _hitPoints;
    public int HitPoints
    {
        get { return _hitPoints; }
        set
        {
            _hitPoints = value;
            this?.OnHitPointsChanged(_hitPoints);
        }
    }
    public Action<int> OnHitPointsChanged;
    
    public int _rank = 0;
    public int Rank
    {
        get { return _rank; }
        set
        {
            _rank = value;
            this?.OnRankChanged(_rank);
        }
    }
    public const int maxUnitRank = 9;
    public Action<int> OnRankChanged;
    public string side = "";
    public Font myFont;
    public int QueuePosition = 0;
    public bool _deployed = false;
    public bool Deployed 
    {
        get { return _deployed; }
        set
        {
            _deployed = value;
            this?.OnDeployedChanged(_deployed);
        }
    }
    public Action<bool> OnDeployedChanged;

    public bool _freezed = false;
    public bool Freezed
    {
        get { return _freezed;  }
        set 
        {
            _freezed = value;
            this?.OnFreezedChanged(_freezed);
        }
    }
    public Action<bool> OnFreezedChanged;
    public string spriteName;
    //Lerping START
    public Vector3 startMarker;
    public Vector3 endMarker;
    // Movement speed in units per second.
    public float speed = 40.0F;
    // Time when the movement started.
    private float startTime;
    // Total distance between the markers.
    private float journeyLength;
    private bool inMoveMode = false;
    //Lerping END

    //Stats display START
    private TextMeshPro textAttack = new TextMeshPro();
    private TextMeshPro textHitPoints = new TextMeshPro();
    private TextMeshPro textCost = new TextMeshPro();
    private GameObject costComponent;
    private GameObject rankComponent;
    private GameObject freezeComponent;
    private SpriteRenderer spriteRank = new SpriteRenderer();
    //Stats display END
    private GameObject mouseHoverOverIndicator;
    private GameObject selectedIndicator;

    // Awake
    void Awake()
    {
        name = $"unit_{side}_{Guid.NewGuid()}";
        spriteName = GetSpriteName();
    }

    public string GetSpriteName()
    {
        //this needs to be a seperate function so that it can be called from roster prefab
        var spriteName = gameObject.transform.Find("svgsprite").GetComponent<SpriteRenderer>().sprite.name;
        return spriteName;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = (GameManager)FindObjectOfType(typeof(GameManager));
        SetUnitStatsDisplay();
        OnAttackChanged += (e) => textAttack.text = Attack.ToString();
        OnHitPointsChanged += (e) => textHitPoints.text = HitPoints.ToString();
        OnCostChanged += (e) => textCost.text = Cost.ToString();
        OnDeployedChanged += (e) => { costComponent.SetActive(!Deployed); rankComponent.SetActive(Deployed); };
        OnRankChanged += (e) =>  ChangeRankIcon();
        OnFreezedChanged += (e) => freezeComponent.SetActive(Freezed);
        //costComponent with old hat is going to be hidden, following SAP standard
        //costComponent.SetActive(!Deployed);
        rankComponent.SetActive(Deployed);
        mouseHoverOverIndicator = transform.Find("hover_over_indicator").gameObject;
        selectedIndicator = transform.Find("selected_indicator").gameObject;
    }

    private void ChangeRankIcon()
    {
        if (gameManager.rankSprites.Length > Rank)
        {
            spriteRank.sprite = gameManager.rankSprites[Rank];
        }
    }

    private void SetUnitStatsDisplay()
    {
        var tmpComponents = transform.GetComponentsInChildren(typeof(TextMeshPro), true);
        foreach (Component component in tmpComponents)
        {
            if (component.name == "HitPoints")
            { 
                textHitPoints = (TextMeshPro)component;
                textHitPoints.text = HitPoints.ToString();
            }
            if (component.name == "Attack")
            { 
                textAttack = (TextMeshPro)component;
                textAttack.text = Attack.ToString();
            }
            if (component.name == "Cost")
            {
                costComponent = component.gameObject;
                textCost = (TextMeshPro)component;
                textCost.text = Cost.ToString();
            }
            if (component.name == "Rank")
            {
                rankComponent = component.gameObject;
                spriteRank = component.gameObject.GetComponentInChildren<SpriteRenderer>();
                spriteRank.sprite = gameManager.rankSprites[Rank];
            }
        }
        freezeComponent = transform.Find("Freeze").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (inMoveMode)
            Move();
    }

    private void Move()
    {
        // Distance moved equals elapsed time times speed..
        float distCovered = (Time.time - startTime) * speed;

        // Fraction of journey completed equals current distance divided by total distance.
        // Only compute if journeyLength > 0, otherwise take out of move mode

        if (journeyLength > 0)
        {
            float fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            transform.position = Vector3.Lerp(startMarker, endMarker, fractionOfJourney);
            if (fractionOfJourney > 0.95F)
            {
                inMoveMode = false;
                transform.position = endMarker;
            }
        }
        else
        {
            inMoveMode = false;
        }
    }

    public void SetIntoMotion(Vector3 dest)
    {
        startMarker = transform.position;
        endMarker = dest;
        // Keep a note of the time the movement started.
        startTime = Time.time;

        // Calculate the journey length.
        journeyLength = Vector3.Distance(startMarker, endMarker);
        inMoveMode = true;
    }

    //Click and Click deployment Part 1
    //click and ...
    private void OnMouseDown()
    {
        if (!gameManager.InBattleModeAndNotDeploymentMode)
        {
            var selectedUnit = gameManager.selectedUnit;
            string selectedUnitSpriteName = string.Empty;
            string thisUnitSpriteName = string.Empty;
            //0. Make sure that the same unit has not been clicked twice. If so, then just deselect and exit
            if (selectedUnit != null)
            {
                selectedUnitSpriteName = selectedUnit.spriteName;
                thisUnitSpriteName = spriteName;
                if (selectedUnit.name == name)
                {
                    gameManager.Deselect();
                    return;
                }
            }
            else 
            {
                Select();
                return;
            }

            //1. BUMP first click shop unit, then click this deployed unit => try to deploy the selected unit
            // so the selected unit is not null, is NOT deployed, and THIS unit IS deployed, then try to deploy the selected unit
            if (selectedUnit != null && !selectedUnit.Deployed
                && Deployed
                && selectedUnitSpriteName != thisUnitSpriteName)
            {
                var goDpeloymentMarker = gameManager.deployment.listLeftDeploymentMarkers.Where(x => x.positionKey == this.QueuePosition).First().gameObject;
                var selectedunitStartPosition = gameManager.deployment.deploymentShopQueuePositions[gameManager.selectedUnit.QueuePosition];
                gameManager.deployment.TrySnapToDeploymentQueueSpace(gameManager.selectedUnit, goDpeloymentMarker, selectedunitStartPosition);
                gameManager.Deselect();
            }
            //2. SWAP first click deployed unit, then click this deployed unit => swap deployment queue positions
            else if (selectedUnit != null && selectedUnit.Deployed
                && Deployed
                && selectedUnitSpriteName != thisUnitSpriteName)
            {
                var pos1 = gameManager.deployment.listLeftDeploymentMarkers.Where(x => x.positionKey == gameManager.selectedUnit.QueuePosition && x.side == gameManager.playerSide).First();
                var pos2 = gameManager.deployment.listLeftDeploymentMarkers.Where(x => x.positionKey == this.QueuePosition && x.side == gameManager.playerSide).First();
                gameManager.selectedUnit.DeployAndSnapPositionToDeploymentMarker(pos2);
                this.DeployAndSnapPositionToDeploymentMarker(pos1);
                gameManager.Deselect();

            }
            //3. RANK UP first click shop unit, then click this deployed unit HAVING A SPRITE OF THE SAME NAME => rank up this unit
            //so the selected unit is not null, is NOT deployed, and THIS unit IS deployed AND has a sprite name the same as the selected unit, then try to deploy the selected unit
            else if (selectedUnit != null && !selectedUnit.Deployed 
                && Deployed
                && selectedUnitSpriteName == thisUnitSpriteName
                && CanAfford()
                && Rank < maxUnitRank)
            {
                RankUp(selectedUnit);
                gameManager.Deselect();
            }
            //4. both are deployed AND are the same type (RANK UP)
            else if (selectedUnit != null && selectedUnit.Deployed
                && Deployed
                && selectedUnitSpriteName == thisUnitSpriteName
                && Rank < maxUnitRank)
            {
                RankUp(selectedUnit);
                gameManager.Deselect();
            }
            else 
            {
                Select();
            }
        }
    }

    private void Select()
    {
        //first dselect any unit presently selected
        gameManager.Deselect();
        gameManager.selectedUnit = this;
        ShowSelectionIndicator(true);
    }

    public void DeployAndSnapPositionToDeploymentMarker(DeploymentMarker deploymentMarker)
    {
        //don't pay for a deployed unit twice
        if (!Deployed)
        {
            gameManager.deployment.gold -= Cost;
        }
        float pointPart = (float)Math.Abs(deploymentMarker.positionKey) / 10;
        float zVal = 1f - pointPart;
        transform.position = new Vector3(deploymentMarker.transform.position.x, deploymentMarker.transform.position.y, zVal);

        //Clear Old Deploy Marker Occupancy
        ClearOldDeployMarkerOccupancy();
        //Set New Deploy Marker Occupancy
        SetNewDeployMarkerOccupancy(deploymentMarker);
        Deployed = true;
        Freezed = false;
    }

    public void RankUp(Unit consumeUnit)
    {
        if (!consumeUnit.Deployed)
        {
            gameManager.deployment.gold -= consumeUnit.Cost;
            FindObjectsOfType<DeploymentMarker>().Where(x => x.positionKey == consumeUnit.QueuePosition ).First().ShowHoverIndicator(false);
        }
        gameManager.deployment.gold += CurrencyBumpBasedOnRank(consumeUnit.Rank);
        Destroy(consumeUnit.gameObject);
        HitPoints++;
        Attack++;
        Rank++;
    }

    public int CurrencyBumpBasedOnRank(int rank)
    {
        int currencyBumpBasedOnRank = (rank / 3);
        return currencyBumpBasedOnRank;
    }

    private void SetNewDeployMarkerOccupancy(DeploymentMarker deploymentMarker)
    {
        QueuePosition = deploymentMarker.positionKey;
        deploymentMarker.occupant = this;
        deploymentMarker.ShowHoverIndicator(false);
    }

    public void ClearOldDeployMarkerOccupancy()
    {
        var oldDeployMarkers = gameManager.deployment.listLeftDeploymentMarkers.Where(x => x.occupant != null && x.occupant.name == name);
        foreach (var oldDeployMarker in oldDeployMarkers)
        {
            oldDeployMarker.occupant = null;
        }
    }

    public bool CanAfford()
    {
        return (Cost <= gameManager.deployment.gold);
    }

    private void OnMouseEnter()
    {
        ShowHoverIndicator(true);
    }

    private void OnMouseExit()
    {
        ShowHoverIndicator(false);
    }

    private void ShowHoverIndicator(bool show)
    {
        mouseHoverOverIndicator.SetActive(show && !gameManager.InBattleModeAndNotDeploymentMode);
    }

    public void ShowSelectionIndicator(bool show)
    {
        selectedIndicator.SetActive(show);
    }
    public UnitDetail GetDetail()
    {
        return new UnitDetail()
        {
            Attack = Attack,
            HitPoints = HitPoints,
            Name = name,
            QueuePosition = QueuePosition,
            Rank = Rank,
            Side = side,
            SpriteName = spriteName
        };
    }
}