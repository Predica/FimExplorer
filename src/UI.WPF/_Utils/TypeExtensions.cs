using System;

namespace Predica.FimExplorer.UI.WPF
{
    public static class TypeExtensions
    {
        public static bool Is<TOther>(this Type @this)
        {
            return @this == (typeof(TOther));
        }

        public static bool IsNot<TOther>(this Type @this)
        {
            return !@this.Is<TOther>();
        }
    }
}