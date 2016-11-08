// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class InterpolationRefactoring
    {
        public static void ComputeRefactorings(RefactoringContext context, InterpolationSyntax interpolation)
        {
            if (context.IsRefactoringEnabled(RefactoringIdentifiers.MergeInterpolationIntoInterpolatedString))
            {
                ExpressionSyntax expression = interpolation.Expression;

                if (expression?.IsKind(SyntaxKind.StringLiteralExpression) == true)
                {
                    var interpolatedString = interpolation.Parent as InterpolatedStringExpressionSyntax;

                    var literalExpression = (LiteralExpressionSyntax)expression;

                    if (interpolatedString?.IsVerbatim() == literalExpression.Token.IsVerbatimStringLiteral())
                    {
                        string innerText = StringLiteralExpressionRefactoring.GetInnerText(literalExpression);

                        context.RegisterRefactoring(
                            $"Merge '{innerText}' into interpolated string",
                            cancellationToken => MergeInterpolationIntoInterpolatedStringRefactoring.RefactorAsync(context.Document, interpolation, cancellationToken));
                    }
                }
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.RemoveInterpolation)
                && (interpolation.OpenBraceToken.Span.Contains(context.Span)
                    || interpolation.CloseBraceToken.Span.Contains(context.Span)))
            {
                context.RegisterRefactoring("Remove interpolation",
                    cancellationToken =>
                    {
                        return RemoveInterpolationRefactoring.RefactorAsync(
                            context.Document,
                            interpolation,
                            cancellationToken);
                    });
            }
        }
    }
}