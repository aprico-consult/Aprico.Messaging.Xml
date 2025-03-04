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

namespace Aprico.Extensions;

/// <summary>Provides extension methods for obtaining XML-related information about XML contract instances.</summary>
/// <remarks>
/// This static class contains utility methods for retrieving fully qualified XML names from XML contract instances. It
/// helps in creating consistent and standardized XML naming for message contracts in distributed systems.
/// </remarks>
[SuppressMessage("Naming", "CA1720:Identifier contains type name")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public static class ObjectXmlExtensions
{
	/// <summary>Retrieves the fully qualified XML name for a given XML contract instance.</summary>
	/// <typeparam name="T">The type of the XML contract, constrained to non-null types.</typeparam>
	/// <param name="object">The XML contract instance for which to retrieve the XML name.</param>
	/// <returns>A string representing the fully qualified XML name of the contract instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the input object is null.</exception>
	/// <remarks>
	/// This method retrieves the fully qualified XML name of the contract instance by calling the corresponding type-level
	/// method.
	/// </remarks>
	public static string GetXmlFullyQualifiedName<T>([DisallowNull] this T @object)
		where T : notnull
	{
		ArgumentNullException.ThrowIfNull(@object);
		return typeof(T).GetXmlFullyQualifiedName();
	}
}
