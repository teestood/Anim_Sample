namespace Fusion.FSM
{
	public static class StateExtensions
	{
		public static void AddTransition<TState>(this IOwnedState<TState> state, TState targetState, Transition<TState> transition, bool forced = false)
			where TState : class, IState
		{
			state.AddTransition(new TransitionData<TState>(targetState, transition, forced));
		}
	}
}
