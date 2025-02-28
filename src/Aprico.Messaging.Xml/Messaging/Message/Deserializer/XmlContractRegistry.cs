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
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Aprico.Extensions;
using Be.Stateless.Linq.Extensions;

namespace Aprico.Messaging.Message.Deserializer;

/// <summary>Provides a registry mechanism for tracking and managing XML contract types across an application.</summary>
/// <remarks>
/// <para>
/// The registry offers comprehensive type management for XML serialization, enabling robust and flexible contract
/// registration.
/// </para>
/// <para>
/// It implements a thread-safe approach using a concurrent dictionary, which allows seamless and secure registration of
/// contract types.
/// </para>
/// <para>
/// Developers can register individual types or entire assemblies, with built-in safeguards that prevent duplicate
/// registrations of XML message types.
/// </para>
/// <para>
/// This design ensures type consistency and provides a reliable runtime mechanism for looking up contract types based on
/// their XML fully qualified names.
/// </para>
/// </remarks>
/// <threadsafety>
/// This class is thread-safe for concurrent read and write operations due to the use of
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </threadsafety>
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "For unit testing.")]
internal class XmlContractRegistry
{
	/// <summary>Determines whether a contract type is registered in the XML contract registry.</summary>
	/// <typeparam name="T">The type to check for registration.</typeparam>
	/// <returns><see langword="true"/> if the contract type is registered; otherwise, <see langword="false"/>.</returns>
	public static bool IsContractRegistered<T>()
	{
		return IsContractRegistered(typeof(T));
	}

	/// <summary>Determines whether a contract type is registered in the XML contract registry using its runtime type.</summary>
	/// <param name="type">The runtime type to check for registration.</param>
	/// <returns><see langword="true"/> if the contract type is registered; otherwise, <see langword="false"/>.</returns>
	/// <remarks>
	/// This method converts the provided runtime type to its XML fully qualified name and checks for registration in the
	/// contract registry. It serves as a type-based alternative to string-based contract registration verification.
	/// </remarks>
	/// <exception cref="ArgumentNullException">Thrown if the provided type is null.</exception>
	public static bool IsContractRegistered(Type type)
	{
		return IsContractRegistered(type.GetXmlFullyQualifiedName());
	}

	/// <summary>Determines whether a contract type is registered in the XML contract registry using its fully qualified XML name.</summary>
	/// <param name="xmlFullyQualifiedName">The XML fully qualified name of the contract type to check.</param>
	/// <returns>
	/// <see langword="true"/> if a contract with the specified XML fully qualified name is registered; otherwise,
	/// <see langword="false"/>.
	/// </returns>
	/// <remarks>
	/// Checks the internal registry to verify the existence of a contract type based on its XML fully qualified name. This
	/// method provides a direct lookup mechanism for contract registration verification using the standardized XML type identifier.
	/// </remarks>
	/// <exception cref="ArgumentNullException">Thrown if the provided XML fully qualified name is null.</exception>
	/// <exception cref="ArgumentException">Thrown if the provided XML fully qualified name is empty or whitespace.</exception>
	public static bool IsContractRegistered(string xmlFullyQualifiedName)
	{
		return _registry.ContainsKey(xmlFullyQualifiedName);
	}

	/// <summary>Retrieves the registered contract type for a given XML fully qualified name.</summary>
	/// <param name="xmlFullyQualifiedName">The XML fully qualified name of the contract type to retrieve.</param>
	/// <returns>The <see cref="Type"/> associated with the specified XML fully qualified name.</returns>
	/// <remarks>
	/// Attempts to fetch the registered contract type from the internal registry using the provided XML fully qualified name.
	/// If no matching contract type is found, an exception is thrown to indicate the absence of registration.
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	/// Thrown when no contract type is registered for the specified XML fully qualified
	/// name.
	/// </exception>
	/// <exception cref="ArgumentNullException">Thrown if the provided XML fully qualified name is null.</exception>
	public virtual Type GetRegisteredContract(string xmlFullyQualifiedName)
	{
		return _registry.TryGetValue(xmlFullyQualifiedName, out var messageType)
			? messageType
			: throw new InvalidOperationException($"No contract type has been registered for XML message type '{xmlFullyQualifiedName}'.");
	}

	/// <summary>Registers a contract assembly by specifying a generic type from the assembly.</summary>
	/// <typeparam name="T">A type contained within the assembly to be registered.</typeparam>
	/// <remarks>
	/// Enables contract assembly registration using a type parameter, automatically extracting the assembly associated with
	/// the provided type for registration.
	/// </remarks>
	public void RegisterContractAssembly<T>()
	{
		RegisterContractAssembly(typeof(T).Assembly);
	}

	/// <summary>Registers all contract types within the specified assembly that have an <see cref="XmlRootAttribute"/>.</summary>
	/// <param name="assembly">The assembly containing contract types to register.</param>
	/// <remarks>
	/// Scans exported types in the provided assembly, identifying those with an XML root attribute, and automatically
	/// registers each matching type as a contract type.
	/// </remarks>
	public void RegisterContractAssembly(Assembly assembly)
	{
		assembly.ExportedTypes.Where(static type => type.GetCustomAttribute<XmlRootAttribute>(inherit: false) is not null)
			.ForEach(RegisterContractType);
	}

	/// <summary>Registers a specific contract type using a generic type parameter.</summary>
	/// <typeparam name="T">The contract type to be registered.</typeparam>
	/// <remarks>Enables direct registration of a contract type by specifying its type through a generic parameter.</remarks>
	public void RegisterContract<T>()
	{
		RegisterContractType(typeof(T));
	}

	/// <summary>Registers a specific contract type using a Type instance.</summary>
	/// <param name="type">The contract type to be registered.</param>
	/// <remarks>Provides an overload for registering a contract type by passing its Type directly.</remarks>
	public void RegisterContract(Type type)
	{
		RegisterContractType(type);
	}

	internal virtual void RegisterContractType(Type type)
	{
		var xmlFullyQualifiedName = type.GetXmlFullyQualifiedName();
		if (_registry.TryAdd(xmlFullyQualifiedName, type)) return;
		var previouslyRegisteredType = _registry[xmlFullyQualifiedName];
		throw new InvalidOperationException($"XML message type '{xmlFullyQualifiedName}' has already been registered for contract type '{previouslyRegisteredType.FullName}'.");
	}

	private static readonly ConcurrentDictionary<string, Type> _registry = [];
}
