// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Roslynator.CSharp.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddEmptyLineAfterBlockCodeFixProvider))]
    [Shared]
    public class AddEmptyLineAfterBlockCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AddEmptyLineAfterBlock); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            SyntaxTrivia trivia = root.FindTrivia(context.Span.Start);

            Debug.Assert(trivia.IsEndOfLineTrivia(), $"{nameof(trivia)} is not EOF");

            if (trivia == null)
                return;

            CodeAction codeAction = CodeAction.Create(
                "Add empty line after block",
                cancellationToken => AddEmptyLineAfterBlockAsync(context.Document, trivia.Token, cancellationToken),
                DiagnosticIdentifiers.AddEmptyLineAfterBlock + EquivalenceKeySuffix);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static async Task<Document> AddEmptyLineAfterBlockAsync(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            SyntaxToken newToken = token.AppendTrailingTrivia(CSharpFactory.NewLineTrivia());

            SyntaxNode newRoot = root.ReplaceToken(token, newToken);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
