using System;
using UnityEngine.UI;

namespace Buttons
{
    public class KickButtonComponent : BaseGameButtonComponent
    {
        public Text topPlayerName;    // name of the top player to kick

        public void SetTopPlayerName(String playerName)
        {
            topPlayerName.text = playerName;
        }
    
        protected override void SetupButtonTitle()
        {
            buttonTitleText.text = $"{actionName} -{actionValue}";
        }
    }
}