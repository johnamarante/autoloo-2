using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

    public int _attackBonus;
    public int AttackBonus
    {
        get { return _attackBonus; }
        set
        {
            _attackBonus = value;
            this?.OnAttackBonusChanged(_attackBonus);
        }
    }
    public Action<int> OnAttackBonusChanged;

    public int _hitPoints;
    public int HitPoints
    {
        get { return _hitPoints; }
        set
        {
            var delta = value - _hitPoints;
            _hitPoints = value;
            this?.OnHitPointsChanged(_hitPoints, delta);
        }
    }
    public Action<int,int> OnHitPointsChanged;

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
    public int MaxUnitRank = 9;
    public int SkirmisherUnlockRank = 99;
    public int GrenadierUnlockRank = 99;
    public string hitPointsFormula = "1";
    public string attackFormula = "1";
    public Action<int> OnRankChanged;
    public string side = "";
    public int tier = 1;
    public Font myFont;
    public int effectFrame = 0;
    public int _queuePosition = 0;
    public int QueuePosition
    {
        get {return _queuePosition;}
        set
        {
            _queuePosition = value;
            if (this?.OnQueuePositionChanged != null)
            {
                this?.OnQueuePositionChanged(_queuePosition);
            }
        }
    }
    public Action<int> OnQueuePositionChanged;

    public bool _deployed = false;
    public bool Deployed 
    {
        get { return _deployed; }
        set
        {
            _deployed = value;
            if (this?.OnDeployedChanged != null)
            {
                //the Deployed property is always true for an already deployed unit
                this?.OnDeployedChanged(_deployed);
            }
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
            if (this?.OnFreezedChanged != null)
            {
                this?.OnFreezedChanged(_freezed);
            }
        }
    }
    public Action<bool> OnFreezedChanged;
    public int cycle = 1;
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
    public SpriteRenderer svgsprite;
    public SpriteRenderer svgspritebackground;
    public Sprite Lsprite;
    public Sprite Rsprite;
    public Sprite Lspritebackground;
    public Sprite Rspritebackground;
    //for infantry fight effects, left/right is decided dynamically by fliping the sprite renderer x property
    public bool showEffect = false;
    public Sprite fightEffect1;
    public Sprite fightEffect2;
    public Sprite fightEffect3;
    public Sprite fightEffect4;
    //Stats display START
    public TextMeshPro textAttack = new();
    public TextMeshPro textHitPoints = new();
    public TextMeshPro textCost = new();
    public GameObject costComponent;
    public GameObject rankComponent;
    public GameObject freezeComponent;
    public SpriteRenderer effectsComponent;
    public SpriteRenderer squareComponent;
    public Vector3 effectPlacementLeft;
    public Vector3 effectPlacementRight;
    private SpriteRenderer spriteRank = new SpriteRenderer();
    //Stats display END
    private GameObject mouseHoverOverIndicator;
    private GameObject selectedIndicator;
    public AudioClip acAttackSFX;
    public bool canFormSquare = false;
    public bool isCavalry = false;
    public bool _squared = false;
    public bool Squared
    {
        get { return _squared; }
        set
        {
            _squared = value;
            if (this?.OnSquaredChanged != null)
            {
                this?.OnSquaredChanged(_squared);
            }
        }
    }
    public Action<bool> OnSquaredChanged;
    public AudioClip acFormSquare;
    public bool _IsSkirmisher = false;
    public bool isSkirmisher
    {
        get { return _IsSkirmisher; }
        set
        {
            _IsSkirmisher = value;
            if (this?.OnIsSkirmisherChanged != null)
            {
                this?.OnIsSkirmisherChanged(_IsSkirmisher);
            }
        }
    }
    public Action<bool> OnIsSkirmisherChanged;

    void Awake()
    {
        svgsprite = gameObject.transform.Find("svgsprite").GetComponent<SpriteRenderer>();
        svgspritebackground = gameObject.transform.Find("svgbackgroundsprite") != null ? gameObject.transform.Find("svgbackgroundsprite").GetComponent<SpriteRenderer>() : null;
        SetSprite();
        spriteName = GetSpriteName();
        var uniqueNameValue = Guid.NewGuid().ToString().Substring(0, 8);
        name = $"{spriteName.Split("_")[0]}_{spriteName.Split("_")[1]}_{uniqueNameValue}";
    }

    public string GetSpriteName()
    {
        //DO NOT use svgsprite.sprite.name for below, this causes errors when instntiating from rosters
        var spriteName = gameObject.transform.Find("svgsprite").GetComponent<SpriteRenderer>().sprite.name;
        return spriteName;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = (GameManager)FindObjectOfType(typeof(GameManager));
        SetUnitStatsDisplay();
        OnAttackChanged += (e) => textAttack.text = (Attack + AttackBonus).ToString();
        OnAttackBonusChanged += (e) => { if (AttackBonus > 0) { textAttack.fontStyle = FontStyles.Underline; } else { textAttack.fontStyle = FontStyles.Normal; } OnAttackChanged(Attack); };
        OnHitPointsChanged += (e, delta) => { textHitPoints.text = HitPoints.ToString(); };
        //formerly in OnHitPointsChanged:  gameManager.floatyNumber.SpawnFloatingNumber(delta, transform.position, (delta < 0));
        OnCostChanged += (e) => textCost.text = Cost.ToString();
        OnDeployedChanged += (e) => { costComponent.SetActive(!Deployed); rankComponent.SetActive(Deployed); };
        OnRankChanged += (e) => { ChangeRankIcon(); CheckUnlocksOnRankUp(); };
        OnFreezedChanged += (e) => freezeComponent.SetActive(Freezed);
        OnIsSkirmisherChanged += (e) => Debug.Log($"is Skirmisher: {isSkirmisher}");
        OnQueuePositionChanged += (e) => { ApplyQueuePositionChangeEffect(e); };
        rankComponent.SetActive(Deployed);
        CheckUnlocksOnStart();
        mouseHoverOverIndicator = transform.Find("hover_over_indicator").gameObject;
        selectedIndicator = transform.Find("selected_indicator").gameObject;
        effectsComponent = (SpriteRenderer)transform.GetComponentsInChildren(typeof(SpriteRenderer), true).Where(x => x.name == "svgeffectssprite").FirstOrDefault();
        if (!transform.GetComponent<Artillery>())
        {
            effectsComponent.flipX = (side == "left") ? false : true;
        }
        try
        {
            squareComponent = (SpriteRenderer)transform.GetComponentsInChildren(typeof(SpriteRenderer), true).Where(x => x.name == "svgsquaresprite").FirstOrDefault();
            if (squareComponent != null)
            {
                OnSquaredChanged += (e) => CalledWhenTheSquaredValueChanges();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    private void CheckUnlocksOnRankUp()
    {
        if (Rank == SkirmisherUnlockRank)
        {
            Debug.Log($"skirmisher attached to {name}");
            gameObject.GetComponent<Skirmish>().enabled = true;
        }
        if (Rank == GrenadierUnlockRank)
        {
            Debug.Log($"grenadier attached to {name}");
            gameObject.GetComponent<GrenadierAttack>().enabled = true;
            //gameObject.GetComponent<Grenadier>().enabled = true;
        }
    }

    private void CheckUnlocksOnStart()
    {
        if (Rank >= SkirmisherUnlockRank)
        {
            Debug.Log($"skirmisher attached to {name}");
            gameObject.GetComponent<Skirmish>().enabled = true;
        }
        if (Rank >= GrenadierUnlockRank)
        {
            Debug.Log($"grenadier attached to {name}");
            gameObject.GetComponent<GrenadierAttack>().enabled = true;
            //gameObject.GetComponent<Grenadier>().enabled = true;
        }
    }

    private void CalledWhenTheSquaredValueChanges()
    {
        squareComponent.enabled = Squared; 
        if (Squared) 
        { 
            gameManager.PlayTransientAudioClip(acFormSquare); 
            AttackBonus += 3; 
        } 
        else 
        { 
            AttackBonus -= 3; 
        }
    }

    private void SetSprite()
    {
        if (side == "left")
        {
            svgsprite.sprite = Lsprite;
            if (svgspritebackground != null)
            {
                svgspritebackground.sprite = Lspritebackground;
            }
        }
        else 
        {
            svgsprite.sprite = Rsprite;
            if (svgspritebackground != null)
            {
                svgspritebackground.sprite = Rspritebackground;
            }
        }
        svgsprite.transform.position = new Vector3(Int32.Parse(svgsprite.sprite.name.Split("_")[3]), Int32.Parse(svgsprite.sprite.name.Split("_")[4]), svgsprite.transform.position.z);
        if (svgspritebackground != null)
        {
            svgspritebackground.transform.position = new Vector3(Int32.Parse(svgsprite.sprite.name.Split("_")[3]), Int32.Parse(svgsprite.sprite.name.Split("_")[4]), svgspritebackground.transform.position.z);
        }
    }

    private void ChangeRankIcon()
    {
        if (gameManager.rankSprites.Length > Rank)
        {
            spriteRank.sprite = gameManager.rankSprites[Rank];
        }
    }

    private void ApplyQueuePositionChangeEffect(int queuePosition)
    {
        if (gameObject.GetComponent<Artillery>()) 
        { 
            gameObject.GetComponent<Artillery>().SetAttackByQueue(queuePosition); 
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
        { 
            Move();
        }
        if (showEffect)
        {
            ShowFightEffects();
        }
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

    public void SetIntoMotion(Vector3 destination)
    {
        startMarker = transform.position;
        endMarker = destination;
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
            string selectedUnitSpriteName = string.Empty; //if we ever need to improve performance, we should give units numeric IDs to compare
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
                var deploymentMarker = gameManager.deployment.listLeftDeploymentMarkers.Where(x => x.positionKey == this.QueuePosition).First();
                var selectedunitStartPosition = gameManager.deployment.deploymentShopQueuePositions[gameManager.selectedUnit.QueuePosition];
                gameManager.deployment.TrySnapToDeploymentQueueSpace(gameManager.selectedUnit, deploymentMarker, selectedunitStartPosition);
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
                && Rank < MaxUnitRank)
            {
                RankUp(selectedUnit);
                gameManager.Deselect();
            }
            //4. both are deployed AND are the same type (RANK UP)
            else if (selectedUnit != null && selectedUnit.Deployed
                && Deployed
                && selectedUnitSpriteName == thisUnitSpriteName
                && Rank < MaxUnitRank)
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
            gameManager.deployment.coin -= Cost;
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
        //do not allow the unit to "lose" veterancy by stacking a higher rank unit onto a lower rank unit
        if (Rank >= consumeUnit.Rank)
        {
            if (!consumeUnit.Deployed)
            {
                gameManager.deployment.coin -= consumeUnit.Cost;
                FindObjectsOfType<DeploymentMarker>().Where(x => x.positionKey == consumeUnit.QueuePosition ).First().ShowHoverIndicator(false);
            }
            gameManager.deployment.coin += CurrencyBumpBasedOnRank(consumeUnit.Rank);
            Destroy(consumeUnit.gameObject);
            HitPoints++;
            Attack++;
            gameManager.floatyNumber.SpawnFloatingString($"+1/1", Color.green, transform.position);
            Rank++;
        }
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
        return (Cost <= gameManager.deployment.coin);
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
            IsSkirmisher = isSkirmisher
        };
    }
    public void ShowFightEffects()
    {
        if (!isCavalry)
        {
            switch (effectFrame)
            {
                case 1:
                    ShowEffect1();
                    break;
                case 5:
                    ShowEffect2();
                    break;
            }
        }
        
        if (isCavalry)
        {
            switch (effectFrame)
            {
                case 1:
                    ShowEffect1();
                    break;
                case 5:
                    ShowEffect2();
                    break;
                case 8:
                    ShowEffect3();
                    break;
                case 10:
                    ShowEffect4();
                    break;
            }
        }

        effectFrame++;
        
        if (!isCavalry)
        {
            if (effectFrame > 20)
            {
                HideEffect();
            }
            else
            {
                //fade
                float effectRatio = (float)(20 - effectFrame) / 20;
                effectsComponent.color = new Color(effectsComponent.color.r, effectsComponent.color.g, effectsComponent.color.b, effectRatio);

            }
        }

        if (isCavalry)
        {
            if (effectFrame > 12)
            {
                HideEffect();
            }
        }
    }

    private void ShowEffect1()
    {
        effectsComponent.enabled = true;
        effectsComponent.sprite = fightEffect1;
        var uniteffectscomponentspritename = effectsComponent.sprite.name;
        effectsComponent.transform.position = gameObject.transform.position +  ((side == "left") ? effectPlacementLeft : effectPlacementRight);
    }

    private void ShowEffect2()
    {
        effectsComponent.sprite = fightEffect2;
    }

    private void ShowEffect3()
    {
        effectsComponent.sprite = fightEffect2;
    }

    private void ShowEffect4()
    {
        effectsComponent.sprite = fightEffect2;
    }

    private void HideEffect()
    {
        showEffect = false;
        effectsComponent.enabled = false;
        effectFrame = 0;
        effectsComponent.color = new Color(255, 255, 255, 255);
    }

    public int ComputeHitPointsFromFoumulaString(int baseUnitRank)
    {
        return ComputeSumFromFormulaString(hitPointsFormula, baseUnitRank);
    }

    public int ComputeAttackFromFoumulaString(int baseUnitRank)
    {
        return ComputeSumFromFormulaString(attackFormula, baseUnitRank);
    }

    private static int ComputeSumFromFormulaString(string formula, int baseUnitRank)
    {
        // Define a regex pattern to match different components of the formula
        string pattern = @"(?:PARENT RANK)|(\d+)|([+\-*/])";
        MatchCollection matches = Regex.Matches(formula, pattern);

        int sum = 0;
        int factor = 1; // To handle addition and subtraction operations

        foreach (Match match in matches)
        {
            if (match.Groups[1].Success) // Matches a number
            {
                int value = int.Parse(match.Groups[1].Value);
                sum += factor * value;
            }
            else if (match.Groups[2].Success) // Matches an operator
            {
                string op = match.Groups[2].Value;
                if (op == "+")
                    factor = 1;
                else if (op == "-")
                    factor = -1;
                else if (op == "*")
                    factor *= 1; // Multiplication does not change factor
                else if (op == "/")
                    factor /= 1; // Division does not change factor
            }
            else // Matches "PARENT RANK"
            {
                sum += factor * baseUnitRank;
            }
        }

        return sum;
    }

}