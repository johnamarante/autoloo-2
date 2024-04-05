using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Skirmish : MonoBehaviour
{
    public Unit unit;
    public Unit skirmisherPrefab;
    public Sprite lineMusket;
    public Sprite lightMusket;
    //public GameObject goNotification;

    private class WeaponSpriteHandler : MonoBehaviour
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
        }

        private void UpdateWeaponSprite()
        {
            Sprite sprite = _unit.isSkirmisher ? _lightMusket : _lineMusket;
            _unit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
            SetSpriteRendererTransform(sprite);
        }

        private void SetSpriteRendererTransform(Sprite sprite)
        {
            Vector3 scale = sprite == _lineMusket ? new Vector3(0.5f, 1.5f, 1f) : new Vector3(0.5f, 0.5f, 1f);
            _unit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().transform.localScale = scale;
        }
    }

    private void Start()
    {
        unit = GetComponent<Unit>();
        Transform attackChild = transform.Find("Attack");
        if (attackChild != null)
        {
            // Attach a click event to the "Attack" child object and pass the Unit instance
            WeaponSpriteHandler clickHandler = attackChild.gameObject.AddComponent<WeaponSpriteHandler>();
            clickHandler.Initialize(unit, lineMusket, lightMusket);
        }
        //if (unit.canFormSquare && unit.side == unit.gameManager.playerSide && !unit.Deployed) 
        //{
        //    GameObject goNoti = Instantiate(goNotification, transform); //, attackChild.position + new Vector3(0f, 3f, -1f), new Quaternion(0, 0.71f, -0.71f, 0));
        //    goNoti.transform.position += new Vector3(3f, -2f, -1f);
        //    goNoti.transform.rotation = new Quaternion(0, 0.71f, -0.71f, 0);
        //    Destroy(goNoti, 5);
        //}
    }

    public void DeploySkirmishers(string side, List<Unit> unitQueue)
    {
        Debug.Log("deploying skirmishers...");
        if (unit.cycle == 1 && skirmisherPrefab != null)
        {
            if (unitQueue.Count > 4)
            {
                Debug.Log("no space for skirmishers on the battlefield! boost attack...");
                unit.AttackBonus = 1;
            }
            else
            {
                skirmisherPrefab.side = side;
                skirmisherPrefab._hitPoints = skirmisherPrefab.ComputeHitPointsFromFoumulaString(unit.Rank);
                skirmisherPrefab._attack = skirmisherPrefab.ComputeAttackFromFoumulaString(unit.Rank);
                var goSkirmisher = Instantiate(skirmisherPrefab);
                goSkirmisher.Deployed = true;
                goSkirmisher.isSkirmisher = true;
                foreach (var alliedUnit in unitQueue)
                {
                    if (side == "left")
                    { 
                        alliedUnit.QueuePosition--;
                        goSkirmisher.QueuePosition = -1;
                    }
                    if (side == "right")
                    { 
                        alliedUnit.QueuePosition++;
                        goSkirmisher.QueuePosition = 1;
                    }
                }
                goSkirmisher.name = $"{goSkirmisher.name}_skirmisher";
                goSkirmisher.transform.position = unit.transform.position;
                unitQueue = unitQueue.OrderByDescending(u => u.QueuePosition).ToList();
                if (side == "left")
                {
                    unit.gameManager.LeftQueueUnits.Add(goSkirmisher);
                    unit.gameManager.LeftQueueUnits = unit.gameManager.LeftQueueUnits.OrderByDescending(u => u.QueuePosition).ToList();
                    unit.gameManager.ArrangeUnitsOnBattlefield(ref unit.gameManager.LeftQueueUnits, unit.gameManager.fightQueuePositions);
                }
                if (side == "right")
                {
                    unit.gameManager.RightQueueUnits.Add(goSkirmisher);
                    unit.gameManager.RightQueueUnits = unit.gameManager.RightQueueUnits.OrderBy(u => u.QueuePosition).ToList();
                    unit.gameManager.ArrangeUnitsOnBattlefield(ref unit.gameManager.RightQueueUnits, unit.gameManager.fightQueuePositions);
                }
                //increment the unit's cycle so that cycle can be checked to prevent continously deploying skirmishers again and again,
                ////and because deploing skirmishers is in fact a move on the part of this unit
                unit.cycle++;
            }
        }
    }

    private void OnEnable()
    {

        transform.Find("skirmisher_Badge").gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        transform.Find("skirmisher_Badge").gameObject.SetActive(false);
    }
}