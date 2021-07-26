using Oddin.OddsFeedSdk.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.Exceptions
{
    internal interface IExceptionWrapper
    {
        T Wrap<T>(Func<T> call);
    }

    internal class ExceptionWrapper : IExceptionWrapper
    {
        private readonly IFeedConfiguration _configuration;

        public ExceptionWrapper(IFeedConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T Wrap<T>(Func<T> call)
        {
            try
            {
                return call();
            }
            catch (Exception)
            {
                if(_configuration.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
                return default;
            }
        }
        
        public async Task<T> Wrap<T>(Func<Task<T>> call)
        {
            try
            {
                return await call();
            }
            catch (Exception)
            {
                if(_configuration.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
                return default;
            }
        }
    }
}
