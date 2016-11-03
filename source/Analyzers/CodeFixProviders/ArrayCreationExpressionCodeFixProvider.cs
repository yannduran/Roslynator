// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ArrayCreationExpressionCodeFixProvider))]
    [Shared]
    public class ArrayCreationExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.SimplifyArrayCreationExpression); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            ArrayCreationExpressionSyntax arrayCreation = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<ArrayCreationExpressionSyntax>();

            Debug.Assert(arrayCreation != null, "");

            if (arrayCreation == null)
                return;

            CodeAction codeAction = CodeAction.Create(
                "Simplify array creation expression",
                cancellationToken => RefactorAsync(context.Document, arrayCreation, cancellationToken),
                DiagnosticIdentifiers.SimplifyArrayCreationExpression + EquivalenceKeySuffix);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private async Task<Document> RefactorAsync(
            Document document,
            ArrayCreationExpressionSyntax arrayCreation,
            CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            InitializerExpressionSyntax initializer = arrayCreation.Initializer;
            ArrayCreationExpressionSyntax newArrayCreation = arrayCreation
                .WithoutInitializer()
                .WithType(
                    arrayCreation.Type
                        .WithRankSpecifiers(ArrayRankSpecifier(NumericLiteralExpression(0))))
                        .AppendTrailingTrivia(initializer.OpenBraceToken.GetLeadingAndTrailingTrivia())
                        .AppendTrailingTrivia(initializer.CloseBraceToken.GetLeadingAndTrailingTrivia());

            SyntaxNode newRoot = root.ReplaceNode(
                arrayCreation,
                newArrayCreation.WithFormatterAnnotation());

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
