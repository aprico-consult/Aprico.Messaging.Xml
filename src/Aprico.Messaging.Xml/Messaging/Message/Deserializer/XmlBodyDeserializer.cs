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
using System.Xml;
using System.Xml.Serialization;
using CommunityToolkit.HighPerformance;

namespace Aprico.Messaging.Message.Deserializer;

/// <summary>Provides static methods for deserializing XML-formatted message bodies to objects of specified types.</summary>
/// <remarks>
/// This class offers flexible XML deserialization methods that support various input types such as
/// <see cref="ReadOnlyMemory{Byte}"/>, <see cref="ReadOnlySequence{Byte}"/>, and <see cref="Stream"/>.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// // Example usage with ReadOnlyMemory<byte>
/// byte[] xmlBytes = Encoding.UTF8.GetBytes("<MyType>Data</MyType>");
/// MyType result = (MyType)XmlBodyDeserializer.Deserialize(typeof(MyType), xmlBytes);
///
/// // Example usage with Stream
/// using (var stream = new MemoryStream(xmlBytes))
/// {
///     MyType result = (MyType)XmlBodyDeserializer.Deserialize(typeof(MyType), stream);
/// }
/// ]]>
/// </code>
/// </example>
/// <seealso cref="XmlSerializer"/>
/// <seealso cref="XmlReader"/>
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public static class XmlBodyDeserializer
{
	/// <summary>
	/// Deserializes the message <paramref name="body"/> to an object of the specified <paramref name="type"/> using XML
	/// deserialization.
	/// </summary>
	/// <param name="body">The raw message body as a <see cref="ReadOnlyMemory{Byte}"/>.</param>
	/// <param name="type">The <see cref="Type"/> of object to deserialize the message <paramref name="body"/> into.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="InvalidOperationException">Thrown if deserialization fails or returns null.</exception>
	public static object Deserialize(this ReadOnlyMemory<byte> body, Type type)
	{
		return Deserialize(type, body);
	}

	/// <summary>
	/// Deserializes the message <paramref name="body"/> to an object of the specified <paramref name="type"/> using XML
	/// deserialization.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> of object to deserialize the message <paramref name="body"/> into.</param>
	/// <param name="body">The raw message body as a <see cref="ReadOnlyMemory{Byte}"/>.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="InvalidOperationException">Thrown if deserialization fails or returns null.</exception>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
	public static object Deserialize(Type type, ReadOnlyMemory<byte> body)
	{
		using var stream = body.AsStream();
		return Deserialize(type, stream);
	}

	/// <summary>
	/// Deserializes the message <paramref name="body"/> to an object of the specified <paramref name="type"/> using XML
	/// deserialization.
	/// </summary>
	/// <param name="body">The raw message body as a <see cref="ReadOnlySequence{Byte}"/>.</param>
	/// <param name="type">The <see cref="Type"/> of object to deserialize the message <paramref name="body"/> into.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="InvalidOperationException">Thrown if deserialization fails or returns null.</exception>
	public static object Deserialize(this ReadOnlySequence<byte> body, Type type)
	{
		return Deserialize(type, body);
	}

	/// <summary>
	/// Deserializes the message <paramref name="body"/> to an object of the specified <paramref name="type"/> using XML
	/// deserialization.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> of object to deserialize the message <paramref name="body"/> into.</param>
	/// <param name="body">The raw message body as a <see cref="ReadOnlySequence{Byte}"/>.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="InvalidOperationException">Thrown if deserialization fails or returns null.</exception>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
	public static object Deserialize(Type type, ReadOnlySequence<byte> body)
	{
		using var stream = body.AsStream();
		return Deserialize(type, stream);
	}

	/// <summary>
	/// Deserializes the message <paramref name="body"/> to an object of the specified <paramref name="type"/> using XML
	/// deserialization.
	/// </summary>
	/// <param name="body">The raw message body as a <see cref="Stream"/>.</param>
	/// <param name="type">The <see cref="Type"/> of object to deserialize the message <paramref name="body"/> into.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="InvalidOperationException">Thrown if deserialization fails or returns null.</exception>
	public static object Deserialize(this Stream body, Type type)
	{
		return Deserialize(type, body);
	}

	/// <summary>
	/// Deserializes the message <paramref name="body"/> to an object of the specified <paramref name="type"/> using XML
	/// deserialization.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> of object to deserialize the message <paramref name="body"/> into.</param>
	/// <param name="body">The raw message body as a <see cref="Stream"/>.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="InvalidOperationException">Thrown if deserialization fails or returns null.</exception>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
	public static object Deserialize(Type type, Stream body)
	{
		var xmlSerializer = new XmlSerializer(type);
		using var xmlReader = XmlReader.Create(body);
		return xmlSerializer.Deserialize(xmlReader) ?? throw new InvalidOperationException($"Deserialization failed for type {type}.");
	}
}
