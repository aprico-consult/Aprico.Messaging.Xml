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
using Aprico.Extensions;

namespace Aprico.Messaging.Message.Deserializer;

[SuppressMessage("Design", "CA1063:Implement IDisposable Correctly")]
[SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
public abstract class XmlContractRegistryFixture : IDisposable
{
	#region Nested Type: GetRegisteredContract

	[Collection(nameof(XmlContractRegistry))]
	public class GetRegisteredContract : XmlContractRegistryFixture
	{
		[Fact]
		public void FailsForUnregisteredContractType()
		{
			var sut = new XmlContractRegistry();
			Invoking(() => sut.GetRegisteredContract(typeof(FullyQualifiedDummyOne).GetXmlFullyQualifiedName()))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($"No contract type has been registered for XML message type '{typeof(FullyQualifiedDummyOne).GetXmlFullyQualifiedName()}'.");
		}

		[Fact]
		public void SucceedsForRegisteredContractType()
		{
			var sut = new XmlContractRegistry();
			sut.RegisterContract<FullyQualifiedDummyOne>();
			sut.GetRegisteredContract(typeof(FullyQualifiedDummyOne).GetXmlFullyQualifiedName())
				.Should()
				.Be<FullyQualifiedDummyOne>();
		}
	}

	#endregion

	#region Nested Type: RegisterContract

	[Collection(nameof(XmlContractRegistry))]
	public class RegisterContract : XmlContractRegistryFixture
	{
		[Fact]
		public void CannotRegisterContractHavingXmlRootQualifiedName()
		{
			var sut = new XmlContractRegistry();
			Invoking(sut.RegisterContract<RootNameQualifiedDummy>)
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage("xmlRootAttribute.Namespace cannot be null or an empty string.*");
		}

		[Fact]
		public void CannotRegisterTwoContractsHavingSameXmlFullyQualifiedName()
		{
			var sut = new XmlContractRegistry();
			sut.RegisterContract<FullyQualifiedDummyOne>();
			Invoking(sut.RegisterContract<FullyQualifiedDummyTwo>)
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage(
					$"The XML message type '{typeof(FullyQualifiedDummyOne).GetXmlFullyQualifiedName()}' "
					+ $"has already been registered by the contract type '{typeof(FullyQualifiedDummyOne).FullName}' "
					+ $"and cannot be registered again by the contract type '{typeof(FullyQualifiedDummyTwo).FullName}'.");
		}

		[Fact]
		public void CanRegisterContractHavingXmlFullyQualifiedName()
		{
			var sut = new XmlContractRegistry();
			sut.RegisterContract<FullyQualifiedDummyOne>();
			XmlContractRegistry.IsContractRegistered(typeof(FullyQualifiedDummyOne).GetXmlFullyQualifiedName())
				.Should()
				.BeTrue();
			XmlContractRegistry.IsContractRegistered<FullyQualifiedDummyOne>()
				.Should()
				.BeTrue();
		}

		[Fact]
		[SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Test non generic overload too.")]
		public void CanRegisterContractHavingXmlPartiallyQualifiedName()
		{
			var sut = new XmlContractRegistry();
			sut.RegisterContract(typeof(PartiallyQualifiedDummy));
			XmlContractRegistry.IsContractRegistered(typeof(PartiallyQualifiedDummy).GetXmlFullyQualifiedName())
				.Should()
				.BeTrue();
			XmlContractRegistry.IsContractRegistered(typeof(PartiallyQualifiedDummy))
				.Should()
				.BeTrue();
			XmlContractRegistry.IsContractRegistered($"{typeof(PartiallyQualifiedDummy).GetRequiredXmlRootAttribute().Namespace}#")
				.Should()
				.BeFalse();
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
