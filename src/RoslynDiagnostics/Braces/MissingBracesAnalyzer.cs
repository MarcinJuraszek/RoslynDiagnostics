using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Threading;

namespace RoslynDiagnostics.Braces
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    class MissingBracesAnalyzer : SyntaxNodeAnalyzer
    {
        internal const string DiagnosticId = "MissingBracesAnalyzer";
        internal const string Description = "Add missing braces.";
        internal const string MessageFormat = "Add missing braces.";
        internal const string Category = "Braces";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        MissingBracesAnalyzer()
            : base(Rule, SyntaxKind.IfStatement, SyntaxKind.ElseClause, SyntaxKind.ForStatement, SyntaxKind.WhileStatement)
        {
        }

        public override void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Location, object[]> addDiagnostic, CancellationToken cancellationToken)
        {
            var ifStatement = node as IfStatementSyntax;
            if (ifStatement != null)
            {
                AddDiagnosticsIfNecessary(ifStatement.Statement, () => ifStatement.IfKeyword.GetLocation(), addDiagnostic);
                return;
            }

            var elseClause = node as ElseClauseSyntax;


            if (elseClause != null)
            {
                AddDiagnosticsIfNecessary(elseClause.Statement, () => elseClause.ElseKeyword.GetLocation(), addDiagnostic);
                return;
            }

            var forStatement = node as ForStatementSyntax;
            if (forStatement != null)
            {
                AddDiagnosticsIfNecessary(forStatement.Statement, () => forStatement.ForKeyword.GetLocation(), addDiagnostic);
                return;
            }

            var whileStatement = node as WhileStatementSyntax;
            if (whileStatement != null)
            {
                AddDiagnosticsIfNecessary(whileStatement.Statement, () => whileStatement.WhileKeyword.GetLocation(), addDiagnostic);
                return;
            }
        }

        private void AddDiagnosticsIfNecessary(StatementSyntax statement, Func<Location> getLocation, Action<Location, object[]> addDiagnostic)
        {
            if(statement != null && !statement.IsKind(SyntaxKind.Block))
            {
                addDiagnostic(getLocation(), null);
            }
        }
    }
}
