namespace Script.Gacha.Shard {
    public interface IShard {
        
        int ShardsNeeded { get; }
        void Combine();
    }
}