using System.Threading.Tasks;

namespace DotNetCompression.Compression
{
  public interface ICompressionService
  {
    Task CompressAsync();
  }
}
