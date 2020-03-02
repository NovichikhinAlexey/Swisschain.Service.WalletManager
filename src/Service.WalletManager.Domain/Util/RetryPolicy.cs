using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Service.WalletManager.Domain.Util
{
    public static class RetryPolicy
    {
        public static async Task<T> RetryWithExpBackoff<T>(Func<Task<T>> func, ILogger logger,int retry = 5, int basicDelayMs = 100)
        {
            int counter = 0;
            Exception exception = null;

            do
            {
                try
                {
                    var result = await func();

                    return result;
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Exception happened, retrying");
                    exception = e;
                }

                var waitForMs = basicDelayMs * (int)Math.Pow(2, counter);
                await Task.Delay(waitForMs);
                counter++;

                if (counter == retry)
                    throw exception;

            } while (true);
        }

        public static async Task RetryWithExpBackoff(Func<Task> func, ILogger logger, int retry = 5, int basicDelayMs = 100)
        {
            int counter = 0;
            Exception exception = null;

            do
            {
                try
                {
                    await func();

                    return;
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Exception happened, retrying");
                    exception = e;
                }

                var waitForMs = basicDelayMs * (int)Math.Pow(2, counter);
                await Task.Delay(waitForMs);
                counter++;

                if (counter == retry)
                    throw exception;

            } while (true);
        }
    }
}
