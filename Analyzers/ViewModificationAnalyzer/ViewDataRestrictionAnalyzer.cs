using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ViewModificationAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ViewDataRestrictionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "VIEWDATA001";
        private static readonly LocalizableString Title = "View class modifying Data class fields";
        private static readonly LocalizableString MessageFormat = "Class '{0}' (View) is not allowed to modify fields of class '{1}' (Data)";
        private static readonly LocalizableString Description = "Classes with 'View' in their name should not modify fields or properties of classes with 'Data' in their name.";
        private const string Category = "Architecture";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeAssignment, 
                SyntaxKind.SimpleAssignmentExpression, 
                SyntaxKind.AddAssignmentExpression, 
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression, 
                SyntaxKind.DivideAssignmentExpression,
                SyntaxKind.ModuloAssignmentExpression, 
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression, 
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.LeftShiftAssignmentExpression, 
                SyntaxKind.RightShiftAssignmentExpression);
            
            context.RegisterSyntaxNodeAction(AnalyzeIncrementDecrement, 
                SyntaxKind.PostIncrementExpression, 
                SyntaxKind.PostDecrementExpression,
                SyntaxKind.PreIncrementExpression, 
                SyntaxKind.PreDecrementExpression);
        }

        private void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            AnalyzeModification(context, assignment.Left);
        }

        private void AnalyzeIncrementDecrement(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is PostfixUnaryExpressionSyntax postfix)
                AnalyzeModification(context, postfix.Operand);
            else if (context.Node is PrefixUnaryExpressionSyntax prefix)
                AnalyzeModification(context, prefix.Operand);
        }

        private void AnalyzeModification(SyntaxNodeAnalysisContext context, ExpressionSyntax target)
        {
            // Only check code inside the Assets directory
            string filePath = target.SyntaxTree.FilePath;
            if (string.IsNullOrEmpty(filePath) || !filePath.Replace('\\', '/').Contains("/Assets/"))
                return;

            // Find the containing type of the modification
            var containingType = target.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (containingType == null || !containingType.Identifier.Text.Contains("View"))
                return;

            // Get the symbol being modified
            var symbol = context.SemanticModel.GetSymbolInfo(target).Symbol;
            if (symbol == null) return;

            // Check if it's a field or property
            INamedTypeSymbol targetType = null;
            if (symbol is IFieldSymbol fieldSymbol)
            {
                targetType = fieldSymbol.ContainingType;
            }
            else if (symbol is IPropertySymbol propertySymbol)
            {
                targetType = propertySymbol.ContainingType;
            }

            if (targetType != null && targetType.Name.Contains("Data"))
            {
                // If the namespace contains "runtime_data", ignore it
                var ns = targetType.ContainingNamespace?.ToDisplayString();
                if (!string.IsNullOrEmpty(ns) && ns.Contains("runtime_data"))
                    return;

                // Ensure we don't report if the target is actually inside the same type (e.g. nested class)
                var diagnostic = Diagnostic.Create(Rule, target.GetLocation(), containingType.Identifier.Text, targetType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
