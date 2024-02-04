using System;
using System.Linq;
using UnityEngine;

public class Skirmish : MonoBehaviour
{
    public Unit unit;
    public Sprite lineMusket;
    public Sprite lightMusket;
    public GameObject goNotification;

    private class WeaponClickHandler : MonoBehaviour
    {
        private Unit _parentUnit;
        private GameObject _goHoverCorners;
        public Sprite _lineMusket;
        public Sprite _lightMusket;

        public void Initialize(Unit unit, Sprite lineMusket, Sprite lightMusket)
        {
            _parentUnit = unit;
            _lineMusket = lineMusket;
            _lightMusket = lightMusket;

            // Check if the parent unit is not null and inSkirmishMode is valid
            if (_parentUnit != null && (_parentUnit.SkirmishMode ? _lightMusket : _lineMusket) != null)
            {
                UpdateWeaponSprite(_parentUnit.SkirmishMode ? _lightMusket : _lineMusket);

                // Attempt to find "hover_over_indicator" transform
                Transform hoverIndicatorTransform = transform.Find("hover_over_indicator");

                // Assign _goHoverCorners only if the transform is found
                if (hoverIndicatorTransform != null)
                {
                    _goHoverCorners = hoverIndicatorTransform.gameObject;
                }
            }
        }

        private void OnMouseDown()
        {
            if (!_parentUnit.gameManager.InBattleModeAndNotDeploymentMode && _parentUnit.canFormSquare)
            {
                _parentUnit.SkirmishMode = !_parentUnit.SkirmishMode;
                UpdateWeaponSprite(_parentUnit.SkirmishMode ? _lightMusket : _lineMusket);
            }
        }

        private void UpdateWeaponSprite(Sprite sprite)
        {
            _parentUnit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
            SetSpriteRendererTransform(sprite);
        }

        private void SetSpriteRendererTransform(Sprite sprite)
        {
            Vector3 scale = sprite == _lineMusket ? new Vector3(0.5f, 1.5f, 1f) : new Vector3(0.5f, 0.5f, 1f);
            _parentUnit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().transform.localScale = scale;
        }

        private void OnMouseEnterExit(bool show)
        {
            if (_parentUnit.canFormSquare) ShowHoverIndicator(show);
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
        // By virtue of this class being attached, the unit should be in skirmish mode by default
        unit.SkirmishMode = true;
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
        if (unit.side == "left")
        {
            if (unit.gameManager.LeftQueueUnits.Count > 4)
            {
                Debug.Log("no space for skirmishers on the battlefield! boost attack...");
                unit.AttackBonus = 1;
            }
            else
            {
                var skirmisherPrefab = unit.gameManager.LeftUnitRoster.Where(x => x.GetSpriteName().Split('_')[1] == unit.GetSpriteName().Split('_')[1]).ToList()[0];
                var goSkirmisher = Instantiate(skirmisherPrefab);
                goSkirmisher.Deployed = true;
                foreach (var alliedUnit in unit.gameManager.LeftQueueUnits)
                {
                    alliedUnit.QueuePosition--;
                }
                goSkirmisher.QueuePosition = -1;
                unit.gameManager.LeftQueueUnits.Add(goSkirmisher);
                unit.gameManager.ArrangeUnitsOnBattlefield(ref unit.gameManager.LeftQueueUnits, unit.gameManager.fightQueuePositions);
            }
        }
    }
        
        //OR
        //need to generate a new unit and set it to deployed
        //put that unit in the leftqueue[0] space
        //reorder units (very quickly)
}