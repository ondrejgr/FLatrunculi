using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Common;

namespace Latrunculi
{
    [Serializable]
    public class ModelException : Exception
    {
        public ErrorDefinitions.Error ModelError
        {
            get;
            private set;
        }

        public ModelException(ErrorDefinitions.Error error) : base(ErrorMessages.toString(error))
        {
            ModelError = error;
        }

        protected ModelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        static public T TryThrow<T>(Result<T, ErrorDefinitions.Error> result)
        {
            if (result == null)
                throw new ArgumentNullException("result", "Výsledek provedené operace je prázdný.");
            if (result.IsError)
                throw new ModelException(((Result<T, ErrorDefinitions.Error>.Error)result).Item);
            else if (result.IsSuccess)
                return ((Result<T, ErrorDefinitions.Error>.Success)result).Item;
            else
                throw new ArgumentException("Neznámý typ výsledku operace.", "result");
        }
    }
}
