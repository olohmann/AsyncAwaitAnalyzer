using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AsyncAwaitAnalyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncVoidMethodsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ASYNC-0001";

        internal const string Title = "Avoid async/void methods.";

        internal const string AsyncVoidMethodViolationMsg = "Declaring async void methods is considered bad practice.\n" + 
                                                            "You should return a Task to signal your clients that your method has an async behavior.\n" +
                                                            "The exception: event handlers are allowed to be implemented as async void.\n"+
                                                            "For more information see http://bit.ly/async-await-best-practice.";

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
            context.RegisterSyntaxNodeAction(CheckForAsyncVoidViolationsInLambdas, SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.SimpleLambdaExpression);
        }

        private void CheckForAsyncVoidViolationsInLambdas(SyntaxNodeAnalysisContext context)
        {
            var parenthesizedLambdaExpressionSyntax = context.Node as ExpressionSyntax;
            if (parenthesizedLambdaExpressionSyntax == null)
            {
                return;
            }
            
            var methodSymbol = context.SemanticModel.GetSymbolInfo(parenthesizedLambdaExpressionSyntax).Symbol as IMethodSymbol;
            if (!methodSymbol.IsAsync)
            {
                return;
            }

            if (!methodSymbol.ReturnsVoid)
            {
                return;
            }

            var parent = parenthesizedLambdaExpressionSyntax.Parent as AssignmentExpressionSyntax;
            if (parent != null)
            {
                // We tolerate event handler assignments.
                var isParentAddAssignmentExpression = parent.IsKind(SyntaxKind.AddAssignmentExpression);
                if (isParentAddAssignmentExpression)
                {
                    return;
                }
            }

            var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0]);
            context.ReportDiagnostic(diagnostic);
        }

        private void CheckForAsyncVoidViolations(SymbolAnalysisContext context)
        {
            var methodSymbol = context.Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            if (!IsAsyncVoidMethod(methodSymbol))
            {
                return;
            }

            if (methodSymbol.IsEventHandler())
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
