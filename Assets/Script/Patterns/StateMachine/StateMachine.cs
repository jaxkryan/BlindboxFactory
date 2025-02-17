public abstract class StateMachine<T> where T : IState {
    public T CurrentState {get; protected set;}

    public void Initialize(T state) {
        state.Enter();
        CurrentState = state;
    }

    public void ChangeState(T state) {
        CurrentState.Exit();
        CurrentState = state;
        CurrentState.Enter();
    }
}