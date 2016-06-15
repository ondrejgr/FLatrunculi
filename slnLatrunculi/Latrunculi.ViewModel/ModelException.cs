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
        public ModelException(object result) : base(ErrorMessages.toString(result))
        {
        }

        protected ModelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        static public T TryThrow<T,U>(Result<T, U> result)
        {
            if (result == null)
                throw new ArgumentNullException("result", "Výsledek provedené operace je prázdný.");
            if (result.IsError)
                throw new ModelException(result);
            else
                return ((Result<T, U>.Success)result).Item;
        }
    }
}
