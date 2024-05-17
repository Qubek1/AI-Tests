using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Othello
{
    public class UIGameController : MonoBehaviour
    {
        public OthelloGameModel gameModel;
        public int playerColor = 1;
        public GameObject buttonPrefab;
        public GameObject pawnPrefab;
        public Transform layoutGroup;
        public Transform pawnsGroup;
        public Color blackColor;
        public Color whiteColor;
        public Text text;
        public OthelloAI othelloAI;

        public GameObject[,] buttons;
        public GameObject[,] pawns;
        public TextMeshProUGUI[,] texts;

        public string annFileName;
        public Gradient textGradient;

        public void OnGameUpdate()
        {
            ANN ann = new ANN(annFileName);
            float[] inputLayer = new float[128];
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {

                    inputLayer[x + y * 8] = gameModel.boardState[x, y] == gameModel.currentPlayersTurn ? 1 : -1;
                    inputLayer[64 + x + y * 8] =
                        (gameModel.boardState[x, y] != gameModel.currentPlayersTurn && gameModel.boardState[x, y] != 0) ? 1 : -1;
                }
            }
            float[] answer = ann.NewCalculateWithDerivatives(inputLayer);
            ann.Save(annFileName);

            text.text = "Score: " + gameModel.score.ToString();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    texts[x, y].text = answer[x + y * 8].ToString("N2");
                    texts[x, y].color = textGradient.Evaluate((answer[x + y * 8] + 1) / 2);
                    if (gameModel.boardState[x, y] == 0)
                    {
                        if (pawns[x, y] != null)
                        {
                            Destroy(pawns[x, y]);
                        }
                        continue;
                    }
                    if (pawns[x, y] == null)
                    {
                        GameObject newPawn = Instantiate(pawnPrefab, buttons[x, y].transform);
                        pawns[x, y] = newPawn;
                    }
                    if (gameModel.boardState[x, y] == 2)
                    {
                        pawns[x, y].GetComponent<Image>().color = blackColor;
                    }
                    else
                    {
                        pawns[x, y].GetComponent<Image>().color = whiteColor;
                    }
                }
            }
        }

        public void OnClick(int x, int y)
        {
            //if (gameModel.currentPlayersTurn != playerColor)
            //{
            //    return;
            //}
            OthelloMove move = new OthelloMove();
            move.position = new Vector2Int(x, y);
            move.team = gameModel.currentPlayersTurn;
            if (gameModel.ValidateMove(move))
            {
                gameModel.MakeMove(move);
            }
        }

        private void Start()
        {
            GenerateUIBoard();
            gameModel = new OthelloGameModel();
            gameModel.gameUpdate += OnGameUpdate;
            gameModel.StartGame();
            //gameModel = new OthelloGameModel();
            //gameModel.gameUpdate += OnGameUpdate;
            //othelloAI.originalGameModel = gameModel;
            //gameModel.gameUpdate += othelloAI.OnGameUpdate;
            //gameModel.StartGame();
        }

        private void GenerateUIBoard()
        {
            buttons = new GameObject[8, 8];
            pawns = new GameObject[8, 8];
            texts = new TextMeshProUGUI[8, 8];
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    GameObject button = Instantiate(buttonPrefab, layoutGroup);
                    FieldController field = button.GetComponent<FieldController>();
                    field.x = x;
                    field.y = y;
                    field.gameController = this;
                    buttons[x, y] = button;
                    texts[x, y] = field.text;
                }
            }
        }
    }
}