using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spyro
{
    public class ObjectPool : IEnumerator<(GameObject, int)>, IEnumerable<(GameObject, int)>
    {
        private List<GameObject> pooledList;
        private List<bool> poolState;
        private int iteration;
        private Action<GameObject, int> onInsReset;
        public ObjectPool(int poolAmount, Transform parent = null, Action<GameObject> onInsInit = null, Action<GameObject, int> onEntityReset = null)
        {
            pooledList = new List<GameObject>();
            poolState = new List<bool>();
            for (int i = 0; i < poolAmount; ++i)
            {
                var go = new GameObject($"Pooled Object [{i}]");
                go.transform.SetParent(parent);
                go.SetActive(false);
                if (onInsInit != null)
                    onInsInit.Invoke(go);
                onInsReset = onEntityReset;
                pooledList.Add(go);
                poolState.Add(false);
            }
            iteration = 0;
        }
        public GameObject UseFirstAvailableEntity()
        {
            for (int i = 0; i < pooledList.Count; ++i)
            {
                var go = pooledList[i];
                if (!go.activeSelf)
                {
                    go.SetActive(true);
                    poolState[i] = true;
                    return go;
                }
            }
            return default;
        }

        public void ResetPool()
        {
            for (int i = 0; i < pooledList.Count; ++i)
            {
                var go = pooledList[i];

                go.SetActive(false);
                if (poolState[i] && onInsReset != null)
                {
                    onInsReset.Invoke(go, i);
                }
                poolState[i] = false;
            }
        }

        public (GameObject, int) Current
        {
            get
            {
                if (iteration < 0 || iteration >= pooledList.Count)
                    throw new InvalidOperationException();
                return (pooledList[iteration], iteration);
            }
        }

        object IEnumerator.Current => Current;
        public bool MoveNext()
        {
            if (pooledList.All(x => !x.activeSelf))
                return false;
            while (iteration < pooledList.Count)
            {
                iteration++;
                if (iteration >= pooledList.Count) // Reached end of list
                    return false;
                if (pooledList[iteration].activeSelf)
                {
                    return true;
                }
                if (poolState[iteration])
                {
                    poolState[iteration] = false;
                    if (onInsReset != null)
                        onInsReset?.Invoke(pooledList[iteration], iteration);
                }


            }
            return false; // No more active objects
        }

        public void Reset()
        {
            iteration = -1;
        }

        // Implementing IEnumerable<T> interface for iteration
        public IEnumerator<(GameObject, int)> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            // Dispose if needed
        }
    }


}

