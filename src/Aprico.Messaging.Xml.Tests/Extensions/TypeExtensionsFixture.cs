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
using System.Xml.Serialization;

namespace Aprico.Extensions;

public abstract class TypeExtensionsFixture
{
	#region Nested Type: GetXmlFullyQualifiedName

	public class GetXmlFullyQualifiedName : TypeExtensionsFixture
	{
		[Fact]
		public void FailsWhenNoXmlRootAttribute()
		{
			Invoking(static () => typeof(Dummy).GetXmlFullyQualifiedName())
				.Should()
				.Throw<InvalidOperationException>();
		}

		[Fact]
		public void SucceedsForXmlFullyQualified()
		{
			var qualifiedName = typeof(FullyQualifiedDummy).GetXmlFullyQualifiedName();
			qualifiedName.Should()
				.Be("https://schemas.aprico.be#DummyXml");
		}

		[Fact]
		public void SucceedsForXmlNamespaceRootQualified()
		{
			var qualifiedName = typeof(NamespaceQualifiedDummy).GetXmlFullyQualifiedName();
			qualifiedName.Should()
				.Be("https://schemas.aprico.be#NamespaceQualifiedDummy");
		}

		[Fact]
		public void SucceedsForXmlRootQualified()
		{
			// TODO ?? should not allow to send message that are xml root qualified only ??
			var qualifiedName = typeof(RootQualifiedDummyXml).GetXmlFullyQualifiedName();
			qualifiedName.Should()
				.Be("#DummyXml");
		}

		private sealed class Dummy;

		[XmlRoot("DummyXml", Namespace = "https://schemas.aprico.be")]
		private sealed class FullyQualifiedDummy;

		[XmlRoot(Namespace = "https://schemas.aprico.be")]
		private sealed class NamespaceQualifiedDummy;

		[XmlRoot("DummyXml")]
		private sealed class RootQualifiedDummyXml;
	}

	#endregion
}
