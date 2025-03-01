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
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Aprico.Extensions;
using Be.Stateless.Extensions;
using Microsoft.IO;

namespace Aprico.Xml.Extensions;

/// <summary>Provides extension methods for XML serialization operations.</summary>
/// <remarks>
/// This static class contains utility methods to enhance XML serialization capabilities and provide additional
/// functionality for XML-related operations.
/// </remarks>
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public static class ObjectXmlSerializationExtensions
{
	/// <summary>Serializes an object of type <typeparamref name="T"/> to XML as a read-only byte sequence.</summary>
	/// <typeparam name="T">The type of the object to serialize. Must be a non-null type.</typeparam>
	/// <param name="body">The object to be serialized to XML. Cannot be null.</param>
	/// <returns>A <see cref="ReadOnlySequence{Byte}"/> containing the XML serialized representation of the object.</returns>
	/// <remarks>
	/// This method uses a recyclable memory stream to serialize the object efficiently. It avoids creating unnecessary
	/// allocations by returning a <see cref="ReadOnlySequence{Byte}"/>. The serialization is performed using the
	/// <see cref="XmlWriter"/> with uniform configuration settings to ensure consistent XML serialization.
	/// </remarks>
	/// <exception cref="ArgumentNullException">Thrown if the input object is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown if XML serialization fails.</exception>
	[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
	public static ReadOnlySequence<byte> SerializeAsXmlBinary<T>([DisallowNull] this T body)
		where T : notnull
	{
		using var stream = _streamManager.GetStream($"{nameof(ObjectXmlSerializationExtensions)}.{nameof(SerializeAsXmlBinary)}");
		body.WriteXml(settings => XmlWriter.Create(stream, settings));
		return stream.GetReadOnlySequence();
	}

	/// <summary>Serializes an object of type <typeparamref name="T"/> to an XML string representation.</summary>
	/// <typeparam name="T">The type of the object to serialize. Must be non-null.</typeparam>
	/// <param name="body">The object to be serialized to XML.</param>
	/// <returns>A string containing the XML representation of the object.</returns>
	/// <remarks>This extension method converts the object to an XML string using a <see cref="StringWriter"/>.</remarks>
	/// <exception cref="ArgumentNullException">Thrown if the input object is null.</exception>
	[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
	public static string SerializeAsXmlString<T>([DisallowNull] this T body)
		where T : notnull
	{
		using var writer = new StringWriter();
		body.WriteXml(settings => XmlWriter.Create(writer, settings));
		return writer.ToString();
	}

	private static void WriteXml<T>([DisallowNull] this T body, Func<XmlWriterSettings, XmlWriter> xmlWriterFactory)
		where T : notnull
	{
		ArgumentNullException.ThrowIfNull(body);
		ArgumentNullException.ThrowIfNull(xmlWriterFactory);

		// fail fast if type does not have an XmlRootAttribute
		var rootAttribute = typeof(T).GetRequiredXmlRootAttribute();

		var xmlns = new XmlSerializerNamespaces();
		// omitting xsi and xsd namespaces when serializing an object in .NET, see https://stackoverflow.com/a/935749/1789441
		xmlns.Add(string.Empty, string.Empty);
		// TODO !! should not support this, must always have a non empty namespace !! ?? or maybe OK here but not when registering contracts ??
		rootAttribute.Namespace.IfNotNullOrEmpty(ns => xmlns.Add("q1", ns));

		using var writer = xmlWriterFactory(_xmlWriterSettings);
		// https://docs.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlserializer?view=netframework-4.8#dynamically-generated-assemblies
		var serializer = new XmlSerializer(typeof(T));
		serializer.Serialize(writer, body, xmlns);
		writer.Flush();
	}

	// https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream#usage-guidelines
	private static readonly RecyclableMemoryStreamManager _streamManager = new(
		new RecyclableMemoryStreamManager.Options {
			BlockSize = 4096, // standard page size, good for most scenarios
			LargeBufferMultiple = 1024 * 1024,
			MaximumBufferSize = 16 * 1024 * 1024,
			MaximumLargePoolFreeBytes = 64 * 1024 * 1024,
			MaximumSmallPoolFreeBytes = 1024 * 1024
		});

	private static readonly XmlWriterSettings _xmlWriterSettings = new() {
		Encoding = Encoding.UTF8,
		Indent = false,
		OmitXmlDeclaration = true,
		NamespaceHandling = NamespaceHandling.OmitDuplicates
	};
}
