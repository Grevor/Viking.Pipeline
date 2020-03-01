using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Viking.Pipeline.FrameworkWpf
{
    public class WpfPipelineProperty<TType> : IWpfPipelineReadonlyProperty<TType>
    {
        public WpfPipelineProperty
            (
            IPropertyChangeRaiser propertyChanged,
            Expression<Func<TType>> propertyExpression,
            AssignablePipelineStage<TType>? setter
            )
            : this(propertyChanged, propertyExpression, setter, setter)
        { }

        public WpfPipelineProperty
            (
            IPropertyChangeRaiser propertyChanged,
            Expression<Func<TType>> propertyExpression,
            IPipelineStage<TType> getter,
            AssignablePipelineStage<TType>? setter
            )
        {
            PropertyChanged = propertyChanged ?? throw new ArgumentNullException(nameof(propertyChanged));
            PropertyName = GetPropertyNameFromExpression(propertyExpression);
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
            Setter = setter;

            Reaction = getter.CreateReaction(RaisePropertyChangedOnValueChanged);
        }

        private IPropertyChangeRaiser PropertyChanged { get; }
        private string PropertyName { get; }
        private IPipelineStage<TType> Getter { get; }
        private AssignablePipelineStage<TType>? Setter { get; }
        private IPipelineStage Reaction { get; }

        public TType Get() => Getter.GetValue();
        public void Set(TType newValue)
        {
            if (Setter == null)
                throw new InvalidOperationException("Setting was attempted on a read-only property");

            Setter.SetValue(newValue);
        }

        private void RaisePropertyChangedOnValueChanged() => PropertyChanged.RaisePropertyChanged(PropertyName);

        private static string GetPropertyNameFromExpression(Expression<Func<TType>> propertyExpression)
        {
            if (!(propertyExpression.Body is MemberExpression member))
                throw new ArgumentException("Expression must be a property access expression.", nameof(propertyExpression));

            if (!(member.Member is PropertyInfo property))
                throw new ArgumentException("Expression must be a property access expression.", nameof(propertyExpression));

            return property.Name;
        }
    }

    public static class WpfPipelineProperty
    {

        #region Bog Standard Constructors
        public static WpfPipelineProperty<T> Create<T>
            (
            IPropertyChangeRaiser raiser,
            Expression<Func<T>> propertyExpression,
            AssignablePipelineStage<T>? pipelineStage
            )
        {
            return new WpfPipelineProperty<T>(raiser, propertyExpression, pipelineStage, pipelineStage);
        }

        public static WpfPipelineProperty<T> Create<T>
            (
            IPropertyChangeRaiser raiser,
            Expression<Func<T>> propertyExpression,
            IPipelineStage<T> getter,
            AssignablePipelineStage<T>? setter
            )
        {
            return new WpfPipelineProperty<T>(raiser, propertyExpression, getter, setter);
        }

        #endregion

        public static WpfPipelineProperty<T> CreateWithEqualityCheck<T>
            (
            IPropertyChangeRaiser raiser,
            Expression<Func<T>> propertyExpression,
            IPipelineStage<T> getter,
            AssignablePipelineStage<T>? setter
            ) 
            => CreateWithEqualityCheck(raiser, propertyExpression, getter, setter, EqualityComparer<T>.Default);

        public static WpfPipelineProperty<T> CreateWithEqualityCheck<T>
            (
            IPropertyChangeRaiser raiser,
            Expression<Func<T>> propertyExpression,
            IPipelineStage<T> getter,
            AssignablePipelineStage<T>? setter,
            IEqualityComparer<T> comparer
            )
        {
            var equalityComparison = getter.WithEqualityCheck(comparer);
            return new WpfPipelineProperty<T>(raiser, propertyExpression, equalityComparison, setter);
        }

        public static WpfPipelineProperty<T> CreateWithEqualityCheck<T>
            (
            IPropertyChangeRaiser raiser,
            Expression<Func<T>> propertyExpression,
            IPipelineStage<T> getter,
            AssignablePipelineStage<T>? setter,
            EqualityCheck<T> comparer
            )
        {
            var equalityComparison = getter.WithEqualityCheck(comparer);
            return new WpfPipelineProperty<T>(raiser, propertyExpression, equalityComparison, setter);
        }

        public static IWpfPipelineReadonlyProperty<T> CreateReadonly<T>
            (
            IPropertyChangeRaiser raiser,
            Expression<Func<T>> propertyExpression,
            IPipelineStage<T> pipelineStage
            )
        {
            return new WpfPipelineProperty<T>(raiser, propertyExpression, pipelineStage, null);
        }

        public static IWpfPipelineReadonlyProperty<T> CreateReadonlyWithEqualityCheck<T>
            (
            IPropertyChangeRaiser raiser,
            Expression<Func<T>> propertyExpression,
            IPipelineStage<T> pipelineStage
            )
            => CreateReadonlyWithEqualityCheck(raiser, propertyExpression, pipelineStage, EqualityComparer<T>.Default);

        public static IWpfPipelineReadonlyProperty<T> CreateReadonlyWithEqualityCheck<T>
            (
            IPropertyChangeRaiser raiser,
            Expression<Func<T>> propertyExpression,
            IPipelineStage<T> pipelineStage,
            IEqualityComparer<T> comparer
            )
        {
            var equalityComparison = pipelineStage.WithEqualityCheck(comparer);
            return new WpfPipelineProperty<T>(raiser, propertyExpression, equalityComparison, null);
        }

        public static IWpfPipelineReadonlyProperty<T> CreateReadonlyWithEqualityCheck<T>
            (
            IPropertyChangeRaiser raiser,
            Expression<Func<T>> propertyExpression,
            IPipelineStage<T> pipelineStage,
            EqualityCheck<T> comparer
            )
        {
            var equalityComparison = pipelineStage.WithEqualityCheck(comparer);
            return new WpfPipelineProperty<T>(raiser, propertyExpression, equalityComparison, null);
        }
    }
}
