using System.Collections.Generic;

namespace Nrealus.Extensions
{
    public static class MyCollectionsExtension
    {

        public static void Remove2<T>(this List<T> listA, List<T> listB)
        {
            foreach (var v in listB)
            {
                listA.Remove(v);
            }
        }

        public static bool Contains2<T>(this List<T> listA, List<T> listB)
        {
            bool b = false;
            foreach (var v in listB)
            {
                if (!listA.Contains(v))
                    return false;
                b = true;
            }
            return b;
        }

        public static bool EqualContentUnordered<T>(this List<T> aListA, List<T> aListB)
        {
            if (aListA == null || aListB == null || aListA.Count != aListB.Count)
                return false;
            if (aListA.Count == 0)
                return true;
            Dictionary<T, int> lookUp = new Dictionary<T, int>();
            // create index for the first list
            for(int i = 0; i < aListA.Count; i++)
            {
                int count = 0;
                if (!lookUp.TryGetValue(aListA[i], out count))
                {
                    lookUp.Add(aListA[i], 1);
                    continue;
                }
                lookUp[aListA[i]] = count + 1;
            }
            for (int i = 0; i < aListB.Count; i++)
            {
                int count = 0;
                if (!lookUp.TryGetValue(aListB[i], out count))
                {
                    // early exit as the current value in B doesn't exist in the lookUp (and not in ListA)
                    return false;
                }
                count--;
                if (count <= 0)
                    lookUp.Remove(aListB[i]);
                else
                    lookUp[aListB[i]] = count;
            }
            // if there are remaining elements in the lookUp, that means ListA contains elements that do not exist in ListB
            return lookUp.Count == 0;
        }
    }
}