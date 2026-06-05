using ATM.Application.Abstractions.Persistence;
using ATM.Application.Features.Transaction.Transfer;
using ATM.Domain.Entities;
using ATM.Domain.Errors;

namespace ATM.Application.UnitTests.Features.Transaction;

public class TransferCommandHandlerTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TransferCommandHandler _handler;

    public TransferCommandHandlerTests()
    {
        _handler = new TransferCommandHandler(_accountRepository, _transactionRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidTransfer_MovesFundsRecordsTwoEntriesAndSavesOnce()
    {
        var source = TestEntities.Account(id: 1, balance: 1000m, number: "TR001");
        var target = TestEntities.Account(id: 2, balance: 500m, number: "TR002");
        _accountRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(source);
        _accountRepository.GetByAccountNumberAsync("TR002", Arg.Any<CancellationToken>()).Returns(target);

        var result = await _handler.Handle(new TransferCommand(1, "TR002", 300m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        source.Balance.Should().Be(700m);
        target.Balance.Should().Be(800m);
        await _transactionRepository.Received(2).AddAsync(Arg.Any<Domain.Entities.Transaction>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ToSameAccount_Fails()
    {
        var source = TestEntities.Account(id: 1, balance: 1000m, number: "TR001");
        _accountRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(source);
        _accountRepository.GetByAccountNumberAsync("TR001", Arg.Any<CancellationToken>()).Returns(source);

        var result = await _handler.Handle(new TransferCommand(1, "TR001", 100m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.SameAccountTransfer);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownTarget_FailsWithTargetNotFound()
    {
        var source = TestEntities.Account(id: 1, balance: 1000m, number: "TR001");
        _accountRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(source);
        _accountRepository.GetByAccountNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Account?)null);

        var result = await _handler.Handle(new TransferCommand(1, "TR999", 100m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.TargetNotFound);
    }

    [Fact]
    public async Task Handle_WithInsufficientBalance_Fails()
    {
        var source = TestEntities.Account(id: 1, balance: 50m, number: "TR001");
        var target = TestEntities.Account(id: 2, balance: 500m, number: "TR002");
        _accountRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(source);
        _accountRepository.GetByAccountNumberAsync("TR002", Arg.Any<CancellationToken>()).Returns(target);

        var result = await _handler.Handle(new TransferCommand(1, "TR002", 300m), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.InsufficientFunds);
        source.Balance.Should().Be(50m);
        target.Balance.Should().Be(500m);
    }
}
