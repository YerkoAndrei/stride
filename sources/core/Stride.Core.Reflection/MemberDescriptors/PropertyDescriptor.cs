// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Reflection;

namespace Stride.Core.Reflection;

/// <summary>
/// A <see cref="IMemberDescriptor"/> for a <see cref="PropertyInfo"/>
/// </summary>
public class PropertyDescriptor : MemberDescriptorBase
{
    private readonly MethodInfo? getMethod;
    private readonly MethodInfo? setMethod;

    public PropertyDescriptor(ITypeDescriptor typeDescriptor, PropertyInfo propertyInfo, StringComparer defaultNameComparer)
        : base(propertyInfo, defaultNameComparer)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);

        PropertyInfo = propertyInfo;

        getMethod = propertyInfo.GetMethod;
        setMethod = propertyInfo.SetMethod;

        TypeDescriptor = typeDescriptor;
    }

    /// <summary>
    /// Gets the property information attached to this instance.
    /// </summary>
    /// <value>The property information.</value>
    public PropertyInfo PropertyInfo { get; }

    public override Type Type => PropertyInfo.PropertyType;

    public sealed override bool IsPublic => getMethod?.IsPublic ?? false;

    public override bool HasSet => setMethod != null;

    public override object? Get(object? thisObject)
    {
        return getMethod?.Invoke(thisObject, null);
    }

    public override void Set(object? thisObject, object? value)
    {
        if (setMethod is null)
            throw new InvalidOperationException($"The property [{Name}] of type [{DeclaringType.Name}] has no setter.");

        setMethod.Invoke(thisObject, [value]);
    }

    public override IEnumerable<T> GetCustomAttributes<T>(bool inherit)
    {
        return PropertyInfo.GetCustomAttributes<T>(inherit);
    }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"Property [{Name}] from Type [{(PropertyInfo.DeclaringType != null ? PropertyInfo.DeclaringType.FullName : string.Empty)}]";
    }
}
