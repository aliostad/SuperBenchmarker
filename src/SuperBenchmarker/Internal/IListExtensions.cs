namespace SuperBenchmarker
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class IListExtensions
    {
        public static T Percentile<T>(this IList<T> orderedList, decimal percentile)
        {
            if (orderedList == null)
            {
                throw new ArgumentNullException("orderedList");
            }
            if (orderedList.Count == 0)
            {
                throw new InvalidOperationException("List is empty");
            }
            if ((percentile > 100M) || (percentile < 0M))
            {
                throw new ArgumentOutOfRangeException("percentile");
            }
            int num = (int)((percentile * orderedList.Count) / 100M);
            num = Math.Min(orderedList.Count - 1, num);
            return orderedList[num];
        }

        public static void Shuffle<T>(this IList<T> list, int spareFirstN = 0)
        {
            var r = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(spareFirstN, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
