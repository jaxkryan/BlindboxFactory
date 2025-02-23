using System;
using System.Collections.Generic;
using System.Linq;
using Script.Utils;

namespace Script.Resources {

    [Serializable]
    public struct ResourceConversionPair {
        public Resource From;
        [ValueMatchField(nameof(From))]
        public Resource To;
    }

    public static class ConversionExtension {

        public static bool TryConversion(this Dictionary<ResourceConversionPair, float> dict, Resource from,
            Resource to, int amount, out int result, bool tryFindingExchangeRate = true, bool roundDownEachConversion = true) {
            result = Int32.MaxValue;
            if (!dict.TryGetConversionRates(from, to, out var path, tryFindingExchangeRate)) return false;
            float a = amount;
            foreach (var rate in path.Select(p => p.Weight)) {
                a = roundDownEachConversion ? MathF.Floor(a * rate) : a * rate;
            }

            result = (int)MathF.Floor(a);

            return true;
        }

        public static bool TryConversion(this Dictionary<ResourceConversionPair, float> dict,
            ResourceConversionPair pair, int amount, out int result, bool tryFindingExchangeRate = true, bool roundDownEachConversion = true)
            => dict.TryConversion(pair.From, pair.To, amount, out result, roundDownEachConversion);

        public static bool TryGetConversionRates(this Dictionary<ResourceConversionPair, float> dict, Resource from, Resource to, out List<(Resource Node, float Weight)> path, bool tryFindingExchangeRate = true) {
            path = default;

            //Check if there is a set pair
            if (dict.TryGetValue(new ResourceConversionPair { From = from, To = to }, out var rate)) {
                path = new List<(Resource Node, float Weight)>() { (to, rate) };
                return true;
            }

            //Else use path finding
            if (tryFindingExchangeRate) 
                if (PathFinding(dict, from).TryGetValue(to, out var r) && r.Any(node => node.Node == to)) {
                    path = r;
                    return true;

                }

            return false;

            Dictionary<Resource, List<(Resource Node, float Rate)>> PathFinding(Dictionary<ResourceConversionPair, float> dict, Resource from) {
                var ret =  Enum.GetValues(typeof(Resource)).Cast<Resource>().ToDictionary(r => r, r => new List<(Resource Node, float Rate)>());
                ret[from] = new List<(Resource Node, float Rate)>(){ (from, 0)};
                ;

                List<Resource> nodes = ret.Select(r => r.Key).Where(r => TryGetPathWeight(from, r, out _)).ToList();
                foreach (var node in nodes) {
                    PathRecursive(node, new List<Resource>());
                }

                //foreach (var r in ret) {
                //    Console.WriteLine($"-- RESOURCE: {r.Key} --");
                //    foreach (var node in r.Value) {
                //        Console.WriteLine($"{node.Node}: {node.Rate}");
                //    }
                //}
                return ret;

                float GetRateFromPath(List<(Resource Node, float Rate)> list)  {
                    if (list.Count == 0) return Single.MaxValue;
                    float i = 1;
                    foreach (var w in list) {
                        i *= w.Rate;
                    }
                    return i;
                }

                void PathRecursive(Resource current, List<Resource> path) {
                    var currentRate = 1f;
                    var prev = path.Count == 0 ? from : path.Last();
                    if (TryGetPathWeight(prev, current, out var prevToCurrent)) currentRate = prevToCurrent;

                    if (GetRateFromPath(ret[current]) >= currentRate * GetRateFromPath(ret[prev])) {
                        if (prev != from) ret[prev].ForEach(p => ret[current].Add(p));
                        ret[current].Add((current, currentRate));
                        currentRate = GetRateFromPath(ret[current]);
                    }

                    List<Resource> nodes = ret.Select(r => r.Key).Where(r => TryGetPathWeight(current, r, out _) && !path.Contains(r)).ToList();
                    Dictionary<Resource, float> relativeDistance = new();
                    foreach (var node in nodes) {
                        if (!TryGetPathWeight(current, node, out var weight)) continue;
                        if (GetRateFromPath(ret[node]) < weight * currentRate) continue;
                        
                        path.Add(current);
                        PathRecursive(node, path);
                    }
                }

                bool TryGetPathWeight(Resource from, Resource to, out float weight) {
                    weight = Single.MaxValue;
                    if (from == to) return false;

                    if (dict.TryGetValue(new ResourceConversionPair { From = from, To = to }, out var w)) {
                        weight = w;
                        //Console.WriteLine($"---- From {from} to {to}: {weight}"); 
                        return true;
                    }
                    if (dict.TryGetValue(new ResourceConversionPair { From = to, To = from }, out var wReverse)) {
                        weight = 1 / wReverse;
                        //Console.WriteLine($"---- From {from} to {to}: {weight}"); 
                        return true;
                    }


                    return false;
                }
            }
        }

        public static bool TryGetConversionRates(this Dictionary<ResourceConversionPair, float> dict,
            ResourceConversionPair pair, out List<(Resource Node, float Weight)> path, bool tryFindingExchangeRate = true)
            => dict.TryGetConversionRates(pair.From, pair.To, out path, tryFindingExchangeRate);
    }
}