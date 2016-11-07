﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ReplaceEqualsExpressionWithStringIsNullOrEmptyRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, BinaryExpressionSyntax binaryExpression)
        {
            if (binaryExpression.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression))
            {
                ExpressionSyntax left = binaryExpression.Left;

                if (left?.IsKind(SyntaxKind.NullLiteralExpression) == false)
                {
                    ExpressionSyntax right = binaryExpression.Right;

                    if (right?.IsKind(SyntaxKind.NullLiteralExpression) == true)
                    {
                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                        ITypeSymbol leftSymbol = semanticModel.GetTypeInfo(left, context.CancellationToken).ConvertedType;

                        if (leftSymbol?.IsString() == true)
                        {
                            string title = (binaryExpression.IsKind(SyntaxKind.EqualsExpression))
                                ? $"Replace '{binaryExpression}' with 'string.IsNullOrEmpty({left})'"
                                : $"Replace '{binaryExpression}' with '!string.IsNullOrEmpty({left})'";

                            context.RegisterRefactoring(
                                title,
                                cancellationToken => RefactorAsync(context.Document, binaryExpression, cancellationToken));
                        }
                    }
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            ExpressionSyntax newNode = InvocationExpression(
                StringType(),
                "IsNullOrEmpty",
                ArgumentList(Argument(binaryExpression.Left)));

            if (binaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
                newNode = LogicalNotExpression(newNode);

            newNode = newNode
                .WithTriviaFrom(binaryExpression)
                .WithFormatterAnnotation();

            SyntaxNode newRoot = root.ReplaceNode(binaryExpression, newNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}