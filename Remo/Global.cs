using System;
using System.Collections.Generic;
using System.Text;

namespace Remo
{
    public static class Global
    {
        public static bool Is<T>(this T obj, params T[] args)
        {
            foreach (object item in args) { if (item.Equals(obj)) { return true; } } return false;
        }
    }
}
