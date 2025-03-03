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
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CommunityToolkit.HighPerformance;

namespace Aprico.Messaging.Message.Deserializer;

/// <summary>Provides an abstract base implementation for XML message deserialization.</summary>
/// <typeparam name="TXmlDeserializer">The type of XML message deserializer, typically the derived class itself.</typeparam>
/// <remarks>
/// This abstract class serves as a base for creating specialized XML message deserializer implementations and provides a
/// fluent registration API for XML message contract management.
/// </remarks>
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public abstract class AbstractXmlMessageDeserializer<TXmlDeserializer>
	where TXmlDeserializer : AbstractXmlMessageDeserializer<TXmlDeserializer>
{
	protected AbstractXmlMessageDeserializer() { }

	// for unit test purposes only
	private protected AbstractXmlMessageDeserializer(XmlContractRegistry xmlContractRegistry)
	{
		XmlContractRegistry = xmlContractRegistry;
	}

	internal XmlContractRegistry XmlContractRegistry { get; } = new();

	/// <summary>Registers a specific XML contract type with the deserialization contract registry.</summary>
	/// <typeparam name="T">The XML contract type to register.</typeparam>
	/// <returns>The current XML message deserializer instance, enabling fluent configuration.</returns>
	public TXmlDeserializer AddXmlContract<T>()
		where T : notnull
	{
		XmlContractRegistry.RegisterContract<T>();
		return (TXmlDeserializer) this;
	}

	/// <summary>Registers a specific XML contract type with the deserialization contract registry.</summary>
	/// <param name="type">The type of the XML contract to register.</param>
	/// <returns>The current XML message deserializer instance, enabling fluent configuration.</returns>
	public TXmlDeserializer AddXmlContract(Type type)
	{
		XmlContractRegistry.RegisterContract(type);
		return (TXmlDeserializer) this;
	}

	/// <summary>Registers all contract types within the specified assembly that have an <see cref="XmlRootAttribute"/>.</summary>
	/// <typeparam name="T">The assembly containing XML contract types to register.</typeparam>
	/// <returns>The current XML message deserializer instance, enabling fluent configuration.</returns>
	public TXmlDeserializer AddXmlContractAssembly<T>()
		where T : notnull
	{
		XmlContractRegistry.RegisterContractAssembly<T>();
		return (TXmlDeserializer) this;
	}

	/// <summary>Registers all contract types within the specified assembly that have an <see cref="XmlRootAttribute"/>.</summary>
	/// <param name="assembly">The assembly to register for XML contract deserialization.</param>
	/// <returns>The current XML message deserializer instance, enabling fluent configuration.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the provided assembly is null.</exception>
	public TXmlDeserializer AddXmlContractAssembly(Assembly assembly)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		XmlContractRegistry.RegisterContractAssembly(assembly);
		return (TXmlDeserializer) this;
	}

	/// <summary>Deserializes the message body to an object of the specified type using XML deserialization.</summary>
	/// <param name="type">The type of object to deserialize the message body into.</param>
	/// <param name="body">The raw message body as a <see cref="ReadOnlyMemory{Byte}"/>.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="InvalidOperationException">Thrown if deserialization fails or returns null.</exception>
	protected object DeserializeBody(Type type, ReadOnlyMemory<byte> body)
	{
		using var stream = body.AsStream();
		return DeserializeBody(type, stream);
	}

	/// <summary>Deserializes the message body to an object of the specified type using XML deserialization.</summary>
	/// <param name="type">The type of object to deserialize the message body into.</param>
	/// <param name="body">The raw message body as a <see cref="ReadOnlySequence{Byte}"/>.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="InvalidOperationException">Thrown if deserialization fails or returns null.</exception>
	protected object DeserializeBody(Type type, ReadOnlySequence<byte> body)
	{
		using var stream = body.AsStream();
		return DeserializeBody(type, stream);
	}

	/// <summary>Deserializes the message body to an object of the specified type using XML deserialization.</summary>
	/// <param name="type">The type of object to deserialize the message body into.</param>
	/// <param name="body">The raw message body as a <see cref="Stream"/>.</param>
	/// <returns>The deserialized object.</returns>
	/// <exception cref="InvalidOperationException">Thrown if deserialization fails or returns null.</exception>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
	protected object DeserializeBody(Type type, Stream body)
	{
		var xmlSerializer = new XmlSerializer(type);
		using var xmlReader = XmlReader.Create(body);
		return xmlSerializer.Deserialize(xmlReader) ?? throw new InvalidOperationException($"Deserialization failed for type {type}.");
	}

	/// <summary>Retrieves the registered XML contract type based on its fully qualified name.</summary>
	/// <param name="fullyQualifiedName">The fully qualified name of the XML contract type to retrieve.</param>
	/// <returns>The registered XML contract type corresponding to the specified fully qualified name.</returns>
	/// <remarks>
	/// This method is part of a fluent API and may return null if no contract is registered for the given name. The method is
	/// designed to be used within derived classes and assemblies.
	/// </remarks>
	[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Fluent API.")]
	protected internal Type GetXmlContract(string fullyQualifiedName)
	{
		return XmlContractRegistry.GetRegisteredContract(fullyQualifiedName);
	}
}
