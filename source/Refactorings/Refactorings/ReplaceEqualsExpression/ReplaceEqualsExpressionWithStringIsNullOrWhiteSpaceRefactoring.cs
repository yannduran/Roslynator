﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.ReplaceEqualsExpression
{
    internal class ReplaceEqualsExpressionWithStringIsNullOrWhiteSpaceRefactoring : ReplaceEqualsExpressionRefactoring
    {
        public override string MethodName
        {
            get { return "IsNullOrWhiteSpace"; }
        }
    }
}