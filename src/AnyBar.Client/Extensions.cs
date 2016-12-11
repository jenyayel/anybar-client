using System;
using System.Text;

namespace AnyBar
{
    internal static class Extensions
    {
        public static byte[] ToByteArray(this AnyBarImage self)
        {
            return Encoding.UTF8.GetBytes(self.ToString().ToLowerInvariant());
        }
    }
}
