using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Threading;

namespace RoslynDiagnostics.Naming
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    class AsyncMethodNameAnalyzer : SyntaxNodeAnalyzer
    {
        internal const string DiagnosticId = "AsyncMethodNameShouldEndWithAsync";
        internal const string Description = "Method name should end with 'Async' when method is async.";
        internal const string MessageFormat = "Method name should end with 'Async' when it is async. '{0}' method should be named '{1}'.";
        internal const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public AsyncMethodNameAnalyzer()
            : base(Rule, SyntaxKind.MethodDeclaration)
        { }

        public override void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Location, object[]> addDiagnostic, CancellationToken cancellationToken)
        {
            var methodDeclaration = node as MethodDeclarationSyntax;
            if (methodDeclaration == null)
                return;

            if (methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword) &&
                !methodDeclaration.Identifier.Text.EndsWith("Async"))
                addDiagnostic(methodDeclaration.Identifier.GetLocation(), new object[] { methodDeclaration.Identifier, string.Concat(methodDeclaration.Identifier, "Async") });
        }
    }
}
