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
using Aprico.Dummies;
using Aprico.Moq.Extensions;
using Aprico.Xml.Extensions;
using AutoFixture.Xunit2;
using Moq;

namespace Aprico.Messaging.Message.Deserializer;

[SuppressMessage("Design", "CA1063:Implement IDisposable Correctly")]
[SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
public abstract class AbstractXmlMessageDeserializerFixture : IDisposable
{
	#region Nested Type: AddXmlContract

	[Collection(nameof(XmlContractRegistry))]
	public class AddXmlContract : AbstractXmlMessageDeserializerFixture
	{
		[Fact]
		[SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Test non generic overload too.")]
		public void CanChainAddXmlContractCalls()
		{
			var sut = new XmlMessageDeserializerDummy();

			var returnedObject = sut.AddXmlContract<FullyQualifiedDummyOne>()
				.AddXmlContract(typeof(PartiallyQualifiedDummy));

			returnedObject.Should()
				.BeSameAs(sut)
				.And.BeOfType<XmlMessageDeserializerDummy>();
		}

		[Fact]
		public void DelegateToXmlContractRegistry()
		{
			var sut = new AbstractXmlMessageDeserializerSpy();

			sut.AddXmlContract<FullyQualifiedDummyOne>();

			sut.XmlContractRegistry.AsMock()
				.Verify(static r => r.RegisterContractType(typeof(FullyQualifiedDummyOne)), Times.Once);
		}
	}

	#endregion

	#region Nested Type: DeserializeBody

	[Collection(nameof(XmlContractRegistry))]
	public class DeserializeBody : AbstractXmlMessageDeserializerFixture
	{
		[Fact]
		public void SucceedsForRegisteredContract()
		{
			new XmlMessageDeserializerDummy().AddXmlContract<FullyQualifiedDummyOne>()
				.DeserializeBody<FullyQualifiedDummyOne>(new FullyQualifiedDummyOne().SerializeAsXmlBinary())
				.Should()
				.BeOfType<FullyQualifiedDummyOne>();
		}
	}

	#endregion

	#region Nested Type: GetXmlContract

	[Collection(nameof(XmlContractRegistry))]
	public class GetXmlContract : AbstractXmlMessageDeserializerFixture
	{
		[Theory]
		[AutoData]
		public void DelegateToXmlContractRegistry(string xmlFullyQualifiedName)
		{
			var sut = new AbstractXmlMessageDeserializerSpy();

			sut.GetXmlContract(xmlFullyQualifiedName);

			sut.XmlContractRegistry.AsMock()
				.Verify(r => r.GetRegisteredContract(xmlFullyQualifiedName), Times.Once);
		}
	}

	#endregion

	#region IDisposable Members

	public void Dispose()
	{
		XmlContractRegistry.Registry.Clear();
	}

	#endregion
}
