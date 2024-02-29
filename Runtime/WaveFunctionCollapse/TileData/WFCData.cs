using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spyro.WFC
{
    public class WFCData : ScriptableObject
    {
        [SerializeField][HideInInspector] public int dataWidth { get; set; }
        [SerializeField][HideInInspector] public int dataHeight { get; set; }
        [SerializeField][HideInInspector] public int dataLength { get; set; }
        [SerializeField][HideInInspector] public int[] data { get; set; }
        [SerializeField][HideInInspector] public GameObject[] dataType { get; set; }
    }
}

