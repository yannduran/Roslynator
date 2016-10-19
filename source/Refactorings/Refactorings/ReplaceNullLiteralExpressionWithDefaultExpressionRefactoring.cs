// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactorings
{
    internal static class ReplaceNullLiteralExpressionWithDefaultExpressionRefactoring
    {
        public static async Task<Document> RefactorAsync(
            Document document,
            ExpressionSyntax expression,
            ITypeSymbol typeSymbol,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            TypeSyntax type = TypeSyntaxRefactoring.CreateTypeSyntax(typeSymbol)
                .WithSimplifierAnnotation();

            DefaultExpressionSyntax defaultExpression = SyntaxFactory.DefaultExpression(type)
                .WithTriviaFrom(expression);

            SyntaxNode newRoot = root.ReplaceNode(expression, defaultExpression);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}

