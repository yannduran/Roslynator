// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactorings.Tests
{
    public static class ReplaceExpressionWithConstantValueRefactoring
    {
        private const string StringNullConstant = null;
        private const bool BooleanConstant = true;
        private const char CharConstant = 'a';
        private const int Int32Constant = 1;

        private const string EmptyString = "";

        public static string Foo()
        {
            string s = EmptyString;

            string s2 = StringNullConstant;
            bool f = BooleanConstant;
            char ch = CharConstant;
            int i = Int32Constant;

            const string x = "x";
            string xx = x;

            return s;
        }
    }
}
