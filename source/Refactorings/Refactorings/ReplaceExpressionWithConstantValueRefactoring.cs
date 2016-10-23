// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactorings
{
    internal static class ReplaceExpressionWithConstantValueRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, ExpressionSyntax expression)
        {
            SemanticModel semanticModel = await context.GetSemanticModelAsync();

            ISymbol symbol = semanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol;

            if (symbol?.IsErrorType() == false)
            {
                object constantValue = null;

                if (symbol.TryGetConstantValue(out constantValue))
                {
                    ExpressionSyntax newExpression = SyntaxUtility.CreateExpressionFromConstantValue(constantValue);

                    context.RegisterRefactoring(
                        $"Replace '{expression}' with constant value",
                        cancellationToken => RefactorAsync(context.Document, expression, newExpression, cancellationToken));
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ExpressionSyntax expression,
            ExpressionSyntax newExpression,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            SyntaxNode newRoot = root.ReplaceNode(expression, newExpression.WithTriviaFrom(expression));

            return document.WithSyntaxRoot(newRoot);
        }
    }
}