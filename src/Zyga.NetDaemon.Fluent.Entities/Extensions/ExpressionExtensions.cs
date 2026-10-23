using System.Linq.Expressions;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Extensions
{
    /// <summary>
    /// Extension methods that pull the <see cref="PropertyInfo"/> or <see cref="MethodInfo"/> a lambda
    /// expression refers to, used by <see cref="Maps.DeviceMap{TDevice}"/>'s <c>Map(...)</c> overloads to
    /// turn a member-access expression into reflection metadata.
    /// </summary>
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Extracts the <see cref="PropertyInfo"/> referenced by a property-access expression (e.g. <c>x =&gt; x.Foo</c>),
        /// unwrapping a boxing/conversion <see cref="UnaryExpression"/> when the property type needs one.
        /// </summary>
        /// <param name="propertyExpression">The property-access expression.</param>
        /// <exception cref="ArgumentException">Thrown when the expression does not refer to a property.</exception>
        public static PropertyInfo GetProperty<TTarget, TProp>(this Expression<Func<TTarget, TProp>> propertyExpression)
        {
            MemberExpression Exp;

            if (propertyExpression.Body is UnaryExpression)
            {
                UnaryExpression UnExp = (UnaryExpression)propertyExpression.Body;
                if (UnExp.Operand is MemberExpression)
                {
                    Exp = (MemberExpression)UnExp.Operand;
                }
                else
                    throw new ArgumentException();
            }
            else if (propertyExpression.Body is MemberExpression)
            {
                Exp = (MemberExpression)propertyExpression.Body;
            }
            else
            {
                throw new ArgumentException();
            }

            return (PropertyInfo)Exp.Member;
        }

        /// <summary>Extracts the <see cref="MethodInfo"/> referenced by a parameterless action method-call expression (e.g. <c>x =&gt; x.Foo()</c>).</summary>
        /// <param name="methodExpression">The method-call expression.</param>
        /// <exception cref="ArgumentException">Thrown when the expression does not represent a method call.</exception>
        public static MethodInfo GetMethodInfo<TTarget>(this Expression<Action<TTarget>> methodExpression) => GetMethodInfoFromExpressionBody(methodExpression.Body);

        /// <summary>Extracts the <see cref="MethodInfo"/> referenced by a parameterless method-call expression returning <typeparamref name="TResult"/> (e.g. <c>x =&gt; x.Foo()</c>).</summary>
        /// <param name="methodExpression">The method-call expression.</param>
        /// <exception cref="ArgumentException">Thrown when the expression does not represent a method call.</exception>
        public static MethodInfo GetMethodInfo<TTarget, TResult>(this Expression<Func<TTarget, TResult>> methodExpression) => GetMethodInfoFromExpressionBody(methodExpression.Body);

        /// <summary>Extracts the <see cref="MethodInfo"/> referenced by a single-parameter method-call expression (e.g. <c>(x, arg) =&gt; x.Foo(arg)</c>).</summary>
        /// <param name="methodExpression">The method-call expression.</param>
        /// <exception cref="ArgumentException">Thrown when the expression does not represent a method call.</exception>
        public static MethodInfo GetMethodInfo<TTarget, TArg, TResult>(this Expression<Func<TTarget, TArg, TResult>> methodExpression) => GetMethodInfoFromExpressionBody(methodExpression.Body);

        private static MethodInfo GetMethodInfoFromExpressionBody(Expression body)
        {
            if (body is MethodCallExpression methodCall)
                return methodCall.Method;

            throw new ArgumentException("Expression does not represent a method call");
        }
    }
}