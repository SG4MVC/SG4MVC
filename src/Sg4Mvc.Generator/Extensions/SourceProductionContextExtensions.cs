using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Extensions;

public static class SourceProductionContextExtensions
{
    public static void WriteFile(this SourceProductionContext context, SyntaxNode fileTree, String filePath)
    {
        context.AddSource(filePath, fileTree.GetText(Encoding.UTF8));
    }
}
