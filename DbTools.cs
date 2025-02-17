using System.Transactions;

namespace samplenpgsql;

public static class DbTools
{
    private static readonly TimeSpan _defaultTimeout = new(3, 0, 0);
    public static TransactionScope TransactionScopeAsyncCreate(
        TimeSpan? timeout = null,
        TransactionScopeOption transactionScopeOption = TransactionScopeOption.RequiresNew,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        TransactionOptions options = new()
        {
            IsolationLevel = isolationLevel,
            Timeout = timeout ?? _defaultTimeout
        };

        return new TransactionScope(transactionScopeOption, options, TransactionScopeAsyncFlowOption.Enabled);
    }

    public static TransactionScope TransactionScopeAsyncCreate(TransactionScope scope)
    {
        return TransactionScopeAsyncCreate(
            _defaultTimeout,
            scope == null ? TransactionScopeOption.RequiresNew : TransactionScopeOption.Required
        );
    }

    public static TransactionScope TransactionScopeAsyncCreate(TransactionScope scope, TransactionScopeOption? scopeOption)
    {
        scopeOption ??= scope == null ? TransactionScopeOption.RequiresNew : TransactionScopeOption.Required;

        return TransactionScopeAsyncCreate(
            _defaultTimeout,
            (TransactionScopeOption)scopeOption
        );
    }
}