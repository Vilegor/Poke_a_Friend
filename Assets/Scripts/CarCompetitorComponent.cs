using System;
using UnityEngine;
using TMPro;
using UnityEditorInternal;

public class CarCompetitorComponent : BaseCompetitorComponent
{
    [Header("UI Settings")]
    public SpriteRenderer carSpriteRenderer;
    public TextMeshPro levelText;
    
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
    
    private void ValidateSkinId()
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

    private void UpdateSkin()
    {
        String carSkinPath = $"Cars/car_{skinId}";
        Sprite newCarSprite = Resources.Load<Sprite>(carSkinPath);
        
        Debug.Log($"Car #{id}. Skin update: {carSkinPath})");
        
        carSpriteRenderer.sprite = newCarSprite;
    }
}