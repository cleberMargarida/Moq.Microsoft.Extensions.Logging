using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Reflection;

internal class Log
{
    private static readonly Expression<Action<ILogger>> _baseLambda =
    x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
    );

    private static readonly MethodCallExpression _baseCall =
        (MethodCallExpression)_baseLambda.Body;

    private static readonly Expression information =
        Expression.Constant(LogLevel.Information);

    private static readonly Expression eventId =
        _baseCall.Arguments[1];

    private static readonly Expression exception =
        _baseCall.Arguments[3];

    private static readonly Expression func =
        _baseCall.Arguments[4];

    internal static Expression<Action<ILogger>> LogInformation(string message, params object[] args)
    {
        //FormattedLogValues
        var state = Assembly
            .GetAssembly(typeof(ILogger))!
            .GetTypes()
            .First(x => x.Name == "FormattedLogValues")
            .GetConstructors()![0]
            .Invoke(new object[] { message, args });

        var itIsForState = typeof(It)
            .GetMethods()
            .First(x => x.Name == nameof(It.Is))
            .MakeGenericMethod(state.GetType());

        var genericBody = (Expression<Func<object, bool>>)(x => x == state.ToString());
        var replacer = new ExpressionParameterReplacer(state.GetType());
        var typedBody = typeof(ExpressionParameterReplacer)
            .GetMethod(nameof(ExpressionParameterReplacer.ReplaceParameter))!
            .MakeGenericMethod(state.GetType())
            .Invoke(replacer, new[] { genericBody });

        var funcType = typeof(Func<,,>).MakeGenericType(state.GetType(), typeof(Exception), typeof(string));
        var itIsForFunc = typeof(It)
            .GetMethods()
            .First(x => x.Name == nameof(It.IsAny))
            .MakeGenericMethod(funcType);

        var logger = Expression.Parameter(typeof(ILogger), "x");
        var methodInfo = typeof(ILogger).GetMethod("Log")!.MakeGenericMethod(state.GetType());

        var call = Expression.Call(logger, methodInfo, new Expression[]
        {
            information,
            eventId,
            Expression.Call(itIsForState, arg0: (Expression)typedBody!),
            exception,
            Expression.Call(null, itIsForFunc)
        });

        return Expression.Lambda<Action<ILogger>>(call, logger);
    }
}
