// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Pihrtsoft.CodeAnalysis.CSharp.Analyzers.Tests
{
    internal class DefaultSwitchLabelShouldBeLastLabelInSection
    {
        public static void Foo()
        {
            RegexOptions options = RegexOptions.None;

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    break;
                case RegexOptions.ECMAScript:
                case RegexOptions.ExplicitCapture:
                case RegexOptions.IgnoreCase:
                case RegexOptions.IgnorePatternWhitespace:
#if DEBUG
                default:
                case RegexOptions.Multiline:
                case RegexOptions.RightToLeft:
                case RegexOptions.Singleline:
#endif
                    {
                        break;
                    }
            }
        }
    }
}
