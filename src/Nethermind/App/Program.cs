using Nethermind.Evm;
using Nethermind.State;

EvmState state = EvmState.RentTopLevel(
    10_000,
    ExecutionType.CALL,
    Snapshot.Empty,
    new(),
    new());

Console.WriteLine(state.GasAvailable);
