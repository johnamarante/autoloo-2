using System;
using System.Linq;
using UnityEngine;

public class Skirmish : MonoBehaviour
{
    public Unit unit;
    public Unit skirmisherL1;
    public Unit skirmisherL2;
    public Unit skirmisherL3;
    public Sprite lineMusket;
    public Sprite lightMusket;
    public GameObject goNotification;

    private class WeaponClickHandler : MonoBehaviour
    {
        private Unit _unit;
        private GameObject _goHoverCorners;
        public Sprite _lineMusket;
        public Sprite _lightMusket;

        public void Initialize(Unit unit, Sprite lineMusket, Sprite lightMusket)
        {
            _unit = unit;
            _lineMusket = lineMusket;
            _lightMusket = lightMusket;

            UpdateWeaponSprite();
            Transform hoverIndicatorTransform = transform.Find("hover_over_indicator");
            if (hoverIndicatorTransform != null)
            {
                _goHoverCorners = hoverIndicatorTransform.gameObject;
            }
        }

        private void OnMouseDown()
        {
            if (!_unit.gameManager.InBattleModeAndNotDeploymentMode && _unit.canFormSquare)
            {
                _unit.SkirmishMode = !_unit.SkirmishMode;
                UpdateWeaponSprite();
            }
        }

        private void UpdateWeaponSprite()
        {
            Sprite sprite = _unit.SkirmishMode ? _lightMusket : _lineMusket;
            _unit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
            SetSpriteRendererTransform(sprite);
        }

        private void SetSpriteRendererTransform(Sprite sprite)
        {
            Vector3 scale = sprite == _lineMusket ? new Vector3(0.5f, 1.5f, 1f) : new Vector3(0.5f, 0.5f, 1f);
            _unit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().transform.localScale = scale;
        }

        private void OnMouseEnterExit(bool show)
        {
            if (_unit.canFormSquare) ShowHoverIndicator(show);
        }

        private void OnMouseEnter() => OnMouseEnterExit(true);

        private void OnMouseExit() => OnMouseEnterExit(false);

        public void ShowHoverIndicator(bool show)
        {
            _goHoverCorners.SetActive(show);
        }
    }

    private void Start()
    {
        unit = GetComponent<Unit>();
        Transform attackChild = transform.Find("Attack");
        if (attackChild != null)
        {
            // Attach a click event to the "Attack" child object and pass the Unit instance
            WeaponClickHandler clickHandler = attackChild.gameObject.AddComponent<WeaponClickHandler>();
            clickHandler.Initialize(unit, lineMusket, lightMusket);
        }
        if (unit.canFormSquare && unit.side == unit.gameManager.playerSide && !unit.Deployed) 
        {
            GameObject goNoti = Instantiate(goNotification, transform); //, attackChild.position + new Vector3(0f, 3f, -1f), new Quaternion(0, 0.71f, -0.71f, 0));
            goNoti.transform.position += new Vector3(3f, -2f, -1f);
            goNoti.transform.rotation = new Quaternion(0, 0.71f, -0.71f, 0);
            Destroy(goNoti, 5);
        }
    }

    public void DeploySkirmishers()
    {
        Debug.Log("deploying skirmishers...");
        if (unit.side == "left" && unit.cycle == 1)
        {
            if (unit.gameManager.LeftQueueUnits.Count > 4)
            {
                Debug.Log("no space for skirmishers on the battlefield! boost attack...");
                unit.AttackBonus = 1;
            }
            else
            {
                Unit skirmisherPrefab;
                float skimisherLevel = (float)(unit.Rank / 3);
                if (skimisherLevel > 2)
                {
                    skirmisherPrefab = skirmisherL3;
                }
                else if (skimisherLevel > 1)
                {
                    skirmisherPrefab = skirmisherL2;
                }
                else 
                {
                    skirmisherPrefab = skirmisherL1;
                }
                skirmisherPrefab.side = "left";
                var goSkirmisher = Instantiate(skirmisherPrefab);
                goSkirmisher.Deployed = true;
                goSkirmisher.SkirmishMode = true;
                foreach (var alliedUnit in unit.gameManager.LeftQueueUnits)
                {
                    alliedUnit.QueuePosition--;
                }
                goSkirmisher.name = $"{goSkirmisher.name}_skirmisher";
                goSkirmisher.QueuePosition = -1;
                goSkirmisher.transform.position = unit.transform.position;
                unit.gameManager.LeftQueueUnits.Add(goSkirmisher);
                unit.gameManager.LeftQueueUnits = unit.gameManager.LeftQueueUnits.OrderByDescending(u => u.QueuePosition).ToList();
                unit.gameManager.ArrangeUnitsOnBattlefield(ref unit.gameManager.LeftQueueUnits, unit.gameManager.fightQueuePositions);
                //increment the unit's cycle so that cycle can be checked to prevent continously deploying skirmishers again and again,
                ////and because deploing skirmishers is in fact a move on the part of this unit
                unit.cycle++;
            }
        }
        if (unit.side == "right" && unit.cycle == 1)
        {
            if (unit.gameManager.RightQueueUnits.Count > 4)
            {
                Debug.Log("no space for skirmishers on the battlefield! boost attack...");
                unit.AttackBonus = 1;
            }
            else
            {
                Unit skirmisherPrefab;
                float skimisherLevel = (unit.Rank / 3);
                if (skimisherLevel > 2)
                {
                    skirmisherPrefab = skirmisherL3;
                }
                else if (skimisherLevel > 1)
                {
                    skirmisherPrefab = skirmisherL2;
                }
                else
                {
                    skirmisherPrefab = skirmisherL1;
                }
                skirmisherPrefab.side = "right";
                var goSkirmisher = Instantiate(skirmisherPrefab);
                goSkirmisher.Deployed = true;
                goSkirmisher.SkirmishMode = true;
                foreach (var alliedUnit in unit.gameManager.RightQueueUnits)
                {
                    alliedUnit.QueuePosition++;
                }
                goSkirmisher.name = $"{goSkirmisher.name}_skirmisher";
                goSkirmisher.QueuePosition = 1;
                goSkirmisher.transform.position = unit.transform.position;
                unit.gameManager.RightQueueUnits.Add(goSkirmisher);
                unit.gameManager.RightQueueUnits = unit.gameManager.RightQueueUnits.OrderBy(u => u.QueuePosition).ToList();
                unit.gameManager.ArrangeUnitsOnBattlefield(ref unit.gameManager.RightQueueUnits, unit.gameManager.fightQueuePositions);
                //increment the unit's cycle so that cycle can be checked to prevent continously deploying skirmishers again and again,
                ////and because deploing skirmishers is in fact a move on the part of this unit
                unit.cycle++;
            }
        }
    }
}