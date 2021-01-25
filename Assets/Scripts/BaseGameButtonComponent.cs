using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void OnGameButtonClickHandler();
public class BaseGameButtonComponent : MonoBehaviour
{
    [Header("Action Setting")]
    public String actionName;
    public int actionValue;    // min level is 1
    public float cooldownTimeSeconds;    // seconds

    [Header("UI Components")]
    public Text buttonTitleText;
    public Text cooldownCommentText;
    public Text cooldownFakeProgressText;    // fake progress_bar: 25 dots are used

    // private
    private Button _actionButton;
    private List<OnGameButtonClickHandler> _clickHandlers = new List<OnGameButtonClickHandler>();
    private float _currentCooldown = 0;
    private float _lastCooldown = 0;
    
    private void OnButtonClick()
    {
        foreach (var handler in _clickHandlers)
        {
            handler();
        }
    }

    public void RestartCooldown(float cooldown)
    {
        _actionButton.interactable = false;
        _currentCooldown = cooldown;
        _lastCooldown = cooldown;
        UpdateCooldownProgress(1.0f);
    }

    private void Update()
    {
        if (_actionButton.interactable) return;
        
        _currentCooldown -= Time.deltaTime;

        if (_currentCooldown <= 0)
        {
            UpdateCooldownProgress(0);
            _actionButton.interactable = true;
        }
        else
        {
            UpdateCooldownProgress(_currentCooldown / _lastCooldown);
        }
    }

    void OnEnable()
    {
        _actionButton = GetComponent<Button>();
        _actionButton.onClick.AddListener(OnButtonClick);
    }
    
    void OnDisable()
    {
        _actionButton.onClick.RemoveListener(OnButtonClick);
    }

    public void AddClickListener(OnGameButtonClickHandler listener)
    {
        _clickHandlers.Add(listener);
    }
    
    public void RemoveClickListener(OnGameButtonClickHandler listener)
    {
        _clickHandlers.Remove(listener);
    }

    public void OnValidate()
    {
        ValidateValues();
        SetupButtonUi();
    }

    private void ValidateValues()
    {
        if (actionValue < 1)
        {
            actionValue = 1;
        }

        if (cooldownTimeSeconds < 0.0f)
        {
            cooldownTimeSeconds = 0.0f;
        }
    }

    protected virtual void SetupButtonUi()
    {
        SetupButtonTitle();
        SetupCooldownUi();
    }

    protected virtual void SetupButtonTitle()
    {
        buttonTitleText.text = $"{actionName} +{actionValue}";
    }

    private void SetupCooldownUi()
    {
        cooldownCommentText.text = $"{cooldownTimeSeconds} sec";
        UpdateCooldownProgress(0.0f);
    }

    // progress in [0.0f, 1.0f]
    private void UpdateCooldownProgress(float progress)
    {
        const int maxDotsAmount = 25;
        String cdProgress = "";

        int dotsNumber = (int)Math.Round(maxDotsAmount * progress);
        for (int i = 0; i < dotsNumber; i++)
        {
            cdProgress += ".";
        }

        cooldownFakeProgressText.text = cdProgress;
    }
}
