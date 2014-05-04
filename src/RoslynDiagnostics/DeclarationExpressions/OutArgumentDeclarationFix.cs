using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynDiagnostics.DeclarationExpressions
{
    [ExportCodeFixProvider(OutArgumentDeclarationAnalyzer.DiagnosticId, LanguageNames.CSharp)]
    class OutArgumentDeclarationFix : CodeFixProvider
    {
        public OutArgumentDeclarationFix()
            : base(OutArgumentDeclarationAnalyzer.DiagnosticId)
        {
        }

        public async override Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var diagnosticSpan = diagnostics.First().Location.SourceSpan;

            var argument = root.FindToken(diagnosticSpan.Start).Parent as ArgumentSyntax;
            if (argument == null)
                return null;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var declarator = OutArgumentDeclarationAnalyzer.GetDeclaration(argument, semanticModel, cancellationToken, out SymbolInfo? info);
            if (declarator == null || !info.HasValue)
                return Enumerable.Empty<CodeAction>();

            return new[] { CodeAction.Create("Use declaration expression", c => UseDeclarationExpression(document, argument, declarator, c)) };
        }

        private async Task<Document> UseDeclarationExpression(Document document, ArgumentSyntax argument, VariableDeclaratorSyntax declarator,
            CancellationToken cancellationToken)
        {
            // get variable declaration
            var declaration = declarator.Parent;

            // get statement which contains both local declaration statement and method call with out argument
            var statement = OutArgumentDeclarationAnalyzer.GetContainingStatement(declaration.Parent);

            // remove entire local declaration statement or just single variable declaration
            // depending on how many variables are declared within single local declaration statement
            var nodeToRemove = declaration.ChildNodes().OfType<VariableDeclaratorSyntax>().Count() > 1 ? declarator : declaration.Parent;
            var newStatement = statement.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepEndOfLine);

            // get variable type
            var type = declaration.ChildNodes().First() as TypeSyntax;
            // create new Declaration Expression using variable type and declarator
            var newDeclarationExpression = SyntaxFactory.DeclarationExpression(type, declarator);
            // fix the trivia aroung Declaration Expression
            var firstToken = newDeclarationExpression.GetFirstToken();
            var leadingTrivia = firstToken.LeadingTrivia;
            var trimmedDeclarationExpression = newDeclarationExpression.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty));
            // get ArgumentSyntax from newStatement which is equivalent to argument from original syntax tree
            var newArgument = newStatement.DescendantNodes()
                                          .FirstOrDefault(n => n.IsEquivalentTo(argument));
            // replace argument with new version, containing Declaration Expression
            newStatement = newStatement.ReplaceNode(newArgument.ChildNodes().First(), trimmedDeclarationExpression);

            // get root for current document and replace statement with new version
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(statement, newStatement);

            // return document with modified syntax
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
