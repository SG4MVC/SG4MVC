using System;

namespace Sg4Mvc;

// ReSharper disable once UnusedMember.Global
/// <summary>
/// Force exclusion of a class or action for SG4MVC code generation
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class GenerateSg4MvcAttribute : Attribute
{
}
