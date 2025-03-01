#region Copyright & License

// Copyright © 2024 - 2025 Aprico Consultants
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Serialization;
using Be.Stateless.Extensions;

// @formatter:wrap_chained_method_calls chop_if_long
namespace Aprico.Extensions;

/// <summary>Provides extension methods for retrieving XML-related information for types used in XML message contracts.</summary>
/// <remarks>
/// This static class offers utility methods to extract and generate fully qualified XML names based on XML root
/// attributes for types used in XML serialization and messaging scenarios.
/// </remarks>
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public static class TypeXmlExtensions
{
	/// <summary>Retrieves the XML root attribute for the specified type, throwing an exception if no attribute is found.</summary>
	/// <param name="type">The type to retrieve the XML root attribute from.</param>
	/// <returns>The <see cref="XmlRootAttribute"/> associated with the type.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the input <paramref name="type"/> is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown when no <see cref="XmlRootAttribute"/> is defined for the type.</exception>
	/// <remarks>
	/// This method ensures that an <see cref="XmlRootAttribute"/> is present for the given type, without considering
	/// inherited attributes. If no attribute is found, it throws an <see cref="InvalidOperationException"/> with a descriptive
	/// message.
	/// </remarks>
	public static XmlRootAttribute GetRequiredXmlRootAttribute(this Type type)
	{
		return type.GetXmlRootAttribute().UnlessIsNull($"The type '{type.FullName}' must be decorated with an {nameof(XmlRootAttribute)}.");
	}

	/// <summary>Generates a fully qualified XML name for the given type based on its XML root attribute.</summary>
	/// <param name="type">The type for which to generate the XML fully qualified name.</param>
	/// <returns>A string representing the fully qualified XML name of the type.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the input type is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the type lacks an XmlRootAttribute.</exception>
	/// <remarks>
	/// <para>
	/// The method constructs the XML name using the namespace and either the type name or the explicitly defined element name
	/// from the XmlRootAttribute. The naming convention uses a '#' separator to combine the XML namespace with either the type name or
	/// a custom root element name, providing flexibility in XML serialization naming strategies.
	/// </para>
	/// <para>The method generates a fully qualified XML name based on two scenarios.</para>
	/// <list type="number">
	/// <item>
	/// When <see cref="XmlRootAttribute"/> has both a namespace and an explicit root element name, i.e. when
	/// <see cref="HasXmlPartiallyQualifiedName"/> returns <see langword="false"/>, it generates the name using the format
	/// <c>{Namespace}#{ExplicitElementName}</c>.
	/// </item>
	/// <item>
	/// When <see cref="XmlRootAttribute"/> has a namespace but no explicit root element name, i.e. when
	/// <see cref="HasXmlPartiallyQualifiedName"/> returns <see langword="true"/>, it generates the name using the format
	/// <c>{Namespace}#{TypeName}</c>, thereby reducing the likelihood of naming conflicts among types sharing the same XML namespace.
	/// </item>
	/// </list>
	/// </remarks>
	[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Validated by nested call to GetXmlRootAttribute.")]
	public static string GetXmlFullyQualifiedName(this Type type)
	{
		var xmlRootAttribute = type.GetRequiredXmlRootAttribute();
		var xmlNamespace = xmlRootAttribute.Namespace.UnlessIsNullOrEmpty($"The {nameof(XmlRootAttribute)} decorating the type '{type.FullName}' must specify an XML namespace.");
		return xmlRootAttribute.IsXmlPartiallyQualifiedName()
			? $"{xmlNamespace}#{type.Name}"
			: $"{xmlNamespace}#{xmlRootAttribute.ElementName}";
	}

	/// <summary>Retrieves the <see cref="XmlRootAttribute"/> for the specified type.</summary>
	/// <param name="type">The type from which to extract the <see cref="XmlRootAttribute"/>.</param>
	/// <returns>The <see cref="XmlRootAttribute"/> associated with the type.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the input type is null.</exception>
	/// <remarks>This method extracts the <see cref="XmlRootAttribute"/> without considering inherited attributes.</remarks>
	public static XmlRootAttribute? GetXmlRootAttribute(this Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		return type.GetCustomAttribute<XmlRootAttribute>(inherit: false);
	}

	/// <summary>Determines whether the specified type has a partially qualified XML name.</summary>
	/// <param name="type">The type to check for a partially qualified XML name.</param>
	/// <returns><see langword="true"/> if the type has a partially qualified XML name; otherwise, <see langword="false"/>.</returns>
	/// <remarks>
	/// A partially qualified XML name is characterized by an <see cref="XmlRootAttribute"/> that defines a namespace but
	/// lacks a specific element name.
	/// </remarks>
	public static bool HasXmlPartiallyQualifiedName(this Type type)
	{
		return type.GetXmlRootAttribute()?.IsXmlPartiallyQualifiedName() == true;
	}

	/// <summary>Determines if the XmlRootAttribute represents a partially qualified XML name.</summary>
	/// <param name="attribute">The XmlRootAttribute to check.</param>
	/// <returns>
	/// <see langword="true"/> if the attribute has a namespace but no explicit element name; otherwise,
	/// <see langword="false"/>.
	/// </returns>
	/// <remarks>A partially qualified name is defined as having a non-empty namespace, but an empty or null element name.</remarks>
	private static bool IsXmlPartiallyQualifiedName(this XmlRootAttribute attribute)
	{
		return !attribute.Namespace.IsNullOrWhiteSpace() && attribute.ElementName.IsNullOrWhiteSpace();
	}
}
