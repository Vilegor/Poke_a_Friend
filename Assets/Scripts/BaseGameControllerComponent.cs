using System.Collections.Generic;
using UnityEngine;
using GameData;
using UnityEngine.UI;

public class BaseGameControllerComponent : MonoBehaviour
{
    [Header("Game UI Components")]
    public GameUiController gameUiController;
    public GameObject startGameUiContainer;
    public GameObject endGameUiContainer;

    public Button startGameButton;
    public Button replayGameButton;
    public Text playerWonText;
    public Text gameTimeText;

    [Header("Competitor Setting")]
    public List<BaseCompetitorComponent> competitorObjects;    // views

    [Header("Game Setting")]
    public int startLevel;    // min level is always 0. 0 means death
    public int maxLevel;
    
    public float maxYLength; // y position counts from 0

    public int startHealthPoints;
    
    [Header("- Bot Setting -")]
    public bool isBotGame;
    
    [Header("- Action Setting -")]
    public int boostActionValue;
    public float boostActionCooldown;
    public float turboActionCooldown;
    
    public int attackActionValue;    // TODO: separate values for HP and Level
    public float attackActionCooldown;

    [Header("- Action Modifications -")]
    public int eliminationBonusValue;    // TODO: maybe give bonus only for eliminating a leader "a King"
    public bool attackAlsoReducesLevel;
    
    // protected game data
    protected List<CompetitorModel> CompetitorModels;
    protected float CurrentGameTime;
    protected bool IsGameRunning;

    protected CompetitorModel MyPlayerModel;

