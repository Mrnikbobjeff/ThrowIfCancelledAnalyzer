using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;

namespace UseThrowIfCancellationRequested
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseThrowIfCancellationRequestedCodeFixProvider)), Shared]
    public class UseThrowIfCancellationRequestedCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(UseThrowIfCancellationRequestedAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IfStatementSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution: c => MakeUppercaseAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Solution> MakeUppercaseAsync(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            

            var throwIfCt = ExpressionStatement(
                                InvocationExpression(
                                   MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       (ifStatement.Condition as MemberAccessExpressionSyntax).Expression,
                                       IdentifierName("ThrowIfCancellationRequested")))).WithAdditionalAnnotations(Formatter.Annotation);
            var newRoot = root.ReplaceNode(ifStatement, throwIfCt.WithTriviaFrom(ifStatement.Statement));
            //var finalRoot = Formatter.Format(newRoot, Formatter.Annotation, document.Project.Solution.Workspace);
            return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, newRoot.NormalizeWhitespace());
        }
    }
}
