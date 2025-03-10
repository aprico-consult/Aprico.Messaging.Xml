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
using System.Text;
using Aprico.Dummies;

namespace Aprico.Xml.Extensions;

public abstract class ObjectXmlSerializationExtensionsFixture
{
	#region Nested Type: SerializeAsXmlBinary

	public class SerializeAsXmlBinary : ObjectXmlSerializationExtensionsFixture
	{
		[Fact]
		public void FailsForRootNameQualified()
		{
			Invoking(static () => new RootNameQualifiedDummy().SerializeAsXmlBinary())
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage("xmlRootAttribute.Namespace cannot be null or an empty string.*");
		}

		[Fact]
		public void FailsForUnqualified()
		{
			Invoking(static () => new UnqualifiedDummy().SerializeAsXmlBinary())
				.Should()
				.Throw<InvalidOperationException>();
		}

		[Fact]
		public void SucceedsForFullyQualified()
		{
			var xmlBinary = new FullyQualifiedDummy().SerializeAsXmlBinary();
			xmlBinary.ToArray()
				.Should()
				.BeEquivalentTo(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes($"<q:DummyXml xmlns:q=\"https://schemas.aprico.be\"><q:Id>{Guid.Empty}</q:Id></q:DummyXml>"));
		}

		[Fact]
		public void SucceedsForPartiallyQualified()
		{
			var xmlBinary = new PartiallyQualifiedDummy().SerializeAsXmlBinary();
			xmlBinary.ToArray()
				.Should()
				.BeEquivalentTo("<q:PartiallyQualifiedDummy xmlns:q=\"https://schemas.aprico.be\" />"u8.ToArray());
		}
	}

	#endregion

	#region Nested Type: SerializeAsXmlString

	public class SerializeAsXmlString : ObjectXmlSerializationExtensionsFixture
	{
		[Fact]
		public void FailsForRootNameQualified()
		{
			Invoking(static () => new RootNameQualifiedDummy().SerializeAsXmlString())
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage("xmlRootAttribute.Namespace cannot be null or an empty string.*");
		}

		[Fact]
		public void FailsForUnqualified()
		{
			Invoking(static () => new UnqualifiedDummy().SerializeAsXmlString())
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage("type.GetXmlRootAttribute() cannot be null.*");
		}

		[Fact]
		public void SucceedsForFullyQualified()
		{
			var xmlString = new FullyQualifiedDummy().SerializeAsXmlString();
			xmlString.Should()
				.Be($"<q:DummyXml xmlns:q=\"https://schemas.aprico.be\"><q:Id>{Guid.Empty}</q:Id></q:DummyXml>");
		}

		[Fact]
		public void SucceedsForPartiallyQualified()
		{
			var xmlString = new PartiallyQualifiedDummy().SerializeAsXmlString();
			xmlString.Should()
				.Be("<q:PartiallyQualifiedDummy xmlns:q=\"https://schemas.aprico.be\" />");
		}
	}

	#endregion
}
