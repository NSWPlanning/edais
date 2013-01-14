using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace ETH.Tests
{
	public class MockInjector
	{
		readonly Dictionary<Type, Mock> mocks;

		public MockInjector()
		{
			mocks = new Dictionary<Type, Mock>();
		}

		public T Create<T>()
		{
			var constructorInfo = typeof(T).GetConstructors().First();
			var parameterInfos = constructorInfo.GetParameters();

			var args = new List<object>();
			foreach (var parameter in parameterInfos)
			{
				if (mocks.ContainsKey(parameter.ParameterType))
				{
					args.Add(mocks[parameter.ParameterType].Object);
				}
				else
				{
					var mock = (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(parameter.ParameterType));
					mocks[parameter.ParameterType] = mock;
					args.Add(mock.Object);
				}
			}

			var instance = Activator.CreateInstance(typeof(T), args.ToArray());

			return (T)instance;
		}

		public Mock<TMock> GetMock<TMock>()
			where TMock : class
		{
			if (mocks.ContainsKey(typeof(TMock)))
			{
				return (Mock<TMock>)mocks[typeof(TMock)];
			}

			var mock = new Mock<TMock>();
			mocks[typeof(TMock)] = mock;
			return mock;
		}
	}
}
