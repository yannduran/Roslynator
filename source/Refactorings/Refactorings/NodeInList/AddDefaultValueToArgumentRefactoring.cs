// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactorings.NodeInList
{
    internal class AddDefaultValueToArgumentRefactoring : NodeInListRefactoring<ArgumentSyntax, ArgumentListSyntax>
    {
        public AddDefaultValueToArgumentRefactoring(ArgumentListSyntax listSyntax)
            : base(listSyntax, listSyntax.Arguments)
        {
        }

        public async Task ComputeRefactoringAsync(RefactoringContext context, ArgumentListSyntax argumentList)
        {
            if (context.Settings.IsRefactoringEnabled(RefactoringIdentifiers.AddDefaultValueToArgument))
            {
                int index = FindNode(context.Span);

                if (index != -1)
                {
                    SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                    ArgumentSyntax argument = argumentList.Arguments[index];

                    foreach (ITypeSymbol typeSymbol in argument.DetermineParameterTypes(semanticModel, context.CancellationToken))
                    {
                        ExpressionSyntax defaultValue = SyntaxUtility.CreateDefaultValue(typeSymbol);

                        if (defaultValue != null)
                        {
                            context.RegisterRefactoring(
                                $"Add default value '{defaultValue}'",
                                cancellationToken =>
                                {
                                    return RefactorAsync(
                                        context.Document,
                                        index,
                                        argument.WithExpression(defaultValue),
                                        cancellationToken);
                                });
                        }
                    }
                }
            }
        }

        private async Task<Document> RefactorAsync(
            Document document,
            int index,
            ArgumentSyntax newArgument,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var info = new RewriterInfo<ArgumentSyntax>(
                List[index],
                newArgument,
                GetTokenBefore(index),
                GetTokenAfter(index));

            SyntaxNode newRoot = root.ReplaceNode(ListSyntax, base.Rewrite(info));

            return document.WithSyntaxRoot(newRoot);
        }

        public override SyntaxToken GetOpenParenToken()
        {
            return ListSyntax.OpenParenToken;
        }

        public override SyntaxToken GetCloseParenToken()
        {
            return ListSyntax.CloseParenToken;
        }

        protected override NodeSyntaxRewriter<ArgumentSyntax> GetRewriter(RewriterInfo<ArgumentSyntax> info)
        {
            return new ArgumentSyntaxRewriter(info);
        }
    }
}
