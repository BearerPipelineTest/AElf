using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Kernel.SmartContract.Application;

namespace AElf.Kernel.BlockTransactionLimitController.Tests
{
    public class OptionalLogEventListeningService<T> : ILogEventListeningService<T> where T:ILogEventHandler
    {
        private LogEventListeningService<T> _inner;

        public OptionalLogEventListeningService(LogEventListeningService<T> inner)
        {
            _inner = inner;
        }

        public static bool Enabled { get; set; }

        public async Task ProcessAsync(IEnumerable<Block> blocks)
        {
            if (Enabled)
            {
                await _inner.ProcessAsync(blocks);
            }
        }
    }
}