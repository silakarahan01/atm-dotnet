using ATM.Application.Abstractions.Persistence;
using ATM.Application.Features.Transaction.Withdraw;
using ATM.Domain.Entities;
using ATM.Domain.Errors;

namespace ATM.Application.UnitTests.Features.Transaction;

public class WithdrawCommandHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly WithdrawCommandHandler _handler;

    public WithdrawCommandHandlerTests()
    {
        _handler = new WithdrawCommandHandler(_accountRepository, _transactionRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithSufficientBalance_DecreasesBalanceRecordsTransactionAndSaves()
    {
        var account = TestEntities.Account(id: 1, balance: 1000m);
        _accountRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(account);

        var result = await _handler.Handle(new WithdrawCommand(1, 400m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        account.Balance.Should().Be(600m);
        await _transactionRepository.Received(1).AddAsync(
            Arg.Is<Domain.Entities.Transaction>(t => t.Amount == 400m && t.BalanceAfter == 600m),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInsufficientBalance_FailsAndDoesNotSave()
    {
        var account = TestEntities.Account(id: 1, balance: 100m);
        _accountRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(account);

        var result = await _handler.Handle(new WithdrawCommand(1, 500m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.InsufficientFunds);
        account.Balance.Should().Be(100m);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownAccount_FailsWithNotFound()
    {
        _accountRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Account?)null);

        var result = await _handler.Handle(new WithdrawCommand(99, 100m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.NotFound);
    }
}
