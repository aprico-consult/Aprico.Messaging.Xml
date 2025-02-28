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
using Aprico.Extensions;

namespace Aprico.Messaging.Message.Deserializer;

public abstract class XmlContractRegistryFixture
{
	#region Nested Type: GetRegisteredContract

	public class GetRegisteredContract : XmlContractRegistryFixture
	{
		[Fact]
		public void FailsForUnregisteredContractType()
		{
			var sut = new XmlContractRegistry();
			Invoking(() => sut.GetRegisteredContract(typeof(DummyOne).GetXmlFullyQualifiedName()))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage("No contract type has been registered for XML message type 'urn:schemas.aprico.be:get-contract-type#DummyOne'.");
		}

		[Fact]
		public void SucceedsForRegisteredContractType()
		{
			var sut = new XmlContractRegistry();
			sut.RegisterContract<DummyTwo>();
			sut.GetRegisteredContract(typeof(DummyTwo).GetXmlFullyQualifiedName())
				.Should()
				.Be<DummyTwo>();
		}

		[XmlRoot("DummyOne", Namespace = "urn:schemas.aprico.be:get-contract-type")]
		private sealed class DummyOne;

		[XmlRoot("DummyTwo", Namespace = "urn:schemas.aprico.be:get-contract-type")]
		private sealed class DummyTwo;
	}

	#endregion

	#region Nested Type: RegisterContract

	public class RegisterContract : XmlContractRegistryFixture
	{
		[Fact]
		public void CannotRegisterTwoContractsHavingSameXmlFullyQualifiedName()
		{
			var sut = new XmlContractRegistry();
			sut.RegisterContract<DummyOne>();
			Invoking(sut.RegisterContract<DummyTwo>)
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($"XML message type '{typeof(DummyTwo).GetXmlFullyQualifiedName()}' has already been registered for contract type '{typeof(DummyOne).FullName}'.");
		}

		[Fact]
		public void CanRegisterContractHavingXmlFullyQualifiedName()
		{
			var sut = new XmlContractRegistry();
			sut.RegisterContract<FullyQualified>();
			XmlContractRegistry.IsContractRegistered("urn:schemas.aprico.be:register-contract#FullyQualified")
				.Should()
				.BeTrue();
			XmlContractRegistry.IsContractRegistered<FullyQualified>()
				.Should()
				.BeTrue();
		}

		[Fact]
		[SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Test non generic overload too.")]
		public void CanRegisterContractHavingXmlNamespaceQualifiedName()
		{
			var sut = new XmlContractRegistry();
			sut.RegisterContract(typeof(NamespaceQualified));
			XmlContractRegistry.IsContractRegistered("urn:schemas.aprico.be:register-contract#NamespaceQualified")
				.Should()
				.BeTrue();
			XmlContractRegistry.IsContractRegistered("urn:schemas.aprico.be:register-contract#")
				.Should()
				.BeFalse();
			XmlContractRegistry.IsContractRegistered(typeof(NamespaceQualified))
				.Should()
				.BeTrue();
		}

		// TODO should prevent this
		[Fact]
		public void CanRegisterContractHavingXmlRootQualifiedName()
		{
			var sut = new XmlContractRegistry();
			sut.RegisterContract<RootQualified>();
			XmlContractRegistry.IsContractRegistered("#RootQualified")
				.Should()
				.BeTrue();
		}

		[XmlRoot("DummyXml", Namespace = "urn:schemas.aprico.be:register-contract")]
		private sealed class DummyOne;

		[XmlRoot("DummyXml", Namespace = "urn:schemas.aprico.be:register-contract")]
		private sealed class DummyTwo;

		[XmlRoot("FullyQualified", Namespace = "urn:schemas.aprico.be:register-contract")]
		private sealed class FullyQualified;

		[XmlRoot(Namespace = "urn:schemas.aprico.be:register-contract")]
		private sealed class NamespaceQualified;

		[XmlRoot("RootQualified")]
		private sealed class RootQualified;
	}

	#endregion
}
