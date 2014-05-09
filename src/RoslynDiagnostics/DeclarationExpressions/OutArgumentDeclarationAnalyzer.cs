using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Threading;

namespace RoslynDiagnostics.DeclarationExpressions
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    class OutArgumentDeclarationAnalyzer : SyntaxNodeAnalyzer
    {
        internal const string DiagnosticId = "DeclareUsingDeclarationExpression";
        internal const string Description = "Variable can be declared using Declaration Expression";
        internal const string MessageFormat = "'{0}' can be declared using Declaration Expression";
        internal const string Category = "Declaration";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public OutArgumentDeclarationAnalyzer()
            : base(Rule, SyntaxKind.Argument)
        { }

        public override void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Location, object[]> addDiagnostic, CancellationToken cancellationToken)
        {
            // check if we're dealing with method argument with out keyword
            // which does not use DeclarationExpression
            var argument = node as ArgumentSyntax;
            if (argument == null || !argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword) ||
                !argument.Expression.IsKind(SyntaxKind.IdentifierName))
                return;

            var declarator = GetDeclaration(argument, semanticModel, cancellationToken, out SymbolInfo? info);
            if (!info.HasValue || declarator == null)
                return;

            // get containing statement for our variable declaration
            var declaratorStatement = declarator.Parent.Parent;
            var statement = GetContainingStatement(declaratorStatement);
            if (statement == null)
                return;

            // get all variable usages within that statement,
            // traversing syntax tree, looking for IdentifierNameSyntax nodes
            // and checking if that's the same variable we're working on
            var usages = statement.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(x => semanticModel.GetSymbolInfo(x).Equals(info.Value))
                .ToList();

            // get containing statement for method call argument
            // and new scope span
            var argumentStatement = GetContainingStatement(argument);
            var span = new TextSpan(argumentStatement.Span.Start,
                argumentStatement.Span.End - argumentStatement.Span.Start);

            // check if all usages fall within scope and declare our diagnostic if so
            if (usages.All(x => span.Contains(x.Span)))
            {
                addDiagnostic(argument.GetLocation(), new object[] { info.Value.Symbol.Name });
            }
        }

        public static SyntaxNode GetContainingStatement(SyntaxNode node)
        {
            var parent = node.Parent;
            while (parent != null && !(parent is StatementSyntax))
                parent = parent.Parent;
            return parent as StatementSyntax;
        }

        public static VariableDeclaratorSyntax GetDeclaration(ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out SymbolInfo? info)
        {
            info = null;

            var identifier = argument.Expression as IdentifierNameSyntax;
            if (identifier == null)
                return null;

            info = semanticModel.GetSymbolInfo(identifier);
            var symbol = info.Value.Symbol;
            if (symbol == null || symbol.Kind != SymbolKind.Local || symbol.IsImplicitlyDeclared)
                return null;

            var declarators = symbol.DeclaringSyntaxReferences;
            if (declarators == null || declarators.Length != 1)
                return null;

            var declarator = declarators[0];
            return declarator.GetSyntax(cancellationToken) as VariableDeclaratorSyntax;
        }
    }
}
