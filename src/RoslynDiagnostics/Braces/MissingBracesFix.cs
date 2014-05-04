using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynDiagnostics.Braces
{
    [ExportCodeFixProvider(MissingBracesAnalyzer.DiagnosticId, LanguageNames.CSharp)]
    class MissingBracesFix : CodeFixProvider
    {
        public MissingBracesFix()
            : base(MissingBracesAnalyzer.DiagnosticId)
        {
        }

        public async override Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var token = root.FindToken(span.Start);

            if (token.IsKind(SyntaxKind.IfKeyword))
            {
                var ifStatement = (IfStatementSyntax)token.Parent;
                var newIfStatement = ifStatement
                    .WithStatement(SyntaxFactory.Block(ifStatement.Statement))
                    .WithAdditionalAnnotations(Formatter.Annotation);
                return GetCodeActions(ifStatement, newIfStatement, root, document);
            }

            if (token.IsKind(SyntaxKind.ElseKeyword))
            {
                var elseClause = (ElseClauseSyntax)token.Parent;
                var newElseClause = elseClause
                    .WithStatement(SyntaxFactory.Block(elseClause.Statement))
                    .WithAdditionalAnnotations(Formatter.Annotation);
                return GetCodeActions(elseClause, newElseClause, root, document);
            }

            if(token.IsKind(SyntaxKind.ForKeyword))
            {
                var forStatement = (ForStatementSyntax)token.Parent;
                var newForStatement = forStatement
                    .WithStatement(SyntaxFactory.Block(forStatement.Statement))
                    .WithAdditionalAnnotations(Formatter.Annotation);
                return GetCodeActions(forStatement, newForStatement, root, document);
            }

            if(token.IsKind(SyntaxKind.WhileKeyword))
            {
                // while can be used as part of DoStatement as well, so we should perform safe type-check
                var whileStatement = token.Parent as WhileStatementSyntax;
                if(whileStatement != null)
                {
                    var newWhileStatement = whileStatement
                        .WithStatement(SyntaxFactory.Block(whileStatement.Statement))
                        .WithAdditionalAnnotations(Formatter.Annotation);
                    return GetCodeActions(whileStatement, newWhileStatement, root, document);
                }
            }

            return null;
        }

        private IEnumerable<CodeAction> GetCodeActions<T>(T oldNode, T newNode, SyntaxNode root, Document document)
            where T : SyntaxNode
        {
            var newRoot = root.ReplaceNode(oldNode, newNode);
            return new[] { CodeAction.Create("Add missing braces.", document.WithSyntaxRoot(newRoot)) };
        }
    }
}
