using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Sg4Mvc.Generator.CodeGen;

public static class SimpleLiteral
{
    public static ExpressionSyntax Null => LiteralExpression(SyntaxKind.NullLiteralExpression);
    public static ExpressionSyntax True => LiteralExpression(SyntaxKind.TrueLiteralExpression);
    public static ExpressionSyntax False => LiteralExpression(SyntaxKind.FalseLiteralExpression);
    public static ExpressionSyntax EmptyString => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(""));
    public static ExpressionSyntax Space => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(" "));

    public static ExpressionSyntax String(String value) => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
}
