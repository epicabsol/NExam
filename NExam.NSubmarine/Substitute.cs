using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace NExam.NSubmarine
{
    internal class ArrayEqualityComparer : IEqualityComparer<object[]>
    {
        public bool Equals(object[] left, object[] right)
        {
            IStructuralEquatable equatable = left;
            return equatable.Equals(right, EqualityComparer<object>.Default);
        }

        public int GetHashCode([DisallowNull] object[] obj)
        {
            IStructuralEquatable equatable = obj;
            int hash = equatable.GetHashCode(EqualityComparer<object>.Default);
            System.Diagnostics.Debug.WriteLine($"Array hash of {String.Join(',', obj)} is {hash}");
            return hash;
        }
    }

    public class Substitute<T>
    {
        private class StandIn : DynamicObject
        {
            public Substitute<T> Substitute { get; }

            public StandIn(Substitute<T> substitute)
            {
                this.Substitute = substitute;
            }

            /*public override bool TryConvert(ConvertBinder binder, out object result)
            {
                if (binder.Type == typeof(T))
                {
                    result = ImpromptuInterface.Impromptu.ActLike(this, typeof(T));
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }*/

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                // TODO: Implement!
                throw new NotImplementedException();
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                // TODO: Implement!
                throw new NotImplementedException();
            }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                MethodInfo info = Substitute.FindMethodInfo(binder.Name, args.Select(arg => arg.GetType()).ToArray());
                //MethodBase method = Type.DefaultBinder.SelectMethod(BindingFlags.Public, typeof(T).GetMethods().Where(m => m.Name == binder.Name).ToArray(), args.Select(arg => arg.GetType()).ToArray(), null);
                //MethodInfo info = typeof(T).GetMethod(binder.Name, BindingFlags.Public, new Binder(), args.Select(arg => arg.GetType()).ToArray(), null);
                //MethodInfo info = method.
                //MethodInfo info = method as MethodInfo; // We can assume this because no testable code will be calling the constructor of a mocked instance that already exists.
                if (info == null)
                {
                    // No method with the given name exists on T.
                    // This should never happen - the calling code wouldn't have compiled.
                    throw new Exception("NSubmarine Substitute: Can't invoke a method that doesn't exist in the original type.");
                }
                else if (Substitute.MethodBindings.TryGetValue(info, out MethodBinding binding))
                {
                    result = binding.Invoke(args);
                    return true;
                }
                else
                {
                    // No method with the given name existed on T when the substitute was created.
                    // This should never happen - the calling code wouldn't have compiled.
                    throw new Exception("NSubmarine Substitute: Can't invoke a method that doesn't exist in the original type.");
                }
            }
        }

        private class MethodBinding
        {
            public MethodInfo Method { get; }

            public Func<object[], object> DefaultHandler { get; set; }
            public Dictionary<object[], Func<object>> CaseHandlers { get; }

            public MethodBinding(MethodInfo method, Func<object[], object> defaultHandler)
            {
                this.Method = method;
                this.DefaultHandler = defaultHandler;
                this.CaseHandlers = new Dictionary<object[], Func<object>>(new ArrayEqualityComparer());
            }

            public object Invoke(object[] arguments)
            {
                if (CaseHandlers.TryGetValue(arguments, out Func<object> handler))
                {
                    return handler();
                }
                else
                {
                    return DefaultHandler(arguments);
                }
            }
        }

        private StandIn Mock;
        public T MockObject => ImpromptuInterface.Impromptu.ActLike(Mock);

        private Dictionary<MethodInfo, MethodBinding> MethodBindings { get; } = new Dictionary<MethodInfo, MethodBinding>();

        public Substitute()
        {
            Type targetType = typeof(T);

            foreach (MethodInfo method in targetType.GetMethods())
            {
                MethodBindings.Add(method, new MethodBinding(method, arguments => this.HandleUnhandledMethod(method, arguments)));
            }

            this.Mock = new StandIn(this);
        }

        private object HandleUnhandledMethod(MethodInfo method, object[] arguments)
        {
            throw new Exception($"NSubmarine Substitute: Method {method.DeclaringType}.{method.Name} called when not handled.");
        }

        private MethodInfo FindMethodInfo(string methodName, Type[] argumentTypes)
        {
            return Type.DefaultBinder.SelectMethod(BindingFlags.Public, typeof(T).GetMethods().Where(m => m.Name == methodName).ToArray(), argumentTypes, null) as MethodInfo;
        }

        private void SetMethodHandler(string methodName, object[] args, Func<object> handler)
        {
            MethodInfo info = FindMethodInfo(methodName, args.Select(arg => arg.GetType()).ToArray());

            if (info == null)
            {
                throw new ArgumentException($"Method '{methodName}' doesn't exist on type {typeof(T)}.");
            }

            MethodBindings[info].CaseHandlers[args] = handler;
        }

        private void SetDefaultMethodHandler(string methodName, Type[] parameterTypes, Func<object[], object> wrappedHandler)
        {
            MethodInfo info = FindMethodInfo(methodName, parameterTypes);

            if (info == null)
            {
                throw new ArgumentException($"Method '{methodName}' doesn't exist on type {typeof(T)}.");
            }

            MethodBindings[info].DefaultHandler = wrappedHandler;
        }

        #region User methods
        public void SetMethodHandler<TResult>(string methodName, Func<TResult> handler)
        {
            this.SetDefaultMethodHandler(methodName, Type.EmptyTypes, (args) => handler());
        }

        // Methods for setting method handlers for specific argument values
        public void SetMethodHandler<TResult, TParam0>(string methodName, TParam0 argument0, Func<TResult> handler)
        {
            this.SetMethodHandler(methodName, new object[] { argument0 }, (Func<object>)(() => handler()));
        }

        public void SetMethodHandler<TResult, TParam0, TParam1>(string methodName, TParam0 argument0, TParam1 argument1, Func<TResult> handler)
        {
            this.SetMethodHandler(methodName, new object[] { argument0, argument1 }, (Func<object>)(() => handler()));
        }

        // Methods for setting the default method handlers
        public void SetMethodDefaultHandler<TResult, TParam0>(string methodName, Func<TParam0, TResult> handler)
        {
            this.SetDefaultMethodHandler(methodName, new Type[] { typeof(TParam0) }, args => handler((TParam0)args[0]));
        }

        public void SetMethodDefaultHandler<TResult, TParam0, TParam1>(string methodName, Func<TParam0, TParam1, TResult> handler)
        {
            this.SetDefaultMethodHandler(methodName, new Type[] { typeof(TParam0), typeof(TParam1) }, args => handler((TParam0)args[0], (TParam1)args[1]));
        }
        #endregion

        public static implicit operator T(Substitute<T> substitute)
        {
            return (dynamic)substitute.Mock;
        }
    }
}
