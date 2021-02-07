using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUiController : MonoBehaviour
{
    [Header("Boost Settings")]
    public Button boostButton;
    public Text boostTitleText;
    public Text boostCooldownText;

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
    
    [Header("Display Setup")]
    public int testCooldownProgress;

    public bool IsTurbo { get; private set; }

    private float _panelWidth;
    private bool _isBlockedWithCooldown = false;
    private float _totalCooldownTime = 0;
    private float _cooldownTimeLeft = 0;

    private void OnValidate()
    {
        UpdateBoostUi(1, 1.0f, false);
        UpdateAttackUi(1, 0.5f, 1, 8);

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

    public void UpdateActionsUi(int boostTurboValue, float boostTurboCooldown, bool isTurbo, int attackValue, float attackCooldown, int leadPlayerId, int lastPlayerId)
    {
        UpdateBoostUi(boostTurboValue, boostTurboCooldown, isTurbo);
        UpdateAttackUi(attackValue, attackCooldown, leadPlayerId, lastPlayerId);
    }

    private void UpdateBoostUi(int value, float cooldown, bool turbo)
    {
        IsTurbo = turbo;
        boostTitleText.text = turbo ? $"Turbo +{value}" : $"Boost +{value}";
        boostCooldownText.text = $"{cooldown}s";

        // change color
        Color greenBoostColor = new Color(0.0208f, 0.5f, 0.0f);
        Color orangeTurboColor = new Color(0.988f, 0.616f, 0.012f);
        Color greyCooldownColor = new Color(0.639f, 0.642f, 0.638f);

        boostTitleText.color = turbo ? orangeTurboColor : greenBoostColor;
        boostCooldownText.color = turbo ? orangeTurboColor : greyCooldownColor;
    }

    private void UpdateAttackUi(int value, float cooldown, int leadPlayerId, int lastPlayerId)
    {
        attackLeaderTitleText.text = $"Lead -{value}";
        attackLastTitleText.text = $"Last -{value}";
        
        attackLeaderCooldownText.text = $"{cooldown}s";
        attackLastCooldownText.text = $"{cooldown}s";

        leaderNameText.text = $"#{leadPlayerId}";
        lastNameText.text = $"#{lastPlayerId}";
    }

    // Start is called before the first frame update
    void Start()
    {
        RectTransform panelRect = uiPanel.GetComponent<RectTransform>();
        _panelWidth = panelRect.sizeDelta.x;
    }

    public void SetActive(bool active)
    {
        uiPanel.SetActive(active);
    }
    
    public void StartBlockUi(float cooldownTime)
    {
        boostButton.interactable = false;
        attackLeaderButton.interactable = false;
        attackLastButton.interactable = false;
        
        _cooldownTimeLeft = _totalCooldownTime = cooldownTime;
        _isBlockedWithCooldown = true;
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
