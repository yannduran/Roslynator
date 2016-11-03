// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ArrayCreationExpressionDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.SimplifyArrayCreationExpression); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.RegisterSyntaxNodeAction(f => AnalyzeArrowExpressionClause(f), SyntaxKind.ArrayCreationExpression);
        }

        private void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context)
        {
            if (GeneratedCodeAnalyzer?.IsGeneratedCode(context) == true)
                return;

            var items = new string[] { null };

            var arrayCreation = (ArrayCreationExpressionSyntax)context.Node;

            InitializerExpressionSyntax initializer = arrayCreation.Initializer;

            if (initializer?.Expressions.Count == 0)
            {
                ArrayTypeSyntax type = arrayCreation.Type;

                if (type != null)
                {
                    SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers = type.RankSpecifiers;

                    if (rankSpecifiers.Count == 1)
                    {
                        ArrayRankSpecifierSyntax rankSpecifier = rankSpecifiers.First();

                        SeparatedSyntaxList<ExpressionSyntax> sizes = rankSpecifier.Sizes;

                        if (sizes.Count == 1
                            && sizes.First().IsKind(SyntaxKind.OmittedArraySizeExpression))
                        {
                            context.ReportDiagnostic(
                                DiagnosticDescriptors.SimplifyArrayCreationExpression,
                                arrayCreation.GetLocation());
                        }
                    }
                }
            }
        }
    }
}
