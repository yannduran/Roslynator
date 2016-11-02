// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Pihrtsoft.CodeAnalysis.CSharp.CSharpFactory;

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactorings
{
    internal static class ReplaceStringContainsWithStringIndexOf
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, InvocationExpressionSyntax invocationExpression)
        {
            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken).Symbol as IMethodSymbol;

            if (methodSymbol?.Name == "Contains"
                && methodSymbol.ContainingType?.IsString() == true)
            {
                ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

                if (parameters.Length == 1
                    && parameters[0].Type.IsString())
                {
                    context.RegisterRefactoring(
                        "Replace 'Contains' with 'IndexOf'",
                        cancellationToken => RefactorAsync(context.Document, invocationExpression, context.CancellationToken));
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var memberAccess = (MemberAccessExpressionSyntax)invocationExpression.Expression;

            InvocationExpressionSyntax newInvocationExpression = invocationExpression
                .WithExpression(memberAccess.WithName(IdentifierName("IndexOf")))
                .AddArgumentListArguments(
                    Argument(
                        ParseName("System.StringComparison.OrdinalIgnoreCase").WithSimplifierAnnotation()));

            BinaryExpressionSyntax notEqualsExpression = NotEqualsExpression(newInvocationExpression, NumericLiteralExpression(-1))
                .WithTriviaFrom(invocationExpression)
                .WithFormatterAnnotation();

            SyntaxNode newRoot = root.ReplaceNode(invocationExpression, notEqualsExpression);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}