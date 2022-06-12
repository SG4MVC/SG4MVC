using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.CodeGen;

namespace Sg4Mvc.Generator.Controllers.Interfaces;

public interface IControllerGeneratorService
{
    String GetControllerArea(INamedTypeSymbol controllerSymbol);
    ClassDeclarationSyntax GeneratePartialController(ControllerDefinition controller);
    ClassDeclarationSyntax GenerateSg4Controller(ControllerDefinition controller);
    ClassBuilder WithViewsClass(ClassBuilder classBuilder, List<View> viewFiles);
}
