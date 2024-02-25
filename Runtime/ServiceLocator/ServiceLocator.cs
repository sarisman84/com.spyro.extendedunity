using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spyro
{
    public static class ServiceLocator<T> where T : new()
    {
        public static T Service { get; private set; } = new T();
    }

}

