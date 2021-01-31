using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    public class CompetitorModel
    {
        private int _id;
        private BaseCompetitorComponent _view;

        public int PlayerId => _view.id;
        public int CurrentLevel => _view.currentLevel;
        public int SkinId => _view.skinId;
        public bool IsPlayer => _view.isPlayer;

        public float PendingCooldown => _pendingCooldown;

        public bool IsLeader
        {
            get => _view.isLeader;
            set => _view.isLeader = value;
        }
        
        // current game state
        private float _pendingCooldown = 0;
        
        // bot settings
        private float _botActionDelay = 0;
        
        // player settings
        private ActionType _pendingActionType;

        public CompetitorModel(int id, BaseCompetitorComponent view)
        {
            _id = id;
            _view = view;
        }

        public int CompareTo(CompetitorModel c)
        {
            if (CurrentLevel == c.CurrentLevel)
            {
                if (PendingCooldown < c.PendingCooldown)
                {
                    return 1;
                }
                if (PendingCooldown > c.PendingCooldown)
                {
                    return -1;
                }

                return 0;
            }

            return CurrentLevel - c.CurrentLevel;
        }

        public void Reset(int startLevel, int skinId, bool isPlayer = false)
        {
            _view.currentLevel = startLevel;
            _view.skinId = skinId;
            _view.isPlayer = isPlayer;
            _view.isLeader = false;
            _botActionDelay = Random.Range(0.0f, 0.15f);
            _pendingActionType = ActionType.None;
            _pendingCooldown = 0.0f;
        }

        public void PerformBoostAction(int value, float cooldown)
        {
            _view.currentLevel += value;
            _pendingCooldown = cooldown;
        }

        public void PerformKickDownAction(float cooldown)
        {
            _pendingCooldown = cooldown;
        }

        public void ApplyKickDownAction(int value)
        {
            _view.currentLevel -= value;
        }

        // UPDATE cycle

        public ActionRequest Update(float timeElapsed, List<ActionType> availableActions)
        {
            return IsPlayer ? PlayerUpdate(timeElapsed, availableActions) : BotUpdate(timeElapsed, availableActions);
        }
        
        private ActionRequest BotUpdate(float timeElapsed, List<ActionType> availableActions)
        {
            if (_pendingCooldown > _botActionDelay * -1.0f)
            {
                _pendingCooldown -= timeElapsed;
            }

            var action = ActionType.None;
            if (_botActionDelay + _pendingCooldown <= 0)
            {
                action = availableActions[0];    // should be boost only in this case
            }
            
            return new ActionRequest(PlayerId, action, _pendingCooldown);
        }

        private ActionRequest PlayerUpdate(float timeElapsed, List<ActionType> availableActions)
        {
            if (_pendingCooldown > 0)
            {
                _pendingCooldown -= timeElapsed;
            }

            if (_pendingCooldown > 0 || !availableActions.Contains(_pendingActionType)) return new ActionRequest(PlayerId, ActionType.None);
            
            var action = _pendingActionType;
            _pendingActionType = ActionType.None;
            return new ActionRequest(PlayerId, action, _pendingCooldown);
        }
    }
}