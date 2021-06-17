namespace Oddin.Oddin.SDK.API
{
    internal class RequestResult<TData> : IRequestResult<TData>
        where TData : class
    {
        public TData Data { get; set; }

        public bool Successful { get; set; }

        public string Message { get; set; }

        public static RequestResult<TData> Success(TData data, string successMessage = "")
            => new RequestResult<TData>()
            {
                Data = data,
                Successful = true,
                Message = successMessage
            };

        public static RequestResult<TData> Failure(string failureMessage = "", TData data = default)
            => new RequestResult<TData>()
            {
                Data = data,
                Successful = false,
                Message = failureMessage
            };
    }

    public interface IRequestResult<TData>
        where TData : class
    {
        /// <summary>
        /// Deserialized content of response to the request
        /// </summary>
        TData Data { get; }

        /// <summary>
        /// <see cref="true"/> if the request was successful
        /// </summary>
        bool Successful { get; }

        /// <summary>
        /// Details of request success of failure
        /// </summary>
        string Message { get; }
    }
}
