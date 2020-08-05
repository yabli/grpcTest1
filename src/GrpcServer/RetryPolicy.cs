namespace Contoso.Grpc
{
    using global::Grpc.Core;
    using System;

    /// <summary>
    /// gRpc retry policy configuration data
    /// ref - https://github.com/grpc/proposal/blob/master/A6-client-retries.md#retry-policy-capabilities
    /// </summary>
    internal class RetryPolicy
    {
        /// <summary>
        /// specifies the maximum number of RPC attempts, including the original request.
        /// MUST be a JSON integer value greater than 1. Values greater than 5 are treated as 5 without being considered 
        /// a validation error.
        /// </summary>
        internal int MaxAttempts { get; }

        /// <summary>
        /// Exponential Backoff parameter
        /// The initial retry attempt will occur at random(0, initialBackoff). In general, the n-th attempt will occur at 
        /// random(0, min(initialBackoff*backoffMultiplier**(n-1), maxBackoff)).
        /// MUST follow the JSON representaion of proto3 Duration type, and MUST have a duration value greater than 0.
        /// </summary>
        internal float InitialBackoff { get; }

        /// <summary>
        /// Exponential Backoff parameter
        /// The initial retry attempt will occur at random(0, initialBackoff). In general, the n-th attempt will occur at 
        /// random(0, min(initialBackoff*backoffMultiplier**(n-1), maxBackoff)).
        /// MUST follow the JSON representaion of proto3 Duration type, and MUST have a duration value greater than 0.
        /// </summary>
        internal float MaxBackoff { get; }

        /// <summary>
        /// Exponential Backoff parameter
        /// The initial retry attempt will occur at random(0, initialBackoff). In general, the n-th attempt will occur at 
        /// random(0, min(initialBackoff*backoffMultiplier**(n-1), maxBackoff)).
        /// MUST be a JSON number greater than 0.
        /// </summary>
        internal float BackoffMultiplier { get; }

        /// <summary>
        /// When gRPC receives a non-OK response status from a server, this status is checked against the set of retryable 
        /// status codes in retryableStatusCodes to determine if a retry attempt should be made.
        /// Each status code MUST be a valid gRPC status code and specified in the integer form or the case-insensitive string form 
        /// (eg. [14], ["UNAVAILABLE"] or ["unavailable"]).
        /// We hard code the status code here, not configurable either by setter or ctor
        /// </summary>
        // internal static readonly int[] RetryableStatusCodes = { (int)StatusCode.Unavailable, (int)StatusCode.Internal };
        // internal static readonly string[] RetryableStatusCodes = { $"\"{StatusCode.Unavailable.ToString().ToUpperInvariant()}\"", $"\"{StatusCode.Internal.ToString().ToUpperInvariant()}\"" };
        internal static readonly string[] RetryableStatusCodes = { $"\"{StatusCode.Unavailable.ToString().ToUpperInvariant()}\""};
        private static readonly string StatusCodesString = string.Join(",", RetryableStatusCodes);
        
        internal RetryPolicy(int maxAttempts, float initialBackoff, float maxBackoff, float backoffMultiplier)
        {
            if (!(maxAttempts > 1 && maxAttempts <= 5))
            {
                throw new ArgumentException(nameof(maxAttempts));
            }

            if (!(initialBackoff > 0))
            {
                throw new ArgumentException(nameof(initialBackoff));
            }

            if (!(maxBackoff > 0))
            {
                throw new ArgumentException(nameof(maxBackoff));
            }

            if (!(backoffMultiplier > 0))
            {
                throw new ArgumentException(nameof(backoffMultiplier));
            }

            this.MaxAttempts = maxAttempts;
            this.InitialBackoff = initialBackoff;
            this.MaxBackoff = maxBackoff;
            this.BackoffMultiplier = backoffMultiplier;
        }

        internal string BuildPolicyString()
        {
            return $"\"retryPolicy\":{{\"maxAttempts\":{this.MaxAttempts},\"initialBackoff\":\"{this.InitialBackoff}s\",\"maxBackoff\":\"{this.MaxBackoff}s\",\"backoffMultiplier\":{this.BackoffMultiplier},\"retryableStatusCodes\":[{StatusCodesString}]}}";
        }
    }
}
