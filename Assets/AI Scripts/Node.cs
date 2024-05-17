using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AI
{
    public class Node
    {
        public List<Node> children;
        public float value;
        public int visitCount;
        public int team;
        public AIMove move;
        public Node parent;

        public Node(AIMove move, Node parent)
        {
            this.move = move;
            this.parent = parent;
            team = move.team;
            visitCount = 0;
            value = 0;
            children = new List<Node>();
        }

        public Node(int team)
        {
            this.team = team;
            move = null;
            parent = null;
            visitCount = 0;
            value = 0;
            children = new List<Node>();
        }

        public Node()
        {
        }
    }
}