using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Buttons;
using GameData;
using UnityEngine.UI;

public class GameControllerComponent : MonoBehaviour
{
    [Header("Game UI Components")]
    public GameObject gameUiContainer;
    public GameObject startGameUiContainer;
    public GameObject endGameUiContainer;

    public Button startGameButton;
    public Button replayGameButton;
    public Text playerWonText;
    public BoostButtonComponent boostButton;
    public KickButtonComponent kickButton;

    [Header("Competitor Setting")]
    public List<BaseCompetitorComponent> competitorObjects;    // views

    public int maxSkinId;
    
    [Header("Game Setting")]
    public int startLevel;    // min level is always 0. 0 means death
    public int maxLevel;
    
    public float maxYLength; // y position counts from 0
    
    public int boostActionValue;
    public float boostActionCooldown;
    public float turboActionCooldown;
    public int kickActionValue;
    public float kickActionCooldown;
    
    // private game data
    private List<CompetitorModel> _competitorModels;
    private float _currentGameTime;
    private bool _isGameRunning;

    private void OnValidate()
    {
        boostButton.actionValue = boostActionValue;
        boostButton.cooldownTimeSeconds = boostActionCooldown;
        boostButton.turboCooldownTimeSeconds = turboActionCooldown;
        boostButton.OnValidate();
        
        kickButton.actionValue = kickActionValue;
        kickButton.cooldownTimeSeconds = kickActionCooldown;
        kickButton.OnValidate();

        if (startLevel < 1)
        {
            startLevel = 1;
        }
        if (maxLevel <= startLevel)
        {
            maxLevel = startLevel + 1;
        }

        int competitorsCount = competitorObjects.Count;
        int levelStep = maxLevel / (competitorsCount - 2);
        for (var i = 0; i < competitorsCount; i++)
        {
            var competitor = competitorObjects[i];
            
            competitor.id = i;
            competitor.maxLevel = maxLevel;
            competitor.maxYLength = maxYLength;
            
            competitor.isWinner = (i == competitorsCount - 2);
            
            if (i == competitorsCount - 1)
            {
                competitor.currentLevel = startLevel;
                competitor.isPlayer = true;
            }
            else
            {
                competitor.currentLevel = levelStep * i;
            }
            competitor.OnValidate();
        }
    }

    private void OnBoostButtonClicked()
    {
        float cooldown = boostButton.isTurbo ? turboActionCooldown : boostActionCooldown;
        boostButton.RestartCooldown(cooldown);
        kickButton.RestartCooldown(cooldown);
    }
    
    private void OnKickButtonClicked()
    {
        kickButton.RestartCooldown(kickActionCooldown);
        boostButton.RestartCooldown(kickActionCooldown);
    }

    private void OnStartGameClicked()
    {
        StartGame();
    }

    private void InitCompetitorModels()
    {
        _competitorModels = new List<CompetitorModel>();

        for (var i = 0; i < competitorObjects.Count; i++)
        {
            var model = new CompetitorModel(i, competitorObjects[i]);
            _competitorModels.Add(model);
        }
    }

    #region Game Life Cycle

    void Start()
    {
        InitCompetitorModels();
        boostButton.AddClickListener(OnBoostButtonClicked);
        kickButton.AddClickListener(OnKickButtonClicked);
        
        startGameButton.onClick.AddListener(OnStartGameClicked);
        replayGameButton.onClick.AddListener(OnStartGameClicked);

        _isGameRunning = false;
        endGameUiContainer.SetActive(false);
        startGameUiContainer.SetActive(true);
    }
    
    private void StartGame()
    {
        ReshuffleCompetitors();
        
        foreach (var competitor in competitorObjects)
        {
            competitor.OnValidate();
        }
        
        startGameUiContainer.SetActive(false);
        endGameUiContainer.SetActive(false);

        _currentGameTime = 0;
        _isGameRunning = true;
    }

    private void ReshuffleCompetitors(int playerSkinId = 0)
    {
        var skinPool = Enumerable.Range(1, maxSkinId).OrderBy(x => Random.value).ToList();
        var playerIndex = Random.Range(0, _competitorModels.Count - 1);
        
        if (playerSkinId != 0)
        {
            skinPool.Remove(playerSkinId);
            skinPool.Insert(playerIndex, playerSkinId);
        }

        for (var i = 0; i < _competitorModels.Count; i++)
        {
            var skinId = skinPool[i];
            _competitorModels[i].Reset(startLevel, skinId, i == playerIndex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isGameRunning) return;
        
        _currentGameTime += Time.deltaTime;

        if (_currentGameTime >= 3)
        {
            _isGameRunning = false;
            OnGameEnded(_competitorModels[0]);
        }
    }
    
    private void OnGameEnded(CompetitorModel winnerModel)
    {
        if (winnerModel.View.isPlayer)
        {
            playerWonText.text = "YOU WON!";
        }
        else
        {
            playerWonText.text = $"Player #{winnerModel.View.id} WON!";
        }
        endGameUiContainer.SetActive(true);
    }

    #endregion
}
