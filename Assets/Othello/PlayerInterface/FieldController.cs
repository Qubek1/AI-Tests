using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Othello
{
    public class FieldController : MonoBehaviour
    {
        public int x, y;
        public UIGameController gameController;
        public TextMeshProUGUI text;

        public void OnClick()
        {
            gameController.OnClick(x, y);
        }
    }
}