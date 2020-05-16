using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Repositorio
{
    [Serializable]
    public class EntidadReferenciadaException : Exception
    {
        public EntidadReferenciadaException()
        {
        }

        public EntidadReferenciadaException(string message) : base(message)
        {
        }

        public EntidadReferenciadaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntidadReferenciadaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
