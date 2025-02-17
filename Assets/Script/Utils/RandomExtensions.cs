using Unity.Mathematics;

namespace Script.Utils {
    public static class RandomExtensions {
        public static TOption PickRandom<TOption>(this TOption[] list) {
            Random r = new Random();
            var n = r.NextInt(0, list.Length - 1);
            
            return list[n];
        }

        public static TOption PickRandom<TOption>(this WeightedOption<TOption>[] list) {
            float totalWeight = 0f;
            list.ForEach(l => totalWeight += l.Weight);
            TOption result = list[0].Option;
            int i = 0;
            while (totalWeight > 0f) {
                result = list[i].Option;
                totalWeight -= list[i].Weight;
                i++;
            }
            
            return result;
        }
    }

    public struct WeightedOption<TOption> {
        public TOption Option;
        public float Weight;
    }
}