using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarGame
{
    public class CarGameControllerComponent : BaseGameControllerComponent
    {
        [Header("Car Settings")]
        public int maxSkinId;

        public int testPlayerSkinId;

        private List<CarCompetitorModel> _carCompetitorModels;
        
        protected override List<CompetitorModel> CreateCompetitorsList()
        {
            _carCompetitorModels = new List<CarCompetitorModel>();
            return _carCompetitorModels.Cast<CompetitorModel>().ToList();
        }
        protected override void ReshuffleCompetitors(bool isBotsOnly, int desiredPlayerIndex = 0)
        {
            base.ReshuffleCompetitors(isBotsOnly, desiredPlayerIndex);
            
            var skinPool = Enumerable.Range(1, maxSkinId).OrderBy(x => Random.value).ToList();
        
            if (!isBotsOnly)
            {
                skinPool.Remove(testPlayerSkinId);
                skinPool.Insert(desiredPlayerIndex, testPlayerSkinId);
            }

            for (var i = 0; i < _carCompetitorModels.Count; i++)
            {
                _carCompetitorModels[i].SkinId = skinPool[i];
            }
        }
    }
}