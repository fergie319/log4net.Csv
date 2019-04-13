using log4net.Appender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace log4net.Csv
{
    public class HeaderOnceAppender : RollingFileAppender
    {
        protected override void WriteHeader()
        {
            try
            {
                if (LockingModel.AcquireLock().Length == 0)
                {
                    base.WriteHeader();
                }
            }
            finally
            {
                LockingModel.ReleaseLock();
            }
        }
    }
}
