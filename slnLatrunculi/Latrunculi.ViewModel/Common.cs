using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Latrunculi
{
    static public class Common
    {
        static public string ConvertExceptionToString(Exception exc)
        {
            if (exc == null)
                throw new ArgumentNullException("exc");

            StringBuilder sbMessages = new StringBuilder();
            while (exc != null)
            {
                sbMessages.AppendFormat("{0} ({1}){2}", exc.Message, exc.GetType().Name, Environment.NewLine);
                exc = exc.InnerException;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Při běhu aplikace Latrunculi došlo k neočekávané výjimce:{0}{1}", Environment.NewLine, sbMessages);

            return sb.ToString().TrimEnd();
        }
    }
}
