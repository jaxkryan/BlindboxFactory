using Unity.Mathematics;

namespace Script.Utils
{
    public static class RandomExtensions
    {
        private static Random _random = Random.CreateFromIndex((uint)System.Environment.TickCount); // Static seeded instance

        public static TOption PickRandom<TOption>(this TOption[] list)
        {
            if (list == null || list.Length <= 0) return default; // Handle empty or null array
            var n = _random.NextInt(0, list.Length); // Upper bound is exclusive, so no -1 needed
            return list[n];
        }

        public static TOption PickRandom<TOption>(this WeightedOption<TOption>[] list)
        {
            if (list == null || list.Length == 0) return default;
            float totalWeight = 0f;
            list.ForEach(l => totalWeight += l.Weight);
            float randomWeight = _random.NextFloat(0f, totalWeight); // Use float for weighted selection
            TOption result = list[0].Option;
            float cumulativeWeight = 0f;
            for (int i = 0; i < list.Length; i++)
            {
                cumulativeWeight += list[i].Weight;
                if (randomWeight <= cumulativeWeight)
                {
                    result = list[i].Option;
                    break;
                }
            }
            return result;
        }
    }

    public struct WeightedOption<TOption>
    {
        public TOption Option;
        public float Weight;
    }
}