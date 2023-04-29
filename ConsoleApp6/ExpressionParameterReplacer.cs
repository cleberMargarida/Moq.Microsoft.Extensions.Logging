using System.Linq.Expressions;
using System.Reflection;

internal class ExpressionParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;
    private MemberExpression _member = default!;
    private ConstantExpression _constant = default!;

    public ExpressionParameterReplacer(Type parameterType)
    {
        _parameter = Expression.Parameter(parameterType, "x");
    }

    public Expression<Func<T, bool>> ReplaceParameter<T>(Expression<Func<object, bool>> expression)
    {
        VisitLambda(expression);
        var toString = typeof(object).GetMethod(nameof(object.ToString))!;
        var left = Expression.Call(_parameter, toString);
        var right = Expression.Call(_member, toString);
        var body = Expression.Equal(left, right);
        return Expression.Lambda<Func<T, bool>>(body, _parameter);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        base.VisitMember(node);

        //change the type from object to FormattedLogValues
        ((TypeInfo)((FieldInfo)node.Member)
            .GetType())
            .DeclaredFields
            .First(x => x.Name == "m_fieldType")!
            .SetValue((FieldInfo)node.Member, _parameter.Type);

        _member = Expression.MakeMemberAccess(_constant, (FieldInfo)node.Member);
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        _constant = node;
        return node;
    }
}
