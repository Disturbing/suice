using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Got this from the following sites :
/// https://vagifabilov.wordpress.com/2010/04/02/dont-use-activator-createinstance-or-constructorinfo-invoke-use-compiled-lambda-expressions/
/// https://ayende.com/blog/3167/creating-objects-perf-implications
/// http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/
/// https://rboeije.wordpress.com/2012/04/19/extension-method-on-type-as-alternative-for-activator-createinstance/ - Implemented this
/// 
/// This extension is a benchmarked better implementation to Activator.CreateInstance when taking the second link into consideration
/// Here the expression is further cached so as to better instantiation for factory implementations
/// </summary>
internal static class InstanceCreatorExtensions
{
    internal static object New(this Type type, params object[] args)
    {
        Type[] argTypes = args.Select(a => a.GetType()).ToArray();

        var ctor = type.GetConstructor(argTypes);

        return GetActivator(ctor)(args);
    }

    internal delegate object ObjectActivator(params object[] args);

    private static Dictionary<ConstructorInfo, object> _activators = new Dictionary<ConstructorInfo, object>();

    internal static ObjectActivator GetActivator(ConstructorInfo ctor)
    {
        object activator;
        if (_activators.TryGetValue(ctor, out activator)) return (ObjectActivator)activator;

        ParameterInfo[] paramsInfo = ctor.GetParameters();

        //create a single param of type object[]
        ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

        var argsExp = new Expression[paramsInfo.Length];

        //pick each arg from the params array
        //and create a typed expression of them
        for (int i = 0; i < paramsInfo.Length; i++)
        {
            Expression index = Expression.Constant(i);

            Expression paramAccessorExp = Expression.ArrayIndex(param, index);

            Expression paramCastExp = Expression.Convert(paramAccessorExp, paramsInfo[i].ParameterType);

            argsExp[i] = paramCastExp;
        }

        //make a NewExpression that calls the ctor with the args we just created
        NewExpression newExp = Expression.New(ctor, argsExp);

        //create a lambda with the NewExpression as body and our param object[] as arg
        LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);

        //compile it
        var compiled = (ObjectActivator)lambda.Compile();

        _activators.Add(ctor, compiled);

        return compiled;
    }
}