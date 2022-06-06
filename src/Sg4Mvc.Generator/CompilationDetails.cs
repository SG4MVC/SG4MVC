using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sg4Mvc.Generator;

public record CompilationDetails(
    ClassDeclarationSyntax ClassDeclarationSyntax,
    INamedTypeSymbol Symbol)
{
    public INamedTypeSymbol Symbol { get; } = Symbol;
    public ClassDeclarationSyntax ClassDeclarationSyntax { get; } = ClassDeclarationSyntax;
}
