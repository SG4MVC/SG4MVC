using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Sg4Mvc.Generator.Services.Interfaces;

namespace Sg4Mvc.Generator.Services;

public class FilePersistService : IFilePersistService
{
    public FilePersistService(GeneratorExecutionContext context)
    {
        Context = context;
    }

    private GeneratorExecutionContext Context { get; }

    public void WriteFile(SyntaxNode fileTree, String filePath)
    {
        Context.AddSource(filePath, fileTree.GetText(Encoding.UTF8));
    }
}
