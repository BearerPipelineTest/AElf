using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Kernel.Blockchain.Application;
using AElf.Kernel.Blockchain.Domain;
using AElf.Types;
using AElf.Kernel.Infrastructure;
using Shouldly;
using Xunit;

namespace AElf.Kernel.SmartContractExecution.Application
{
    public class FullBlockchainExecutingServiceExecuteFailedTests : ExecuteFailedTestBase
    {
        private readonly FullBlockchainExecutingService _fullBlockchainExecutingService;
        private readonly IBlockchainService _blockchainService;
        private readonly IChainManager _chainManager;
        private readonly KernelTestHelper _kernelTestHelper;

        public FullBlockchainExecutingServiceExecuteFailedTests()
        {
            _fullBlockchainExecutingService = GetRequiredService<FullBlockchainExecutingService>();
            _blockchainService = GetRequiredService<IBlockchainService>();
            _chainManager = GetRequiredService<IChainManager>();
            _kernelTestHelper = GetRequiredService<KernelTestHelper>();
        }

        [Fact]
        public async Task ExecuteBlock_ExecuteFailed()
        {
            var chain = await _blockchainService.GetChainAsync();
            var bestChainHeight = chain.BestChainHeight;
            var bestChainHash = chain.BestChainHash;

            var previousHash = chain.BestChainHash;
            var previousHeight = chain.BestChainHeight;
            var blockList = new List<Block>();
            for (var i = 0; i < 3; i++)
            {
                var transactions = new List<Transaction> {_kernelTestHelper.GenerateTransaction()};
                var lastBlock = _kernelTestHelper.GenerateBlock(previousHeight, previousHash, transactions);

                await _blockchainService.AddBlockAsync(lastBlock);
                await _blockchainService.AddTransactionsAsync(transactions);

                await _blockchainService.AttachBlockToChainAsync(chain, lastBlock);
                previousHash = lastBlock.GetHash();
                previousHeight = lastBlock.Height;
                blockList.Add(lastBlock);
            }

            var executionResult =
                await _fullBlockchainExecutingService.ExecuteBlocksAsync(blockList);

            executionResult.ExecutedSuccessBlocks.Count.ShouldBe(0);
            executionResult.ExecutedFailedBlocks.Count.ShouldBe(1);
            executionResult.ExecutedFailedBlocks[0].GetHash().ShouldBe(blockList[0].GetHash());

            chain = await _blockchainService.GetChainAsync();
            var newBlockLink = await _chainManager.GetChainBlockLinkAsync(blockList.First().GetHash());

            newBlockLink.ExecutionStatus.ShouldBe(ChainBlockLinkExecutionStatus.ExecutionFailed);
            chain.BestChainHash.ShouldBe(bestChainHash);
            chain.BestChainHeight.ShouldBe(bestChainHeight);
            chain.LongestChainHash.ShouldBe(bestChainHash);
            chain.LongestChainHeight.ShouldBe(bestChainHeight);
            chain.Branches.ShouldNotContainKey(previousHash.ToStorageKey());
        }
    }
}