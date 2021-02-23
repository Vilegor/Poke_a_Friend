using System;
using UnityEngine;

namespace CarGame
{
    public class CarCompetitorComponent : BaseCompetitorComponent
    {
        [Header("Car Settings")]
        public SpriteRenderer carSpriteRenderer;
        public int skinId;

        protected override void ForceSetLevel(int newLevel)
        {
            levelText.text = newLevel.ToString();
            base.ForceSetLevel(newLevel);
        }
    
        public override void OnValidate()
        {
            base.OnValidate();
            ValidateSkinId();
            UpdateSkin();
        }

        public void ValidateSkinId()
        {
            if (skinId < 1)
            {
                skinId = 1;
            }

            if (skinId > 15)
            {
                skinId = 15;
            }
        }

        public void UpdateSkin()
        {
            String carSkinPath = $"Cars/car_{skinId}";
            Sprite newCarSprite = Resources.Load<Sprite>(carSkinPath);

            carSpriteRenderer.sprite = newCarSprite;
        }
    }
}