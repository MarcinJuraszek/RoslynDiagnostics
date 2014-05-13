using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynDiagnostics.Naming
{
    [ExportCodeFixProvider(AsyncMethodNameAnalyzer.DiagnosticId, LanguageNames.CSharp)]
    class AsyncMethodNameFix : CodeFixProvider
    {
        public AsyncMethodNameFix()
            : base(AsyncMethodNameAnalyzer.DiagnosticId)
        {
        }

        public async override Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var token = root.FindToken(span.Start);

            var methodDeclaration = token.Parent as MethodDeclarationSyntax;
            if (methodDeclaration == null)
                return null;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            if (semanticModel == null)
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);
            if (symbol == null)
                return null;

            var project = document.Project;
            if (project == null)
                return null;

            var solution = document.Project.Solution;
            if (solution == null)
                return null;

            var options = solution.Workspace.GetOptions();
            var newName = token.Text + "Async";
            return new[] {  CodeAction.Create("Change method name to '" + newName + "'.",  (ct) => Renamer.RenameSymbolAsync(solution, symbol, newName, options, ct))};
        }
    }
}
