using System;
using System.Collections;
using System.Collections.Generic;

namespace Spyro
{
    public class StateMachine<TEnum> where TEnum : Enum
    {
        private TEnum currentState;
        private Dictionary<TEnum, Func<StateMachine<TEnum>, IEnumerator>> states;

        public TEnum CurrentState => currentState;

        public void SetNextState(TEnum nextState)
        {
            currentState = nextState;
        }

        public StateMachine(TEnum startingState, Dictionary<TEnum, Func<StateMachine<TEnum>, IEnumerator>> states)
        {
            currentState = startingState;
            this.states = states;
        }

        public IEnumerator Update()
        {
            yield return states[currentState](this);
        }
    }
}

