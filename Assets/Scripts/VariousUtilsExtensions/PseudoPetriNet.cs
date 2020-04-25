using System;
using System.Collections.Generic;
using System.Linq;

namespace VariousUtilsExtensions
{

    public class PseudoPetriNet<TLabel>
	{

		public class PseudoPetriToken
        {

			public TLabel sourceState;

			public TLabel currentState;

        }
		
		protected class State
		{

            public List<PseudoPetriToken> hostedTokens = new List<PseudoPetriToken>();

			private readonly TLabel label;
			private readonly Action<List<PseudoPetriToken>> _onStart;
			private readonly Action<List<PseudoPetriToken>> _onUpdate;
			private readonly Action<List<PseudoPetriToken>> _onStop;

			public void onStart(List<PseudoPetriToken> tokenList)
			{
				if (_onStart != null && tokenList.Count > 0)
				_onStart(tokenList);
			}

			public void onUpdate()
			{
				if (_onUpdate != null && hostedTokens.Count > 0)
				_onUpdate(hostedTokens);
			}

			public void onStop(List<PseudoPetriToken> tokenList)
			{
				if (_onStop != null && tokenList.Count > 0)
				_onStop(tokenList);
			}

			public State(TLabel label, Action<List<PseudoPetriToken>> onStart, Action<List<PseudoPetriToken>> onUpdate, Action<List<PseudoPetriToken>> onStop)
			{
				this._onStart = (_) => { hostedTokens.AddRange(_); };
				this._onStart += onStart;
				
				this._onUpdate = onUpdate;
				
				this._onStop = onStop;
				this._onStop += (_) => { foreach (var v in hostedTokens) hostedTokens.Remove(v); };
				// here or before ?				

				this.label = label;
			}

		}


		public void SetTokenCurrentState(PseudoPetriToken token, TLabel label)
		{
			if(GetAllTokens().Contains(token))
				ChangeState(new List<PseudoPetriToken>() { token }, label);
			else
				throw new System.Exception("Token should already exist in net to for this.");
		}

		public PseudoPetriToken AddTokenAtState(TLabel label)
		{
			PseudoPetriToken t = new PseudoPetriToken();
			ChangeState(new List<PseudoPetriToken>() { t }, label);
			return t;
		}

		public void Update()
		{
			foreach (var t in GetAllTokens())
			{
				if (GetTokenCurrentState<State>(t) != null)
				{
					GetTokenCurrentState<State>(t).onUpdate();
				}
			}
		}

		public void AddState(TLabel label, Action<List<PseudoPetriToken>> onStart, Action<List<PseudoPetriToken>> onUpdate, Action<List<PseudoPetriToken>> onStop)
		{
			SetStateFromLabel(label, new State(label, onStart, onUpdate, onStop));
		}

		public void AddState(TLabel label, Action<List<PseudoPetriToken>> onStart, Action<List<PseudoPetriToken>> onUpdate)
		{
			AddState(label, onStart, onUpdate, null);
		}

	    public void AddState(TLabel label, Action<List<PseudoPetriToken>> onStart)
		{
			AddState(label, onStart, null);
		}

		public void AddState(TLabel label)
		{
			AddState(label, null);
		}

		public void RemoveState(TLabel label)
		{
			_stateDictionary.Remove(label);
		}

		public bool ContainsState(TLabel label)
		{
			return _stateDictionary.ContainsKey(label);
		}

		public IEnumerable<TLabel> GetAllStates()
		{
			return _stateDictionary.Keys;
		}

		public IEnumerable<TLabel> GetAllStatesWithTokens()
		{
			List<TLabel> res = new List<TLabel>();
			foreach (var sts in _stateDictionary)
			{
				if (sts.Value.hostedTokens.Count > 0)
					res.Add(sts.Key);
			}
			return res;
		}

		private Dictionary<TLabel, State> _stateDictionary = new Dictionary<TLabel, State>();		

		private List<PseudoPetriToken> GetAllTokens()
		{
			List<PseudoPetriToken> res = new List<PseudoPetriToken>();
			foreach (var sts in _stateDictionary)
			{
				res.AddRange(sts.Value.hostedTokens);
			}
			return res;
		}

		private void ChangeState(List<PseudoPetriToken> tokens, TLabel newState)
		{
			foreach (var t in tokens)
			{
				if (GetTokenCurrentState<State>(t) != null)
				{
					GetTokenCurrentState<State>(t).onStop(tokens);
				}
			}
				//SetTokenCurrentState(t, newState);

			if (GetStateFromLabel<State>(newState) != null)
			{
				GetStateFromLabel<State>(newState).onStart(tokens);
			}
		}

		private TLabel GetTokenCurrentStateLabel(PseudoPetriToken token)
		{
			TLabel res = default(TLabel);
			foreach (var sts in _stateDictionary)
			{
				if (sts.Value.hostedTokens.Contains(token))
				{
					if (res.Equals(default(TLabel)))
						res = sts.Key;
					else
						throw new System.Exception("One token should not be in different states at the same time !!");
				}
			}
			return res;
			//return token.currentState;
		}

		private TState GetTokenCurrentState<TState>(PseudoPetriToken token) where TState : State
		{
			var t = GetTokenCurrentStateLabel(token);
			if (t != null)
				return GetStateFromLabel<TState>(t);
			else
				return null;
		}
		
		private TState GetStateFromLabel<TState>(TLabel label) where TState : State
		{
			return _stateDictionary[label] as TState;
		}

		private void SetStateFromLabel(TLabel label, State state)
		{
			_stateDictionary[label] = state;
		}

	}
	
}