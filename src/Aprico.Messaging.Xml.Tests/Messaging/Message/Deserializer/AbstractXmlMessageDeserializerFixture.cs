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

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Aprico.Moq.Extensions;
using Aprico.Xml.Extensions;
using AutoFixture.Xunit2;
using Moq;

namespace Aprico.Messaging.Message.Deserializer;

public abstract class AbstractXmlMessageDeserializerFixture
{
	#region Nested Type: AddAbstractXmlContract

	public class AddAbstractXmlContract : AbstractXmlMessageDeserializerFixture
	{
		[Fact]
		[SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Test non generic overload too.")]
		public void CanChainAddXmlContractCalls()
		{
			var sut = new AbstractXmlMessageDeserializerDouble();

			var returnedObject = sut.AddXmlContract<DummyOne>()
				.AddXmlContract(typeof(DummyTwo));

			returnedObject.Should()
				.BeSameAs(sut)
				.And.BeOfType<AbstractXmlMessageDeserializerDouble>();
		}

		[Fact]
		public void DelegateToXmlContractRegistry()
		{
			var sut = new AbstractXmlMessageDeserializerSpy();

			sut.AddXmlContract<DummyOne>();

			sut._xmlContractRegistry.AsMock()
				.Verify(static r => r.RegisterContractType(typeof(DummyOne)), Times.Once);
		}

		[XmlRoot("DummyOne", Namespace = "urn:schemas.aprico.be:add-xml-contract")]
		private sealed class DummyOne;

		[XmlRoot("DummyTwo", Namespace = "urn:schemas.aprico.be:add-xml-contract")]
		private sealed class DummyTwo;
	}

	#endregion

	#region Nested Type: GetAbstractXmlContract

	public class GetAbstractXmlContract : AbstractXmlMessageDeserializerFixture
	{
		[Theory]
		[AutoData]
		public void DelegateToXmlContractRegistry(string xmlFullyQualifiedName)
		{
			var sut = new AbstractXmlMessageDeserializerSpy();

			sut.GetXmlContract(xmlFullyQualifiedName);

			sut._xmlContractRegistry.AsMock()
				.Verify(r => r.GetRegisteredContract(xmlFullyQualifiedName), Times.Once);
		}
	}

	#endregion

	#region Nested Type: XmlMessageBodyDeserializer

	public class XmlMessageBodyDeserializer : AbstractXmlMessageDeserializerFixture
	{
		[Fact]
		public void SucceedsForRegisteredContract()
		{
			var body = new DummyOne().SerializeAsXmlBinary();
			new AbstractXmlMessageDeserializerDouble().AddXmlContract<DummyOne>()
				.DeserializeBody<DummyOne>(body)
				.Should()
				.BeOfType<DummyOne>();
		}

		[XmlRoot("DummySix", Namespace = "urn:schemas.aprico.be:deserializer-body")]
		public class DummyOne;
	}

	#endregion

	#region Nested Type: AbstractXmlMessageDeserializerDouble

	private sealed class AbstractXmlMessageDeserializerDouble : AbstractXmlMessageDeserializer<AbstractXmlMessageDeserializerDouble>
	{
		[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass", Justification = "For testing purpose.")]
		public object DeserializeBody<T>(ReadOnlySequence<byte> body)
		{
			return base.DeserializeBody(typeof(T), body);
		}
	}

	#endregion

	#region Nested Type: AbstractXmlMessageDeserializerSpy

	private sealed class AbstractXmlMessageDeserializerSpy : AbstractXmlMessageDeserializer<AbstractXmlMessageDeserializerSpy>
	{
		public AbstractXmlMessageDeserializerSpy() : base(new Mock<XmlContractRegistry>().Object) { }
	}

	#endregion
}
