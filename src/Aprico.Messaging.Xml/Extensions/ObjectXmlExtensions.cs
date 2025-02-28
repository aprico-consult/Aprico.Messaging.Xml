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

/// <summary>Provides extension methods for obtaining XML-related naming information for XML message contract instances and types.</summary>
/// <remarks>
/// This static class contains utility methods for generating fully qualified XML names specifically for XML message
/// contract instances and types. It helps in creating consistent and standardized XML naming for message contracts in distributed
/// systems or messaging frameworks.
/// </remarks>
[SuppressMessage("Naming", "CA1720:Identifier contains type name")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
public static class ObjectXmlExtensions
{
	/// <summary>Retrieves the fully qualified XML name for a given XML message contract instance.</summary>
	/// <typeparam name="T">The type of the XML message contract, constrained to non-null types.</typeparam>
	/// <param name="object">The XML message contract instance for which to generate the XML name.</param>
	/// <returns>A string representing the fully qualified XML name of the message contract type.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the input object is null.</exception>
	/// <remarks>
	/// This method delegates the XML name generation to the type-level XML name retrieval method. It ensures that the input
	/// object is not null before proceeding with the name generation.
	/// </remarks>
	public static string GetXmlFullyQualifiedName<T>([DisallowNull] this T @object)
		where T : notnull
	{
		ArgumentNullException.ThrowIfNull(@object);
		return typeof(T).GetXmlFullyQualifiedName();
	}
}
