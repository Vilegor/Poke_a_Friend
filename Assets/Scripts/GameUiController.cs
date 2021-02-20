using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUiController : MonoBehaviour
{
    [Header("Boost Settings")]
    public GameObject boostButtonObject;
    public Button boostButton;
    public Text boostTitleText;
    public Text boostCooldownText;
    
    [Header("Turbo Settings")]
    public GameObject turboButtonObject;
    public Button turboButton;
    public Text turboTitleText;
    public Text turboCooldownText;

    [Header("Attack Leader Settings")]
    public Button attackLeaderButton;
    public Text attackLeaderTitleText;
    public Text attackLeaderCooldownText;
    public Text leaderNameText;
    
    [Header("Attack Last Settings")]
    public Button attackLastButton;
    public Text attackLastTitleText;
    public Text attackLastCooldownText;
    public Text lastNameText;

    [Header("Cooldown Settings")]
    public GameObject uiPanel;
    public Image cooldownBarImage;
    
    [Header("Test Display")]
    public int testCooldownProgress;
    public bool testTurbo;

    private float _panelWidth;
    private bool _isBlockedWithCooldown = false;
    private float _totalCooldownTime = 0;
    private float _cooldownTimeLeft = 0;

    private void OnValidate()
    {
        SetupActionsUi(1, 1.0f, 0.5f, 1, 0.5f);
        UpdateUi(testTurbo, 1, 8);

        if (testCooldownProgress < 0)
        {
            testCooldownProgress = 0;
        }
        else if (testCooldownProgress > 100)
        {
            testCooldownProgress = 100;
        }
        
        _panelWidth = 750.0f;
        SetCooldownProgress(testCooldownProgress / 100.0f);
    }

    public void SetupActionsUi(int boostTurboValue, float boostCooldown, float turboCooldown, int attackValue, float attackCooldown)
    {
        SetupBoostUi(boostTurboValue, boostCooldown, turboCooldown);
        SetupAttackUi(attackValue, attackCooldown);
    }

    private void SetupBoostUi(int value, float boostCooldown, float turboCooldown)
    {
        boostTitleText.text = $"Boost +{value}";
        boostCooldownText.text = $"{boostCooldown}s";

        turboTitleText.text = $"Turbo +{value}";
        turboCooldownText.text = $"{turboCooldown}s";
    }

    private void SetupAttackUi(int value, float cooldown)
    {
        attackLeaderTitleText.text = $"Lead -{value}";
        attackLastTitleText.text = $"Last -{value}";
        
        attackLeaderCooldownText.text = $"{cooldown}s";
        attackLastCooldownText.text = $"{cooldown}s";

        leaderNameText.text = "?";
        lastNameText.text = "?";
    }

    public void UpdateUi(bool isTurboAvailable, int leadPlayerId, int lastPlayerId)
    {
        boostButtonObject.SetActive(!isTurboAvailable);
        turboButtonObject.SetActive(isTurboAvailable);
        
        attackLeaderButton.interactable = leadPlayerId >= 0 && !_isBlockedWithCooldown;
        attackLastButton.interactable = lastPlayerId >= 0 && !_isBlockedWithCooldown;
        
        leaderNameText.text = leadPlayerId >= 0 ? $"#{leadPlayerId}" : "YOU";
        lastNameText.text = lastPlayerId >= 0 ? $"#{lastPlayerId}" : "YOU";
    }

    // Start is called before the first frame update
    void Start()
    {
        RectTransform panelRect = uiPanel.GetComponent<RectTransform>();
        _panelWidth = panelRect.sizeDelta.x;
        
        SetCooldownProgress(0);
    }

    public void SetActive(bool active)
    {
        uiPanel.SetActive(active);
    }
    
    public void DisableUi(float cooldownTime = 0)
    {
        boostButton.interactable = false;
        turboButton.interactable = false;
        attackLeaderButton.interactable = false;
        attackLastButton.interactable = false;
        
        _cooldownTimeLeft = _totalCooldownTime = cooldownTime;
        _isBlockedWithCooldown = cooldownTime > 0;
    }

    private void SetCooldownProgress(float progress)
    {
        var imageRect = cooldownBarImage.rectTransform;
        imageRect.sizeDelta = new Vector2(_panelWidth * progress, imageRect.sizeDelta.y);
        
        // change color
        cooldownBarImage.color = Color.HSVToRGB(0.33f * (1 - progress), 0.65f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isBlockedWithCooldown) return;

        _cooldownTimeLeft -= Time.deltaTime;

        if (_cooldownTimeLeft <= 0)
        {
            SetCooldownProgress(0);
            
            boostButton.interactable = true;
            turboButton.interactable = true;
            attackLeaderButton.interactable = true;
            attackLastButton.interactable = true;
            
            _isBlockedWithCooldown = false;
        }
        else
        {
            SetCooldownProgress(_cooldownTimeLeft / _totalCooldownTime);
        }
    }

    
}
