using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AsyncAwaitAnalyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConfigureAwaitAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ASYNC-003";

        internal const string Title = "Apply .ConfigureAwait(false) to avoid deadlocks";
        internal const string MessageFormat = "Apply .ConfigureAwait(false) in your library code to avoid deadlocks.\n" + 
                                              "More information: http://bit.ly/async-await-sync-context.";
        internal const string Category = "AsyncAwait-BestPractices";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        private static readonly string[] _uiNamepsaces = new[]
        {
            "System.Windows",
            "System.Xaml"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.AwaitExpression);
        }

        private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var awaitExpressionSyntax = (AwaitExpressionSyntax)context.Node;
            var invocationExpressionSyntax = awaitExpressionSyntax.Expression as InvocationExpressionSyntax;
            var awaitedExpression = (invocationExpressionSyntax?.Expression as MemberAccessExpressionSyntax);            

            if (IsUiContext(context))
            {
                return;
            }

            // Accept: .ConfigureAwait(false)
            if (awaitedExpression?.Name.Identifier.Text == "ConfigureAwait")
            {
                var configureAwaitIsFalse = invocationExpressionSyntax.ArgumentList.Arguments.Single().Expression.IsKind(SyntaxKind.FalseLiteralExpression);
                if (configureAwaitIsFalse)
                {
                    return;
                }
            }

            // Reject: All others.
            var diagnostic = Diagnostic.Create(Rule, awaitExpressionSyntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsUiContext(SyntaxNodeAnalysisContext context)
        {
            // Strategy: Find usings that indicate that we are 
            var root = context.Node.SyntaxTree.GetRoot(context.CancellationToken);
            var usings = root.ChildNodes()
                .Where(_ => _.IsKind(SyntaxKind.UsingDirective))
                .OfType<UsingDirectiveSyntax>()
                .Select(_ => _.GetText().ToString())
                .ToList();

            return _uiNamepsaces.Any(_ => usings.Any(u => u.Contains(_)));            
        }
    }
}