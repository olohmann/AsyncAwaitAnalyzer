using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;
using Microsoft.CodeAnalysis.Rename;

namespace AsyncAwaitAnalyzer.Analyzers
{
    [ExportCodeFixProvider("AsyncMethodsNamingCodeFixProvider", LanguageNames.CSharp), Shared]
    public class AsyncMethodsNamingCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> GetFixableDiagnosticIds()
        {
            return ImmutableArray.Create(AsyncMethodsNamingAnalyzer.DiagnosticId);
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task ComputeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            SyntaxToken syntaxToken = root.FindToken(diagnosticSpan.Start);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var methodSymbol = semanticModel.GetDeclaredSymbol(syntaxToken.Parent, context.CancellationToken) as IMethodSymbol;

            context.RegisterFix(CodeAction.Create("Add 'Async' suffix to method name", ctoken => FixMethodName(context.Document, methodSymbol, ctoken)), diagnostic);
        }

        private async Task<Solution> FixMethodName(Document document, IMethodSymbol methodSymbol, CancellationToken cancellationToken)
        {            
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(
                document.Project.Solution, 
                methodSymbol, 
                methodSymbol.Name + "Async", 
                optionSet, 
                cancellationToken).ConfigureAwait(false);

            return newSolution;
        }
    }
}