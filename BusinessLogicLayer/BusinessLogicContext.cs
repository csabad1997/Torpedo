using DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer
{
    public partial class BusinessLogicContext : IDisposable
    {
        private SQLContext context { get; set; }
        public BusinessLogicContext()
        {
            context = new SQLContext();
        }

        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
            }
        }
    }
}
