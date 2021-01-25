using System;
using UnityEngine;

namespace Buttons
{
    public class BoostButtonComponent : BaseGameButtonComponent
    {
        [Header("Turbo Settings")]
        public float turboCooldownTimeSeconds;    // seconds
        public bool isTurbo;
        
        public override void OnValidate()
        {
            base.OnValidate();
            
            SetTurbo(isTurbo);
        }

        public void SetTurbo(bool turbo)
        {
            isTurbo = turbo;
            cooldownCommentText.color = isTurbo ? new Color(0.988f, 0.616f, 0.012f) : new Color(0.639f, 0.642f, 0.638f);
            cooldownCommentText.text = isTurbo ? $"{turboCooldownTimeSeconds} sec" : $"{cooldownTimeSeconds} sec";
        }
    }
}