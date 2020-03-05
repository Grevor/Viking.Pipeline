using System;

namespace Viking.Pipeline
{
    /// <summary>
    /// Represents an error related ot the execution or setup of a pipeline.
    /// </summary>
    public class PipelineException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="PipelineException"/>.
        /// </summary>
        public PipelineException() { }
        /// <summary>
        /// Creates a new <see cref="PipelineException"/> with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PipelineException(string message) : base(message) { }
        /// <summary>
        /// Creates a new <see cref="PipelineException"/> with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PipelineException(string message, Exception innerException) : base(message, innerException) { }
    }
}
