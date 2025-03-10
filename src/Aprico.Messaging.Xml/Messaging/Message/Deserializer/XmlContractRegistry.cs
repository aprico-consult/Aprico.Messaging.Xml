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
using Aprico.Extensions;
using Be.Stateless.Linq.Extensions;

namespace Aprico.Messaging.Message.Deserializer;

/// <summary>Provides a registry mechanism for tracking and managing XML contract types across an application.</summary>
/// <remarks>
/// <para>
/// The registry offers comprehensive type management for XML serialization, enabling robust and flexible XML contract
/// registration.
/// </para>
/// <para>
/// It implements a thread-safe approach using a concurrent dictionary, which allows seamless and secure registration of XML
/// contract types.
/// </para>
/// <para>
/// Developers can register individual types or entire assemblies, with built-in safeguards that prevent duplicate
/// registrations of XML message types.
/// </para>
/// <para>
/// This design ensures type consistency and provides a reliable runtime mechanism for looking up XML contract types based on
/// their XML fully qualified names.
/// </para>
/// </remarks>
/// <threadsafety>
/// This class is thread-safe for concurrent read and write operations due to the use of
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </threadsafety>
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "For mocking purposes.")]
internal class XmlContractRegistry
{
	/// <summary>Determines whether a contract <see cref="Type"/> is registered in the <see cref="XmlContractRegistry"/>.</summary>
	/// <typeparam name="T">The type to check for registration.</typeparam>
	/// <returns><see langword="true"/> if the contract <see cref="Type"/> is registered; otherwise, <see langword="false"/>.</returns>
	public static bool IsContractRegistered<T>()
	{
		return IsContractRegistered(typeof(T));
	}

	/// <summary>Determines whether a contract <see cref="Type"/> is registered in the <see cref="XmlContractRegistry"/>.</summary>
	/// <param name="type">The <see cref="Type"/> to check for registration.</param>
	/// <returns><see langword="true"/> if the contract <paramref name="type"/> is registered; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the provided type is null.</exception>
	public static bool IsContractRegistered(Type type)
	{
		return IsContractRegistered(type.GetXmlFullyQualifiedName());
	}

	/// <summary>
	/// Determines whether a contract <see cref="Type"/> is registered in the <see cref="XmlContractRegistry"/> using its
	/// fully qualified XML name.
	/// </summary>
	/// <param name="xmlFullyQualifiedName">The XML fully qualified name of the contract <see cref="Type"/> to check.</param>
	/// <returns>
	/// <see langword="true"/> if a contract with the specified <paramref name="xmlFullyQualifiedName"/> name is registered;
	/// otherwise, <see langword="false"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="xmlFullyQualifiedName"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if the provided <paramref name="xmlFullyQualifiedName"/> is empty or whitespace.</exception>
	public static bool IsContractRegistered(string xmlFullyQualifiedName)
	{
		return Registry.ContainsKey(xmlFullyQualifiedName);
	}

	/// <summary>Retrieves the registered contract <see cref="Type"/> for a given <paramref name="xmlFullyQualifiedName"/>.</summary>
	/// <param name="xmlFullyQualifiedName">The XML fully qualified name of the contract <see cref="Type"/> to retrieve.</param>
	/// <returns>The <see cref="Type"/> associated with the specified <paramref name="xmlFullyQualifiedName"/>.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown if no contract <see cref="Type"/> has been registered for the specified
	/// <paramref name="xmlFullyQualifiedName"/>.
	/// </exception>
	/// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="xmlFullyQualifiedName"/> is null.</exception>
	public virtual Type GetRegisteredContract(string xmlFullyQualifiedName)
	{
		return Registry.TryGetValue(xmlFullyQualifiedName, out var type)
			? type
			: throw new InvalidOperationException($"No contract type has been registered for XML message type '{xmlFullyQualifiedName}'.");
	}

	/// <summary>Registers a specific XML contract <see cref="Type"/>.</summary>
	/// <typeparam name="T">The contract <see cref="Type"/> to register.</typeparam>
	/// <remarks>An XML contract <see cref="Type"/> must be decorated with an <see cref="XmlRootAttribute"/>.</remarks>
	public void RegisterContract<T>()
	{
		RegisterContractType(typeof(T));
	}

	/// <summary>Registers a specific XML contract <see cref="Type"/> using a given instance <paramref name="type"/>.</summary>
	/// <param name="type">The contract type to be registered.</param>
	/// <remarks>An XML contract <see cref="Type"/> must be decorated with an <see cref="XmlRootAttribute"/>.</remarks>
	public void RegisterContract(Type type)
	{
		RegisterContractType(type);
	}

	/// <summary>
	/// Registers all the XML contract <see cref="Type"/>s defined in an assembly, by specifying a generic
	/// <typeparamref name="T"/> <see cref="Type"/> from the assembly.
	/// </summary>
	/// <typeparam name="T">Any <see cref="Type"/> of the assembly containing XML contract <see cref="Type"/>s to register.</typeparam>
	public void RegisterContractAssembly<T>()
	{
		RegisterContractAssembly(typeof(T).Assembly);
	}

	/// <summary>Registers all the XML contract <see cref="Type"/>s within the specified <paramref name="assembly"/>.</summary>
	/// <param name="assembly">The assembly containing XML contract <see cref="Type"/>s to register.</param>
	/// <remarks>An XML contract <see cref="Type"/> must be decorated with an <see cref="XmlRootAttribute"/>.</remarks>
	public void RegisterContractAssembly(Assembly assembly)
	{
		assembly.ExportedTypes.Where(static type => type.GetXmlRootAttribute() is not null)
			.ForEach(RegisterContractType);
	}

	internal virtual void RegisterContractType(Type type)
	{
		var xmlFullyQualifiedName = type.GetXmlFullyQualifiedName();
		if (Registry.TryAdd(xmlFullyQualifiedName, type)) return;
		var previouslyRegisteredType = Registry[xmlFullyQualifiedName];
		if (previouslyRegisteredType == type) return;
		throw new InvalidOperationException(
			$"The XML message type '{xmlFullyQualifiedName}' has already been registered by the contract type '{previouslyRegisteredType.FullName}' and cannot be registered again by the contract type '{type.FullName}'.");
	}

	internal static readonly ConcurrentDictionary<string, Type> Registry = [];
}
