using System;
using Database;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class PollingExceptionTests
    {
        [Fact]
        public void Handler_returns_actionable_message_for_unexpected_error()
        {
            var error = new InvalidOperationException("synthetic failure");
            Assert.Equal("synthetic failure", PollingExceptionHandler.Esegui(error));
        }

        [Fact]
        public void Handler_rejects_null_exception()
        {
            Assert.Throws<ArgumentNullException>(() => PollingExceptionHandler.Esegui(null));
        }

        [Fact]
        public void Adapter_errors_can_be_rethrown_without_replacing_exception_instance()
        {
            var original = new FormatException("synthetic format failure");
            Exception observed = null;
            try
            {
                try { throw original; }
                catch (FormatException) { throw; }
            }
            catch (Exception ex)
            {
                observed = ex;
            }
            Assert.Same(original, observed);
            Assert.Contains("Adapter_errors_can_be_rethrown", observed.StackTrace);
        }
    }
}
