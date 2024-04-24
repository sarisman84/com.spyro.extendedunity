using System;
using System.Collections;
using System.Collections.Generic;

namespace Spyro
{
    public class StateMachine<TEnum> where TEnum : Enum
    {
        struct State
        {
            public Func<StateMachine<TEnum>, IEnumerator> onEnterState;
            public Func<StateMachine<TEnum>, IEnumerator> onExitState;
            public Func<StateMachine<TEnum>, IEnumerator> duringState;
        }

        private TEnum currentState;
        private Dictionary<TEnum, State> states;

        public TEnum CurrentState => currentState;

        public IEnumerator SetNextState(TEnum nextState)
        {
            if (states[currentState].onExitState != null)
                yield return states[currentState].onExitState(this);
            currentState = nextState;
            if (states[currentState].onEnterState != null)
                yield return states[currentState].onEnterState(this);
        }

        public StateMachine(TEnum startingState)
        {
            currentState = startingState;
            states = new Dictionary<TEnum, State>();
        }

        public void SetState(TEnum state, Func<StateMachine<TEnum>, IEnumerator> duringState)
        {
            if (!states.ContainsKey(state))
            {
                states.Add(state, new State());
            }

            states[state] = new State() { duringState = duringState };
        }

        public void SetState(TEnum state, Func<StateMachine<TEnum>, IEnumerator> duringState, Func<StateMachine<TEnum>, IEnumerator> enteringState)
        {
            if (!states.ContainsKey(state))
            {
                states.Add(state, new State());
            }

            states[state] = new State() { duringState = duringState, onEnterState = enteringState };
        }

        public void SetState(TEnum state, Func<StateMachine<TEnum>, IEnumerator> duringState, Func<StateMachine<TEnum>, IEnumerator> enteringState, Func<StateMachine<TEnum>, IEnumerator> exitingState)
        {
            if (!states.ContainsKey(state))
            {
                states.Add(state, new State());
            }

            states[state] = new State() { duringState = duringState, onEnterState = enteringState, onExitState = exitingState };
        }

        public IEnumerator Update()
        {
            if (states[currentState].duringState != null)
                yield return states[currentState].duringState(this);
        }
    }
}

