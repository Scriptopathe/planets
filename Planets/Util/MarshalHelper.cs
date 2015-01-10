using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace SimpleTriangle.Util
{
    public static class MarshalHelper
    {
        /// <summary>
        /// Transforme un objet en un array comprenant ses membres afin d'être exporté.
        /// </summary>
        public static byte[] GetArray(object o)
        {
            var len = Marshal.SizeOf(o);
            var arr = new byte[len];
            var ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(o, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }
}
