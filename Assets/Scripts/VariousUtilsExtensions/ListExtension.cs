using System.Collections.Generic;
using UnityEngine;

namespace VariousUtilsExtensions
{
    public static class ListExtension
    {
        /// <summary>
        /// Returns a List\<Tto\> of the same elements as list, effectively "casting each element" to Tto and "converting list".
        /// </summary>
        /// <typeparam name="Tfrom"></typeparam>
        /// <typeparam name="Tto"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Tto> CastListOfComponents<Tfrom, Tto>(this List<Tfrom> list) where Tfrom : Component where Tto : Component
        {
            List<Tto> res = new List<Tto>();
            foreach (var a in list)
            {
                var _temp = a.GetComponent<Tto>();
                if (_temp != null)
                {
                    res.Add(_temp);
                }
            }
            return res;
        }


    }
}