using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sg4Mvc.Generator.Services.Interfaces;

public interface IStaticFileGeneratorService
{
    MemberDeclarationSyntax GenerateStaticFiles(String projectRoot);
}
