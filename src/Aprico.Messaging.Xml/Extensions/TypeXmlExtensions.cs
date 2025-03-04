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

/// <summary>Provides extension methods for obtaining XML-related information about XML contract types.</summary>
/// <remarks>
/// <para>
/// This static class contains utility methods for retrieving fully qualified XML names from XML contract types. It helps in
/// creating consistent and standardized XML naming for message contracts in distributed systems.
/// </para>
/// <para>
/// Technically, it generates fully qualified XML names based on <see cref="XmlRootAttribute"/> required to decorate types
/// meant to be used in messaging scenarios relying on XML serialization.
/// </para>
/// </remarks>
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public static class TypeXmlExtensions
{
	/// <summary>
	/// Retrieves the <see cref="XmlRootAttribute"/> decorating the given <paramref name="type"/>, throwing an exception if
	/// not found.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> from which to extract the <see cref="XmlRootAttribute"/>.</param>
	/// <returns>The <see cref="XmlRootAttribute"/> instance associated with the given <paramref name="type"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the input <paramref name="type"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">
	/// Thrown if no <see cref="XmlRootAttribute"/> decorates the given
	/// <paramref name="type"/>.
	/// </exception>
	/// <remarks>This method extracts the <see cref="XmlRootAttribute"/> without considering inherited attributes.</remarks>
	public static XmlRootAttribute GetRequiredXmlRootAttribute(this Type type)
	{
		return type.GetXmlRootAttribute().UnlessIsNull($"The type '{type.FullName}' must be decorated with an {nameof(XmlRootAttribute)}.");
	}

	/// <summary>
	/// Generates a fully qualified XML name for the given <paramref name="type"/> based on the <see cref="XmlRootAttribute"/>
	/// decorating it.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> for which to generate the XML fully qualified name.</param>
	/// <returns>A string representing the fully qualified XML name of the given <paramref name="type"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the input <see cref="Type"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the type is not decorated with an <see cref="XmlRootAttribute"/>.</exception>
	/// <remarks>
	/// <para>
	/// The naming convention uses a <c>#</c> separator to combine the XML <see cref="XmlRootAttribute.Namespace"/> with either
	/// the <see cref="XmlRootAttribute.ElementName"/> or the <see cref="MemberInfo.Name">Type.Name</see>, should the
	/// <see cref="XmlRootAttribute.ElementName"/> not be defined, providing flexibility and robustness in XML naming strategies.
	/// </para>
	/// <para>Technically, the method generates a fully qualified XML name based on two scenarios.</para>
	/// <list type="number">
	/// <item>
	/// When <see cref="XmlRootAttribute"/> has both a <see cref="XmlRootAttribute.Namespace"/> and an
	/// <see cref="XmlRootAttribute.ElementName"/>, i.e. when <see cref="IsXmlNamePartiallyQualified"/> returns <see langword="false"/>
	/// , it generates a fully qualified XML name using the format <c>{XmlRootAttribute.Namespace}#{XmlRootAttribute.ElementName}</c>.
	/// </item>
	/// <item>
	/// When <see cref="XmlRootAttribute"/> has a <see cref="XmlRootAttribute.Namespace"/> but no
	/// <see cref="XmlRootAttribute.ElementName"/>, i.e. when <see cref="IsXmlNamePartiallyQualified"/> returns <see langword="true"/>,
	/// it generates a nonetheless fully qualified XML name using the format <c>{XmlRootAttribute.Namespace}#{Type.Name}</c>, thereby
	/// reducing the likelihood of naming conflicts among XML <see cref="Type"/>s that share the same XML
	/// <see cref="XmlRootAttribute.Namespace"/> but do not define a specific <see cref="XmlRootAttribute.ElementName"/>.
	/// </item>
	/// </list>
	/// </remarks>
	[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Validated by nested call to GetXmlRootAttribute.")]
	public static string GetXmlFullyQualifiedName(this Type type)
	{
		var xmlRootAttribute = type.GetRequiredXmlRootAttribute();
		var xmlNamespace = xmlRootAttribute.Namespace.UnlessIsNullOrEmpty(
			$"The {nameof(XmlRootAttribute)} decorating the type '{type.FullName}' must specify an XML {nameof(XmlRootAttribute.Namespace)}.");
		return xmlRootAttribute.IsXmlNamePartiallyQualified()
			? $"{xmlNamespace}#{type.Name}"
			: $"{xmlNamespace}#{xmlRootAttribute.ElementName}";
	}

	/// <summary>Retrieves the <see cref="XmlRootAttribute"/> decorating the given <paramref name="type"/>.</summary>
	/// <param name="type">The <see cref="Type"/> from which to extract the <see cref="XmlRootAttribute"/>.</param>
	/// <returns>
	/// The <see cref="XmlRootAttribute"/> instance associated with the given <paramref name="type"/>, or
	/// <see langword="null"/> otherwise.
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown if the input <paramref name="type"/> is <see langword="null"/>.</exception>
	/// <remarks>This method extracts the <see cref="XmlRootAttribute"/> without considering inherited attributes.</remarks>
	public static XmlRootAttribute? GetXmlRootAttribute(this Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		return type.GetCustomAttribute<XmlRootAttribute>(inherit: false);
	}

	/// <summary>Determines whether the XML name associated with the given <paramref name="type"/> is partially qualified.</summary>
	/// <param name="type">The <see cref="Type"/> to check for a partially qualified XML name.</param>
	/// <returns>
	/// <see langword="true"/> if the given <paramref name="type"/> has a partially qualified XML name;
	/// <see langword="false"/> otherwise.
	/// </returns>
	/// <remarks>
	/// A partially qualified XML name is characterized by an <see cref="XmlRootAttribute"/> that defines a
	/// <see cref="XmlRootAttribute.Namespace"/> but no <see cref="XmlRootAttribute.ElementName"/>.
	/// </remarks>
	public static bool IsXmlNamePartiallyQualified(this Type type)
	{
		return type.GetXmlRootAttribute()?.IsXmlNamePartiallyQualified() == true;
	}

	/// <summary>Determines whether the XML name defined by the <see cref="XmlRootAttribute"/> is partially qualified.</summary>
	/// <param name="attribute">The <see cref="XmlRootAttribute"/> to check for a partially qualified XML name.</param>
	/// <returns>
	/// <see langword="true"/> if the given <see cref="XmlRootAttribute"/> defines a partially qualified XML name;
	/// <see langword="false"/> otherwise.
	/// </returns>
	/// <remarks>
	/// A partially qualified XML name is characterized by an <see cref="XmlRootAttribute"/> that defines a
	/// <see cref="XmlRootAttribute.Namespace"/> but no <see cref="XmlRootAttribute.ElementName"/>.
	/// </remarks>
	private static bool IsXmlNamePartiallyQualified(this XmlRootAttribute attribute)
	{
		return !attribute.Namespace.IsNullOrWhiteSpace() && attribute.ElementName.IsNullOrWhiteSpace();
	}
}
