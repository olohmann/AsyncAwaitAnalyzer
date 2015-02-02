using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AsyncAwaitAnalyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncMethodsNamingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ASYNC-0002";

        internal const string Title = "Async methods should end with Async.\n" +
                                      "Exception: Event handlers.";

        internal const string AsyncVoidMethodViolationMsg = "To state the async nature of the method clearly, use the suffix Async.";

        internal const string Category = "AsyncAwait-BestPractices";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
           DiagnosticId,
           Title,
           AsyncVoidMethodViolationMsg,
           Category,
           DiagnosticSeverity.Warning,
           isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(CheckForAsyncVoidViolations, SymbolKind.Method);
        }

        private void CheckForAsyncVoidViolations(SymbolAnalysisContext context)
        {
            var methodSymbol = context.Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            if (!methodSymbol.IsAsync)
            {
                if (!methodSymbol.ReturnType.ToDisplayString().StartsWith("System.Threading.Tasks.Task"))
                {
                    return;
                }                
            }

            if (methodSymbol.IsEventHandler())
            {
                return;
            }

            if (methodSymbol.Name.EndsWith("Async"))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0]);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsAsyncVoidMethod(IMethodSymbol methodSymbol)
        {
            return methodSymbol.ReturnsVoid && methodSymbol.IsAsync;
        }
    }
}
