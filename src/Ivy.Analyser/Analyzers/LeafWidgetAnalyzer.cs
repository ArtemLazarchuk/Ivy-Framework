using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ivy.Analyser.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LeafWidgetAnalyzer : DiagnosticAnalyzer
    {
        public const string LeafDiagnosticId = "IVYCHILD001";
        public const string SingleChildDiagnosticId = "IVYCHILD002";

        private static readonly DiagnosticDescriptor LeafRule = new DiagnosticDescriptor(
            LeafDiagnosticId,
            "Adding Children to Leaf Widget",
            "'{0}' does not support children",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            "This widget does not support children. Adding children via the | operator will throw NotSupportedException at runtime.");

        private static readonly DiagnosticDescriptor SingleChildRule = new DiagnosticDescriptor(
            SingleChildDiagnosticId,
            "Adding Multiple Children to Single-Child Widget",
            "'{0}' only supports a single child",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "This widget only supports a single child. Adding multiple children via chained | operators will throw NotSupportedException at runtime.");

        private static readonly HashSet<string> LeafWidgetTypes = new HashSet<string>
        {
            "Ivy.Button",
            "Ivy.Badge",
            "Ivy.Progress",
            "Ivy.Field",
            "Ivy.Detail",
            "Ivy.Dialog",
            "Ivy.DialogHeader",
            "Ivy.HeaderLayout",
            "Ivy.SidebarLayout",
            "Ivy.SidebarMenu",
            "Ivy.FooterLayout",
            "Ivy.DropDownMenu",
            "Ivy.DataTable",
            "Ivy.LineChart",
            "Ivy.PieChart",
            "Ivy.BarChart",
            "Ivy.AreaChart",
            "Ivy.Tooltip",
        };

        private static readonly HashSet<string> LeafInterfaceTypes = new HashSet<string>
        {
            "Ivy.IInput",
        };

        private static readonly HashSet<string> SingleChildWidgetTypes = new HashSet<string>
        {
            "Ivy.Card",
            "Ivy.Sheet",
            "Ivy.Confetti",
            "Ivy.FloatingPanel",
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(LeafRule, SingleChildRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBitwiseOr, SyntaxKind.BitwiseOrExpression);
        }

        private static void AnalyzeBitwiseOr(SyntaxNodeAnalysisContext context)
        {
            var binaryExpr = (BinaryExpressionSyntax)context.Node;
            var leftType = context.SemanticModel.GetTypeInfo(binaryExpr.Left, context.CancellationToken).Type;

            if (leftType == null)
                return;

            if (IsLeafWidget(leftType))
            {
                var diagnostic = Diagnostic.Create(LeafRule, binaryExpr.GetLocation(), leftType.Name);
                context.ReportDiagnostic(diagnostic);
                return;
            }

            if (binaryExpr.Left is BinaryExpressionSyntax leftBinary
                && leftBinary.IsKind(SyntaxKind.BitwiseOrExpression))
            {
                var rootType = GetRootType(leftBinary, context.SemanticModel, context.CancellationToken);
                if (rootType != null && IsSingleChildWidget(rootType))
                {
                    var diagnostic = Diagnostic.Create(SingleChildRule, binaryExpr.GetLocation(), rootType.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static ITypeSymbol? GetRootType(BinaryExpressionSyntax expr, SemanticModel model,
            System.Threading.CancellationToken ct)
        {
            var current = expr;
            while (current.Left is BinaryExpressionSyntax left && left.IsKind(SyntaxKind.BitwiseOrExpression))
            {
                current = left;
            }

            return model.GetTypeInfo(current.Left, ct).Type;
        }

        private static bool IsLeafWidget(ITypeSymbol type)
        {
            var current = type;
            while (current != null)
            {
                if (LeafWidgetTypes.Contains(GetFullTypeName(current)))
                    return true;
                current = current.BaseType;
            }

            foreach (var iface in type.AllInterfaces)
            {
                if (LeafInterfaceTypes.Contains(GetFullTypeName(iface.OriginalDefinition)))
                    return true;
            }

            return false;
        }

        private static bool IsSingleChildWidget(ITypeSymbol type)
        {
            var current = type;
            while (current != null)
            {
                if (SingleChildWidgetTypes.Contains(GetFullTypeName(current)))
                    return true;
                current = current.BaseType;
            }

            return false;
        }

        private static string GetFullTypeName(ITypeSymbol type)
        {
            var ns = type.ContainingNamespace;
            if (ns == null || ns.IsGlobalNamespace)
                return type.Name;
            return ns.ToDisplayString() + "." + type.Name;
        }
    }
}
