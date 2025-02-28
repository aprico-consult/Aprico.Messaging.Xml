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

namespace Aprico.Extensions;

/// <summary>Provides extension methods for retrieving XML-related information for types used in XML message contracts.</summary>
/// <remarks>
/// This static class offers utility methods to extract and generate fully qualified XML names based on XML root
/// attributes for types used in XML serialization and messaging scenarios.
/// </remarks>
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public static class TypeExtensions
{
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
	/// <see cref="IsXmlPartiallyQualifiedName"/> returns <see langword="false"/>, it generates the name using the format
	/// <c>{Namespace}#{ExplicitElementName}</c>.
	/// </item>
	/// <item>
	/// When <see cref="XmlRootAttribute"/> has a namespace but no explicit root element name, i.e. when
	/// <see cref="IsXmlPartiallyQualifiedName"/> returns <see langword="true"/>, it generates the name using the format
	/// <c>{Namespace}#{TypeName}</c>, thereby reducing the likelihood of naming conflicts among types sharing the same XML namespace.
	/// </item>
	/// </list>
	/// </remarks>
	public static string GetXmlFullyQualifiedName(this Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		var xmlRootAttribute = type.GetXmlRootAttribute();
		return xmlRootAttribute.IsXmlPartiallyQualifiedName()
			? $"{xmlRootAttribute.Namespace}#{type.Name}"
			: $"{xmlRootAttribute.Namespace}#{xmlRootAttribute.ElementName}";
	}

	/// <summary>Retrieves the XmlRootAttribute for the specified type.</summary>
	/// <param name="type">The type from which to extract the XmlRootAttribute.</param>
	/// <returns>The XmlRootAttribute associated with the type.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the input type is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown if no XmlRootAttribute is found.</exception>
	/// <remarks>
	/// This method extracts the XmlRootAttribute without considering inherited attributes. If no attribute is found, it
	/// throws an InvalidOperationException.
	/// </remarks>
	public static XmlRootAttribute GetXmlRootAttribute(this Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		return type.GetCustomAttribute<XmlRootAttribute>(inherit: false) ?? throw new InvalidOperationException($"Type '{type.FullName}' has no {nameof(XmlRootAttribute)} qualifier.");
	}

	/// <summary>Determines if the XmlRootAttribute represents a partially qualified XML name.</summary>
	/// <param name="attribute">The XmlRootAttribute to check.</param>
	/// <returns>
	/// <see langword="true"/> if the attribute has a namespace but no explicit element name; otherwise,
	/// <see langword="false"/>.
	/// </returns>
	/// <remarks>A partially qualified name is defined as having a non-empty namespace but an empty or null element name.</remarks>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
	public static bool IsXmlPartiallyQualifiedName(this XmlRootAttribute attribute)
	{
		ArgumentNullException.ThrowIfNull(attribute);
		return !attribute.Namespace.IsNullOrWhiteSpace() && attribute.ElementName.IsNullOrWhiteSpace();
	}
}
