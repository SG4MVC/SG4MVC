using System;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Services.Interfaces;

public interface IFilePersistService
{
    void WriteFile(SyntaxNode fileTree, String filePath);
}
