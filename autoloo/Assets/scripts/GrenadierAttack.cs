using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrenadierAttack : MonoBehaviour
{
    public Unit unit;
    public Unit grenadierL1;
    public Unit grenadierL2;
    public Unit grenadierL3;
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

    public void DeployGrenadiers()
    {
        Debug.Log("deploying grenadiers...");        
    }
}