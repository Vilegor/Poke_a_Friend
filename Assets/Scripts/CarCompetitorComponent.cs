using System;
using UnityEngine;
using TMPro;

public class CarCompetitorComponent : BaseCompetitorComponent
{
    [Header("UI Settings")]
    public SpriteRenderer carSpriteRenderer;
    public TextMeshPro levelText;

    protected override void ForceSetLevel(int newLevel)
    {
        levelText.text = newLevel.ToString();
        base.ForceSetLevel(newLevel);
    }

    protected override void ValidateSkinId()
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

    protected override void UpdateSkin()
    {
        String carSkinPath = $"Cars/car_{skinId}";
        Sprite newCarSprite = Resources.Load<Sprite>(carSkinPath);

        carSpriteRenderer.sprite = newCarSprite;
    }
}