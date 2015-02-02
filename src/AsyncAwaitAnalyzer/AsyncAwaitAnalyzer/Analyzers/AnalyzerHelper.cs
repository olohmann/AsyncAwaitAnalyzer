using Microsoft.CodeAnalysis;

namespace AsyncAwaitAnalyzer.Analyzers
{
    public static class AnalyzerHelper
    {
        /// <summary>
        /// Checks if the given method symbol is likely to be an event handler.
        /// </summary>
        /// <param name="methodSymbol"></param>
        /// <returns>true if it is an event handler, false otherwise.</returns>
        public static bool IsEventHandler(this IMethodSymbol methodSymbol)
        {
            if (methodSymbol.Parameters.Length != 2)
            {
                return false;
            }

            if (methodSymbol.Parameters[0].Type.Name != "Object")
            {
                return false;
            }

            ITypeSymbol baseType = methodSymbol.Parameters[1].Type;
            while (baseType.BaseType != null && baseType.BaseType.Name != "Object")
            {
                baseType = baseType.BaseType;
            }

            if (baseType.Name != "EventArgs")
            {
                return false;
            }

            return true;
        }
    }
}
