﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Pihrtsoft.CodeAnalysis.CSharp.Analysis;

namespace Pihrtsoft.CodeAnalysis.CSharp.Analyzers
{
    internal static class MergeIfStatementWithNestedIfStatementAnalyzer
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, IfStatementSyntax ifStatement)
        {
            if (IfElseChainAnalysis.IsIsolatedIf(ifStatement)
                && ConditionAllowsMerging(ifStatement.Condition))
            {
                IfStatementSyntax nestedIf = GetNestedIfStatement(ifStatement);

                if (nestedIf != null
                    && nestedIf.Else == null
                    && ConditionAllowsMerging(nestedIf.Condition)
                    && TriviaAllowsMerging(ifStatement, nestedIf))
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.MergeIfStatementWithNestedIfStatement,
                        ifStatement.GetLocation());

                    FadeOut(context, ifStatement, nestedIf);
                }
            }
        }

        private static bool ConditionAllowsMerging(ExpressionSyntax condition)
        {
            return condition != null
                && !condition.IsMissing
                && !condition.IsKind(SyntaxKind.LogicalOrExpression);
        }

        private static bool TriviaAllowsMerging(IfStatementSyntax ifStatement, IfStatementSyntax nestedIf)
        {
            TextSpan span = TextSpan.FromBounds(
                nestedIf.FullSpan.Start,
                nestedIf.CloseParenToken.FullSpan.End);

            if (nestedIf.DescendantTrivia(span).All(f => f.IsWhitespaceOrEndOfLine()))
            {
                if (ifStatement.Statement.IsKind(SyntaxKind.Block)
                    && nestedIf.Statement.IsKind(SyntaxKind.Block))
                {
                    var block = (BlockSyntax)nestedIf.Statement;

                    return block.OpenBraceToken.LeadingTrivia.All(f => f.IsWhitespaceOrEndOfLine())
                        && block.OpenBraceToken.TrailingTrivia.All(f => f.IsWhitespaceOrEndOfLine())
                        && block.CloseBraceToken.LeadingTrivia.All(f => f.IsWhitespaceOrEndOfLine())
                        && block.CloseBraceToken.TrailingTrivia.All(f => f.IsWhitespaceOrEndOfLine());
                }

                return true;
            }

            return false;
        }

        private static IfStatementSyntax GetNestedIfStatement(IfStatementSyntax ifStatement)
        {
            StatementSyntax statement = ifStatement.Statement;

            switch (statement?.Kind())
            {
                case SyntaxKind.Block:
                    {
                        var block = (BlockSyntax)statement;

                        if (block.Statements.Count == 1
                            && block.Statements[0].IsKind(SyntaxKind.IfStatement))
                        {
                            return (IfStatementSyntax)block.Statements[0];
                        }

                        break;
                    }
                case SyntaxKind.IfStatement:
                    {
                        return (IfStatementSyntax)statement;
                    }
            }

            return null;
        }

        private static void FadeOut(
            SyntaxNodeAnalysisContext context,
            IfStatementSyntax ifStatement,
            IfStatementSyntax nestedIf)
        {
            DiagnosticHelper.FadeOutToken(context, nestedIf.IfKeyword, DiagnosticDescriptors.MergeIfStatementWithNestedIfStatementFadeOut);
            DiagnosticHelper.FadeOutToken(context, nestedIf.OpenParenToken, DiagnosticDescriptors.MergeIfStatementWithNestedIfStatementFadeOut);
            DiagnosticHelper.FadeOutToken(context, nestedIf.CloseParenToken, DiagnosticDescriptors.MergeIfStatementWithNestedIfStatementFadeOut);

            if (ifStatement.Statement.IsKind(SyntaxKind.Block)
                && nestedIf.Statement.IsKind(SyntaxKind.Block))
            {
                DiagnosticHelper.FadeOutBraces(context, (BlockSyntax)nestedIf.Statement, DiagnosticDescriptors.MergeIfStatementWithNestedIfStatementFadeOut);
            }
        }
    }
}