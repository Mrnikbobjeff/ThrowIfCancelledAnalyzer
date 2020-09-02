using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UseThrowIfCancellationRequested
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseThrowIfCancellationRequestedAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UseThrowIfCancellationRequested";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyIfStatement, SyntaxKind.IfStatement);
        }

        private static void AnalyIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifstatement = context.Node as IfStatementSyntax;

            if (ifstatement.Condition is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.ValueText.Equals("IsCancellationRequested"))
            {
                var type = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type as ITypeSymbol;
                if (type is null || !type.Name.Equals("CancellationToken"))
                    return;
                if (!(ifstatement.Statement is ThrowStatementSyntax || ifstatement.Statement is BlockSyntax block && block.Statements.Count() == 1  && block.Statements.Single() is ThrowStatementSyntax))
                    return;
                var diagnostic = Diagnostic.Create(Rule, ifstatement.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
