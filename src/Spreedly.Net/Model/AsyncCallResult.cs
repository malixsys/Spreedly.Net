namespace Spreedly.Net.Model
{
    public class AsyncCallResult<T>
        where T : class
    {
        private readonly AsyncCallFailureReason _asyncCallFailureReason;

        public AsyncCallResult(AsyncCallFailureReason asyncCallFailureReason, T result = null)
        {
            _asyncCallFailureReason = asyncCallFailureReason;
            Contents = result;
        }

        public T Contents { get; private set; }

        public AsyncCallFailureReason FailureReason
        {
            get { return _asyncCallFailureReason; }
        }
    }

    public enum AsyncCallFailureReason
    {
        None,
        TimeOut,
        ResultsNotFound,
        FailedStatusCode,
        FailedConnection
    }
}