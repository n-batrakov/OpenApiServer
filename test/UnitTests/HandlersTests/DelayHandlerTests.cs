using System.Diagnostics;

using OpenApiServer.Core.MockServer.Handlers.Defaults;

using UnitTests.Utils;

using Xunit;

namespace UnitTests.HandlersTests
{
    public class DelayHandlerTests
    {
        [Fact]
        public void CanDelayRequest()
        {
            const int delayMilliseconds = 10;
            var sut = new DelayHandler(new DelayHandler.Options {Value = delayMilliseconds});
            var ctx = RouteContextBuilder.FromUrl("/").Build();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var actual = sut.HandleAsync(ctx).Result;

            stopwatch.Stop();

            Assert.Null(actual);
            Assert.InRange(stopwatch.ElapsedMilliseconds, delayMilliseconds, long.MaxValue);
        }
    }
}