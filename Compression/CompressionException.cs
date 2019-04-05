using System;

namespace DotNetCompression.Compression
{
  public class CompressionException : Exception
  {
    public CompressionException(string message) : base(message)
    {
    }
    public CompressionException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}
