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
using System.Xml.Serialization;
using Aprico.Dummies;

namespace Aprico.Extensions;

public abstract class TypeXmlExtensionsFixture
{
	#region Nested Type: GetRequiredXmlRootAttribute

	[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
	public class GetRequiredXmlRootAttribute : TypeXmlExtensionsFixture
	{
		[Fact]
		public void FailsForUnqualified()
		{
			Invoking(static () => typeof(UnqualifiedDummy).GetRequiredXmlRootAttribute())
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage(
					"type.GetXmlRootAttribute() cannot be null." + Environment.NewLine + $"The type '{typeof(UnqualifiedDummy).FullName}' must be decorated with an {nameof(XmlRootAttribute)}.");
		}

		[Fact]
		public void SucceedsForQualified()
		{
			typeof(RootNameQualifiedDummy).GetRequiredXmlRootAttribute()
				.Should()
				.Be(typeof(RootNameQualifiedDummy).GetXmlRootAttribute());
		}
	}

	#endregion

	#region Nested Type: GetXmlFullyQualifiedName

	public class GetXmlFullyQualifiedName : TypeXmlExtensionsFixture
	{
		[Fact]
		public void FailsForRootNameQualified()
		{
			Invoking(static () => typeof(RootNameQualifiedDummy).GetXmlFullyQualifiedName())
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage(
					"xmlRootAttribute.Namespace cannot be null or an empty string." + Environment.NewLine
					+ $"The {nameof(XmlRootAttribute)} decorating the type '{typeof(RootNameQualifiedDummy).FullName}' must specify an XML namespace.");
		}

		[Fact]
		public void SucceedsForFullyQualified()
		{
			var qualifiedName = typeof(FullyQualifiedDummy).GetXmlFullyQualifiedName();
			qualifiedName.Should()
				.Be("https://schemas.aprico.be#DummyXml");
		}

		[Fact]
		public void SucceedsForPartiallyQualified()
		{
			var qualifiedName = typeof(PartiallyQualifiedDummy).GetXmlFullyQualifiedName();
			qualifiedName.Should()
				.Be($"https://schemas.aprico.be#{nameof(PartiallyQualifiedDummy)}");
		}
	}

	#endregion

	#region Nested Type: GetXmlRootAttribute

	[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
	public class GetXmlRootAttribute : TypeXmlExtensionsFixture
	{
		[Fact]
		public void SucceedsForQualified()
		{
			typeof(UnqualifiedDummy).GetXmlRootAttribute()
				.Should()
				.BeNull();
		}

		[Fact]
		public void SucceedsForUnqualified()
		{
			typeof(RootNameQualifiedDummy).GetXmlRootAttribute()
				.Should()
				.NotBeNull()
				.And.BeOfType<XmlRootAttribute>();
		}
	}

	#endregion

	#region Nested Type: IsXmlNamePartiallyQualified

	public class IsXmlNamePartiallyQualified : TypeXmlExtensionsFixture
	{
		[Fact]
		public void ReturnsFalseForFullyQualified()
		{
			typeof(FullyQualifiedDummy).IsXmlNamePartiallyQualified()
				.Should()
				.BeFalse();
		}

		[Fact]
		public void ReturnsFalseForRootNameQualified()
		{
			typeof(RootNameQualifiedDummy).IsXmlNamePartiallyQualified()
				.Should()
				.BeFalse();
		}

		[Fact]
		public void ReturnsFalseForUnqualified()
		{
			typeof(UnqualifiedDummy).IsXmlNamePartiallyQualified()
				.Should()
				.BeFalse();
		}

		[Fact]
		public void ReturnsTrueForPartiallyQualified()
		{
			typeof(PartiallyQualifiedDummy).IsXmlNamePartiallyQualified()
				.Should()
				.BeTrue();
		}
	}

	#endregion
}
