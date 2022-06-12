using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Sg4Mvc.Generator.CodeGen;

public class BodyBuilder
{
    private IList<StatementSyntax> _expressions = new List<StatementSyntax>();

    private static ArgumentListSyntax GetArguments(ICollection<Object> arguments)
    {
        var result = arguments.Select(a =>
            {
                switch (a)
                {
                    case String argumentName:
                        return IdentifierName(argumentName);
                    case ExpressionSyntax argumentExpression:
                        return argumentExpression;
                    default:
                        throw new InvalidOperationException("Argument of wrong type passed. Has to be String or ExpressionSyntax");
                }
            })
            .Select(Argument)
            .ToArray();
        return ArgumentList(SeparatedList(result));
    }

    public static ExpressionSyntax MethodCallExpression(String entityName, String methodName, ICollection<Object> arguments)
    {
        var methodCallExpression = entityName != null
            ? InvocationExpression(SyntaxNodeHelpers.MemberAccess(entityName, methodName))
            : InvocationExpression(IdentifierName(methodName));
        if (arguments?.Count > 0)
        {
            methodCallExpression = methodCallExpression.WithArgumentList(GetArguments(arguments));
        }
        else
        {
            methodCallExpression = methodCallExpression.WithArgumentList(ArgumentList());
        }

        return methodCallExpression;
    }

    private ExpressionSyntax NewObjectExpression(String entityType, ICollection<Object> arguments)
    {
        var newExpression = ObjectCreationExpression(IdentifierName(entityType));
        if (arguments?.Count > 0)
        {
            newExpression = newExpression.WithArgumentList(GetArguments(arguments));
        }
        else
        {
            newExpression = newExpression.WithArgumentList(ArgumentList());
        }

        return newExpression;
    }

    public BodyBuilder MethodCall(String entityName, String methodName, params Object[] arguments)
    {
        var methodCallExpression = MethodCallExpression(entityName, methodName, arguments);
        _expressions.Add(ExpressionStatement(methodCallExpression));
        return this;
    }

    public BodyBuilder ReturnMethodCall(String entityName, String methodName, params Object[] arguments)
    {
        var methodCallExpression = MethodCallExpression(entityName, methodName, arguments);
        _expressions.Add(ReturnStatement(methodCallExpression));
        return this;
    }

    public BodyBuilder ReturnNewObject(String entityType, params Object[] arguments)
    {
        var newExpression = NewObjectExpression(entityType, arguments);
        _expressions.Add(ReturnStatement(newExpression));
        return this;
    }

    public BodyBuilder ReturnVariable(String variableName)
    {
        _expressions.Add(ReturnStatement(IdentifierName(variableName)));
        return this;
    }

    private VariableDeclarationSyntax NewVariableDeclaration(String name, ExpressionSyntax value, String type = null)
    {
        return VariableDeclaration(IdentifierName(type ?? "var"))
            .WithVariables(
                SingletonSeparatedList(
                    VariableDeclarator(Identifier(name))
                        .WithInitializer(
                            EqualsValueClause(value))));
    }

    public BodyBuilder VariableFromMethodCall(String variableName, String entityName, String methodName, params Object[] arguments)
    {
        var methodCallExpression = MethodCallExpression(entityName, methodName, arguments);
        var variableExpression = NewVariableDeclaration(variableName, methodCallExpression);
        _expressions.Add(LocalDeclarationStatement(variableExpression));
        return this;
    }

    public BodyBuilder VariableFromNewObject(String variableName, String entityType, params Object[] arguments)
    {
        var newExpression = NewObjectExpression(entityType, arguments);
        var variableExpression = NewVariableDeclaration(variableName, newExpression);
        _expressions.Add(LocalDeclarationStatement(variableExpression));
        return this;
    }

    public BodyBuilder Statement(Func<BodyBuilder, BodyBuilder> statement)
    {
        statement(this);
        return this;
    }

    public BodyBuilder ForEach<TEntity>(IEnumerable<TEntity> items, Action<BodyBuilder, TEntity> action)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                action(this, item);
            }
        }

        return this;
    }

    public BlockSyntax Build()
    {
        return Block(_expressions.ToArray());
    }
}
