// EXTENSION OF "StateMachine" CLASS BY NREALUS

// Copyright Gamelogic (c) http://www.gamelogic.co.za

using System;
using System.Collections.Generic;
using Gamelogic.Extensions;

namespace Nrealus.Extensions
{
	
	public class ExtendedStateMachine<TLabel> : StateMachine<TLabel>
	{

		protected class ExtendedState : StateMachine<TLabel>.State
		{

			public readonly Dictionary<TLabel, Action> onDepartureTo;
			public readonly Dictionary<TLabel, Action> onArrivalFrom;

			public ExtendedState(TLabel label, Action onStart, Action onUpdate, Action onStop, Dictionary<TLabel, Action> onDepartureTo, Dictionary<TLabel, Action> onArrivalFrom)
				: base(label, onStart, onUpdate, onStop)
			{
				this.onDepartureTo = onDepartureTo;
				this.onArrivalFrom = onArrivalFrom;
			}

		}

		private Dictionary<TLabel, ExtendedState> _stateDictionary = new Dictionary<TLabel, ExtendedState>();
		protected override TState GetStateFromLabelAs<TState>(TLabel label)
		{ return _stateDictionary[label] as TState; }

		protected override void SetStateFromLabel(TLabel label, State state)
		{
			_stateDictionary[label] = (ExtendedState)state;
		}

		private ExtendedState _currentState;
		protected override TState GetStateCurrentState<TState>()
		{ return _currentState as TState; }

		protected override void SetStateCurrentState(State state)
		{ _currentState = (ExtendedState)state; }

		/// <summary>
		/// Adds a state, and the delegates that should run 
		/// when the state starts, stops, 
		/// and when the state machine is updated.
		/// 
		/// Any delegate can be null, and wont be executed.
		/// </summary>
		/// <param name="label">The name of the state to add.</param>
		/// <param name="onStart">The action performed when the state is entered.</param>
		/// <param name="onUpdate">The action performed when the state machine is updated in the given state.</param>
		/// <param name="onStop">The action performed when the state machine is left.</param>
		/// <param name="onDepartureTo">The action performed when the state machine leaves its state and transitions to another one.</param>
		/// <param name="onArrivalFrom">The action performed when the state machine enters the state from another one.</param>
		public void AddState(TLabel label, Action onStart, Action onUpdate, Action onStop, Dictionary<TLabel, Action> onDepartureTo, Dictionary<TLabel, Action> onArrivalFrom)
		{
			SetStateFromLabel(label, new ExtendedState(label, onStart, onUpdate, onStop, onDepartureTo, onArrivalFrom));
		}

		/// <summary>
		/// Adds a state, and the delegates that should run 
		/// when the state starts, 
		/// and when the state machine is updated.
		/// 
		/// Any delegate can be null, and wont be executed.
		/// </summary>
		/// <param name="label">The name of the state to add.</param>
		/// <param name="onStart">The action performed when the state is entered.</param>
		/// <param name="onUpdate">The action performed when the state machine is updated in the given state.</param>
		/// <param name="onStop">The action performed when the state machine is left.</param>
		/// <param name="onDepartureTo">The action performed when the state machine leaves its state and transitions to another one.</param>
		public void AddState(TLabel label, Action onStart, Action onUpdate, Action onStop, Dictionary<TLabel, Action> onDepartureTo)
		{
			AddState(label, onStart, onUpdate, onStop, onDepartureTo, null);
		}

		/// <summary>
		/// Adds a state, and the delegates that should run 
		/// when the state starts, stops, 
		/// and when the state machine is updated.
		/// 
		/// Any delegate can be null, and wont be executed.
		/// </summary>
		/// <param name="label">The name of the state to add.</param>
		/// <param name="onStart">The action performed when the state is entered.</param>
		/// <param name="onUpdate">The action performed when the state machine is updated in the given state.</param>
		/// <param name="onStop">The action performed when the state machine is left.</param>
		public override void AddState(TLabel label, Action onStart, Action onUpdate, Action onStop)
		{
			SetStateFromLabel(label, new ExtendedState(label, onStart, onUpdate, onStop, null, null));
		}

		/// <summary>
		/// Changes the state from the existing one to the state with the given label.
		/// 
		/// It is legal (and useful) to transition to the same state, in which case the 
		/// current state's onStop action is called, the onStart action is called, and the
		/// state keeps on updating as before. The behaviour is exactly the same as switching to
		/// a new state.
		/// </summary>
		protected override void ChangeState(TLabel newState)
		{
			
			if (GetStateCurrentState<ExtendedState>() != null && GetStateCurrentState<ExtendedState>().onStop != null)
			{
				GetStateCurrentState<ExtendedState>().onStop();
			}
			if (GetStateCurrentState<ExtendedState>() != null && GetStateCurrentState<ExtendedState>().onDepartureTo != null && GetStateCurrentState<ExtendedState>().onDepartureTo.ContainsKey(newState))
			{
				GetStateCurrentState<ExtendedState>().onDepartureTo[newState]();
			}

			var oldState = GetStateCurrentState<ExtendedState>();
			SetStateCurrentState(GetStateFromLabelAs<ExtendedState>(newState));
			
			if (oldState != null && GetStateCurrentState<ExtendedState>().onArrivalFrom != null && GetStateCurrentState<ExtendedState>().onArrivalFrom.ContainsKey(oldState.label))
			{
				GetStateCurrentState<ExtendedState>().onArrivalFrom[oldState.label]();
			}
			if (GetStateCurrentState<ExtendedState>().onStart != null)
			{
				GetStateCurrentState<ExtendedState>().onStart();
			}
		}

	}
}