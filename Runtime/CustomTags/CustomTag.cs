using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spyro
{
    [CreateAssetMenu(fileName = "Custom Tags", menuName = "Tags", order = 0)]
    public class CustomTag : ScriptableObject
    {
        public List<string> tags = new List<string>();
    }
}

