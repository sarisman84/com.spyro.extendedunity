using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Spyro
{
    public class SerializeableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {

        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public void OnAfterDeserialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnBeforeSerialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
            {
                Debug.LogError($"Tried to deserialize a Serializable Dictionary, but the amount of keys ({keys.Count}) and amount of values ({values.Count}) are not the same size!");
            }

            for (int i = 0; i < keys.Count; i++)
            {
                this.Add(keys[i], values[i]);
            }
        }
    }

}

