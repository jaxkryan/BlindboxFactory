public interface IChainOfResponsibility<TChain>  where TChain : IChainOfResponsibility<TChain> {
    TChain SetNext(TChain next);
    void Process();
}