
using System;

namespace JetBlue.ESE.Net
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string key)
          : base("A document with id `" + key + "` was not found.")
        {
        }
    }
}
