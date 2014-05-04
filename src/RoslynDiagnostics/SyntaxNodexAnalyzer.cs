using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace RoslynDiagnostics
{
    internal abstract class SyntaxNodexAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        private DiagnosticDescriptor _rule;

        internal SyntaxNodexAnalyzer(DiagnosticDescriptor rule, params SyntaxKind[] kindsOfInterest)
        {
            _rule = rule;
            SyntaxKindsOfInterest = kindsOfInterest.ToImmutableArray();
            SupportedDiagnostics = ImmutableArray.Create(rule);
        }

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; private set; }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; private set; }

        public abstract void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Location, object[]> addDiagnostic, CancellationToken cancellationToken);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            AnalyzeNode(node, semanticModel, (location, messageArgs) => addDiagnostic(Diagnostic.Create(_rule, location, messageArgs)), cancellationToken);
        }
    }
}