    private void OnValidate()
    {
        gameUiController.SetupActionsUi(boostActionValue, boostActionCooldown, turboActionCooldown, attackActionValue, attackActionCooldown);
        
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
            competitor.currentHp = startHealthPoints;
            
            competitor.isLeader = (i == competitorsCount - 2);
            competitor.isPlayer = (i == competitorsCount - 1);
            competitor.currentLevel = (i == competitorsCount - 1) ? startLevel : (i == competitorsCount - 2) ? maxLevel : levelStep * i;
            
            competitor.OnValidate();
        }
    }

    private void OnBoostButtonClicked()
    {
        if(!gameUiController.boostButton.interactable) return;

        gameUiController.DisableUi(boostActionCooldown);
        MyPlayerModel.RequestPlayerAction(ActionType.Boost);
    }

    private void OnTurboBoostButtonClicked()
    {
        if(!gameUiController.turboButton.interactable) return;
        
        gameUiController.DisableUi(turboActionCooldown);
        MyPlayerModel.RequestPlayerAction(ActionType.TurboBoost);
    }
    
    private void OnAttackLeaderButtonClicked()
    {
        if(!gameUiController.attackLeaderButton.interactable) return;
        
        gameUiController.DisableUi(attackActionCooldown);
        MyPlayerModel.RequestPlayerAction(ActionType.AttackLeader);
    }
    
    private void OnAttackLastButtonClicked()
    {
        if(!gameUiController.attackLastButton.interactable) return;
        
        gameUiController.DisableUi(attackActionCooldown);
        MyPlayerModel.RequestPlayerAction(ActionType.AttackLast);
    }

    private void OnStartGameClicked()
    {
        StartGame();
    }

    private void InitCompetitorModels()
    {
        CompetitorModels = CreateCompetitorsList();

        for (var i = 0; i < competitorObjects.Count; i++)
        {
            var model = new CompetitorModel(i, competitorObjects[i]);
            CompetitorModels.Add(model);
        }
    }

    protected virtual List<CompetitorModel> CreateCompetitorsList()
    {
        return new List<CompetitorModel>();
    }

    #region Game Life Cycle

    void Start()
    {
        InitCompetitorModels();
        
        gameUiController.boostButton.onClick.AddListener(OnBoostButtonClicked);
        gameUiController.turboButton.onClick.AddListener(OnTurboBoostButtonClicked);
        gameUiController.attackLeaderButton.onClick.AddListener(OnAttackLeaderButtonClicked);
        gameUiController.attackLastButton.onClick.AddListener(OnAttackLastButtonClicked);
        
        startGameButton.onClick.AddListener(OnStartGameClicked);
        replayGameButton.onClick.AddListener(OnStartGameClicked);

        IsGameRunning = false;
        endGameUiContainer.SetActive(false);
        startGameUiContainer.SetActive(true);
    }
    
    private void StartGame()
    {
        //var playerIndex = Random.Range(0, CompetitorModels.Count - 1);
        ReshuffleCompetitors(isBotGame);
        
        foreach (var competitor in competitorObjects)
        {
            competitor.OnValidate();
        }
        
        gameUiController.SetActive(!isBotGame);
        startGameUiContainer.SetActive(false);
        endGameUiContainer.SetActive(false);

        CurrentGameTime = 0;
        IsGameRunning = true;
    }

    protected virtual void ReshuffleCompetitors(bool isBotsOnly, int desiredPlayerIndex = 0)
    {
        CompetitorModels.Sort((m1, m2) => m1.PlayerId - m2.PlayerId);

        for (var i = 0; i < CompetitorModels.Count; i++)
        {
            CompetitorModels[i].Reset(startLevel, startHealthPoints, i == desiredPlayerIndex);
        }

        MyPlayerModel = isBotsOnly ? null : CompetitorModels[desiredPlayerIndex];
    }

    void Update()
    {
        if (!IsGameRunning) return;

        // update game time
        var elapsed = Time.deltaTime;
        CurrentGameTime += elapsed;

        // prepare list of only active players
        var activePlayers = GetActivePlayers();
        
        // update My Player UI
        UpdateGameUi(activePlayers);
        
        // collect action requests from players
        var actionRequests = CollectActionRequests(activePlayers, elapsed);

        // perform actions in order
        ProcessRequestActions(actionRequests, activePlayers);
        
        // Update players leaderboard and define new Leader
        UpdateLeader();
        
        //Debug.Log($"Updated: Leader #{_competitorModels[0].PlayerId}, lvl {_competitorModels[0].CurrentLevel}");

        foreach (var view in competitorObjects)
        {
            view.OnValidate();
        }

        if (CompetitorModels[0].CurrentLevel < maxLevel) return;
        
        IsGameRunning = false;
        OnGameEnded(CompetitorModels[0], CurrentGameTime);
    }

    private List<CompetitorModel> GetActivePlayers()
    {
        var activeCompetitors = new List<CompetitorModel>();
        foreach (var comp in CompetitorModels)
        {
            if (!comp.IsEliminated)
            {
                activeCompetitors.Add(comp);
            }
        }

        return activeCompetitors;
    }
    private void UpdateGameUi(List<CompetitorModel> activePlayers)
    {
        if (MyPlayerModel == null) return;
        if (MyPlayerModel.IsEliminated) gameUiController.DisableUi();
        
        var leaderId = MyPlayerModel.IsLeader ? -1 : activePlayers[0].PlayerId;
        
        var last = activePlayers[activePlayers.Count - 1];
        var lastId = MyPlayerModel == last ? -1 : last.PlayerId;
        
        gameUiController.UpdateUi(MyPlayerModel.IsLeader, leaderId, lastId);
    }

    private List<ActionRequest> CollectActionRequests(List<CompetitorModel> activePlayers, float elapsedTime)
    {
        var actionRequests = new List<ActionRequest>();

        foreach (var player in activePlayers)
        {
            var availableActions = new List<ActionType>();
            if (player.PendingCooldown < elapsedTime)
            {
                availableActions.Add(player.IsLeader ? ActionType.TurboBoost : ActionType.Boost);
                if (!player.IsLeader)
                {
                    availableActions.Add(ActionType.AttackLeader);
                }
                if (player != activePlayers[activePlayers.Count - 1])
                {
                    availableActions.Add(ActionType.AttackLast);
                }
            }

            var request = player.Update(elapsedTime, availableActions, activePlayers);
            if (request.Type != ActionType.None)
            {
                actionRequests.Add(request);
            }
        }

        return actionRequests;
    }

    private void ProcessRequestActions(List<ActionRequest> actionRequests, List<CompetitorModel> activePlayers)
    {
        // sort all action request by Time
        actionRequests.Sort((r1, r2) => r2.CompareTo(r1));
        
        foreach (var request in actionRequests)
        {
            Debug.Log($"Bot #{request.PlayerId} Action = {request.Type}");
            switch (request.Type)
            {
                case ActionType.Boost:
                    GetCompetiroById(request.PlayerId).PerformBoostAction(boostActionValue, boostActionCooldown);
                    break;
                case ActionType.TurboBoost:
                    GetCompetiroById(request.PlayerId).PerformBoostAction(boostActionValue, turboActionCooldown);
                    break;
                case ActionType.AttackLeader:
                    var target1 = activePlayers[0];
                    
                    target1.ApplyAttackAction(attackActionValue, attackAlsoReducesLevel);
                    GetCompetiroById(request.PlayerId).PerformAttackAction(attackActionCooldown);
                    
                    if (target1.IsEliminated && eliminationBonusValue > 0)
                    {
                        GetCompetiroById(request.PlayerId).PerformBoostAction(eliminationBonusValue, attackActionCooldown);
                    }
                    break;
                case ActionType.AttackLast:
                    var target8 = activePlayers[activePlayers.Count - 1];
                    
                    target8.ApplyAttackAction(attackActionValue, attackAlsoReducesLevel);
                    GetCompetiroById(request.PlayerId).PerformAttackAction(attackActionCooldown);
                    
                    if (target8.IsEliminated && eliminationBonusValue > 0)
                    {
                        GetCompetiroById(request.PlayerId).PerformBoostAction(eliminationBonusValue, attackActionCooldown);
                    }
                    break;
                case ActionType.None:
                    Debug.LogError("NONE Action!");    // ERROR!
                    break;
            }
        }
    }

    private void UpdateLeader()
    {
        // update leaderbord. [0] is a leader
        
        CompetitorModels.Sort((c1, c2) => c2.CompareTo(c1));
        for (var i = 0; i < CompetitorModels.Count; i++)
        {
            CompetitorModels[i].IsLeader = (i == 0);
        }
    }

    private CompetitorModel GetCompetiroById(int playerId)
    {
        foreach (var model in CompetitorModels)
        {
            if (model.PlayerId == playerId) return model;
        }

        return null;
    }
    
    private void OnGameEnded(CompetitorModel winnerModel, float gameTime)
    {
        if (winnerModel.IsPlayer)
        {
            playerWonText.text = "YOU WON!";
        }
        else
        {
            playerWonText.text = $"Player #{winnerModel.PlayerId + 1} WON!";
        }

        gameTimeText.text = $"in {gameTime} seconds";
        endGameUiContainer.SetActive(true);
    }

    #endregion
}
