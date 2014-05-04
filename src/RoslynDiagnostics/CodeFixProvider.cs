using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynDiagnostics
{
    internal abstract class CodeFixProvider : ICodeFixProvider
    {
        private ImmutableArray<string> _supportedDiagnosticsIds;

        public CodeFixProvider(params string[] supportedDiagnosticIds)
        {
            _supportedDiagnosticsIds = supportedDiagnosticIds.ToImmutableArray();
        }
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return _supportedDiagnosticsIds;
        }

        public abstract Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken);
    }
}