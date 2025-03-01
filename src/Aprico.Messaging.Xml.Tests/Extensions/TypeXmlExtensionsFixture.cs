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

namespace Aprico.Extensions;

public abstract class TypeXmlExtensionsFixture
{
	#region Nested Type: GetRequiredXmlRootAttribute

	[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
	public class GetRequiredXmlRootAttribute : TypeXmlExtensionsFixture
	{
		[Fact]
		public void FailsWhenNotDecoratedWithXmlRootAttribute()
		{
			Invoking(static () => typeof(Dummy).GetRequiredXmlRootAttribute())
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage("type.GetXmlRootAttribute() cannot be null." + Environment.NewLine + $"The type '{typeof(Dummy).FullName}' must be decorated with an {nameof(XmlRootAttribute)}.");
		}

		[Fact]
		public void SucceedsWhenDecoratedWithXmlRootAttribute()
		{
			typeof(ElementNameQualifiedDummyXml).GetRequiredXmlRootAttribute()
				.Should()
				.Be(typeof(ElementNameQualifiedDummyXml).GetXmlRootAttribute());
		}
	}

	#endregion

	#region Nested Type: GetXmlFullyQualifiedName

	public class GetXmlFullyQualifiedName : TypeXmlExtensionsFixture
	{
		[Fact]
		public void FailsWhenForNotXmlNamespaceXmlRootQualified()
		{
			Invoking(static () => typeof(ElementNameQualifiedDummyXml).GetXmlFullyQualifiedName())
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage(
					"xmlRootAttribute.Namespace cannot be null or an empty string." + Environment.NewLine
					+ $"The {nameof(XmlRootAttribute)} decorating the type '{typeof(ElementNameQualifiedDummyXml).FullName}' must specify an XML namespace.");
		}

		[Fact]
		public void ReturnsXmlFullyQualified()
		{
			var qualifiedName = typeof(FullyQualifiedDummy).GetXmlFullyQualifiedName();
			qualifiedName.Should()
				.Be("https://schemas.aprico.be#DummyXml");
		}

		[Fact]
		public void ReturnsXmlFullyQualifiedForPartiallyQualifiedName()
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
		public void SucceedsWhenNoXmlRootAttribute()
		{
			typeof(Dummy).GetXmlRootAttribute()
				.Should()
				.BeNull();
		}

		[Fact]
		public void SucceedsWhenXmlRootAttribute()
		{
			typeof(ElementNameQualifiedDummyXml).GetXmlRootAttribute()
				.Should()
				.NotBeNull()
				.And.BeOfType<XmlRootAttribute>();
		}
	}

	#endregion

	#region Nested Type: HasXmlPartiallyQualifiedName

	public class HasXmlPartiallyQualifiedName : TypeXmlExtensionsFixture
	{
		[Fact]
		public void ReturnsFalseWhenFullyQualified()
		{
			typeof(FullyQualifiedDummy).HasXmlPartiallyQualifiedName()
				.Should()
				.BeFalse();
		}

		[Fact]
		public void ReturnsFalseWhenNoNamespace()
		{
			typeof(ElementNameQualifiedDummyXml).HasXmlPartiallyQualifiedName()
				.Should()
				.BeFalse();
		}

		[Fact]
		public void ReturnsFalseWhenNoXmlRootAttribute()
		{
			typeof(Dummy).HasXmlPartiallyQualifiedName()
				.Should()
				.BeFalse();
		}

		[Fact]
		public void ReturnsTrueWhenNamespaceAndNoElementName()
		{
			typeof(PartiallyQualifiedDummy).HasXmlPartiallyQualifiedName()
				.Should()
				.BeTrue();
		}
	}

	#endregion

	#region Test Dummies

	private sealed class Dummy;

	[XmlRoot("DummyXml")]
	private sealed class ElementNameQualifiedDummyXml;

	[XmlRoot("DummyXml", Namespace = "https://schemas.aprico.be")]
	private sealed class FullyQualifiedDummy;

	[XmlRoot(Namespace = "https://schemas.aprico.be")]
	private sealed class PartiallyQualifiedDummy;

	#endregion
}
