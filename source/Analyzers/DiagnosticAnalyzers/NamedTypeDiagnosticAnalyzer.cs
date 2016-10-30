﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Pihrtsoft.CodeAnalysis.CSharp.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NamedTypeDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.RemovePartialModifierFromTypeWithSinglePart,
                    DiagnosticDescriptors.DeclareTypeInsideNamespace);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.RegisterSymbolAction(f => AnalyzeNamedType(f), SymbolKind.NamedType);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            if (GeneratedCodeAnalyzer?.IsGeneratedCode(context) == true)
                return;

            var symbol = (INamedTypeSymbol)context.Symbol;

            TypeKind kind = symbol.TypeKind;

            if (kind == TypeKind.Class
                || kind == TypeKind.Struct
                || kind == TypeKind.Interface)
            {
                ImmutableArray<SyntaxReference> syntaxReferences = symbol.DeclaringSyntaxReferences;

                if (syntaxReferences.Length == 1)
                {
                    var declaration = syntaxReferences[0].GetSyntax(context.CancellationToken) as MemberDeclarationSyntax;

                    if (declaration != null)
                    {
                        SyntaxToken partialToken = declaration.GetModifiers()
                            .FirstOrDefault(f => f.IsKind(SyntaxKind.PartialKeyword));

                        if (partialToken.IsKind(SyntaxKind.PartialKeyword))
                        {
                            context.ReportDiagnostic(
                                DiagnosticDescriptors.RemovePartialModifierFromTypeWithSinglePart,
                                partialToken.GetLocation());
                        }
                    }
                }
            }

            if (symbol.ContainingNamespace?.IsGlobalNamespace == true)
            {
                ImmutableArray<SyntaxReference> syntaxReferences = symbol.DeclaringSyntaxReferences;

                foreach (SyntaxReference syntaxReference in syntaxReferences)
                {
                    SyntaxNode node = syntaxReference.GetSyntax(context.CancellationToken);

                    if (node != null)
                    {
                        SyntaxToken identifier = GetDeclarationIdentifier(kind, node);

                        if (!identifier.IsKind(SyntaxKind.None))
                        {
                            context.ReportDiagnostic(
                                DiagnosticDescriptors.DeclareTypeInsideNamespace,
                                identifier.GetLocation(),
                                identifier.ValueText);
                        }
                    }
                }
            }
        }

        private static SyntaxToken GetDeclarationIdentifier(TypeKind kind, SyntaxNode node)
        {
            switch (kind)
            {
                case TypeKind.Class:
                    return ((ClassDeclarationSyntax)node).Identifier;
                case TypeKind.Struct:
                    return ((StructDeclarationSyntax)node).Identifier;
                case TypeKind.Interface:
                    return ((InterfaceDeclarationSyntax)node).Identifier;
                case TypeKind.Delegate:
                    return ((DelegateDeclarationSyntax)node).Identifier;
                case TypeKind.Enum:
                    return ((EnumDeclarationSyntax)node).Identifier;
                default:
                    {
                        Debug.Assert(false, kind.ToString());
                        return default(SyntaxToken);
                    }
            }
        }
    }
}
