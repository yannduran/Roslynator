﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    public static class IntroduceFieldToLockOnRefactoring
    {
        private const string LockObjectName = "_lockObject";

        public static async Task<Document> RefactorAsync(
            Document document,
            LockStatementSyntax lockStatement,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (lockStatement == null)
                throw new ArgumentNullException(nameof(lockStatement));

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            MemberDeclarationSyntax containingMember = lockStatement.FirstAncestorOrSelf<MemberDeclarationSyntax>();

            if (containingMember != null)
            {
                var containingDeclaration = (MemberDeclarationSyntax)containingMember
                    .Ancestors()
                    .FirstOrDefault(f => f.IsKind(
                        SyntaxKind.ClassDeclaration,
                        SyntaxKind.InterfaceDeclaration,
                        SyntaxKind.StructDeclaration));

                if (containingDeclaration != null)
                {
                    SyntaxList<MemberDeclarationSyntax> members = containingDeclaration.GetMembers();

                    int index = members.IndexOf(containingMember);

                    string name = LockObjectName;

                    if (document.SupportsSemanticModel)
                    {
                        SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
                        name = SyntaxUtility.GetUniqueName(name, semanticModel, lockStatement.Expression.Span.Start);
                    }

                    LockStatementSyntax newLockStatement = lockStatement
                        .WithExpression(IdentifierName(name));

                    MemberDeclarationSyntax newContainingMember = containingMember
                        .ReplaceNode(lockStatement, newLockStatement);

                    bool isStatic = containingMember.GetModifiers().Contains(SyntaxKind.StaticKeyword);

                    FieldDeclarationSyntax field = CreateFieldDeclaration(name, isStatic).WithFormatterAnnotation();

                    SyntaxList<MemberDeclarationSyntax> newMembers = members
                        .Replace(members[index], newContainingMember)
                        .Insert(GetLastFieldIndex(members, index) + 1, field);

                    MemberDeclarationSyntax newNode = containingDeclaration.SetMembers(newMembers);

                    SyntaxNode newRoot = root.ReplaceNode(containingDeclaration, newNode);

                    return document.WithSyntaxRoot(newRoot);
                }
            }

            return document;
        }

        private static int GetLastFieldIndex(SyntaxList<MemberDeclarationSyntax> members, int index)
        {
            for (int i = index; i >= 0; i--)
            {
                if (members[i].IsKind(SyntaxKind.FieldDeclaration))
                    return i;
            }

            return -1;
        }

        private static FieldDeclarationSyntax CreateFieldDeclaration(string name, bool isStatic)
        {
            return FieldDeclaration(
                (isStatic) ? Modifiers.PrivateStaticReadOnly() : Modifiers.PrivateReadOnly(),
                ObjectType(),
                Identifier(name).WithRenameAnnotation(),
                ObjectCreationExpression(ObjectType(), ArgumentList()));
        }
    }
}
