using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMViewerPlugin
{
    public static class EntityTools 
    {
        internal static void LoadFromCache(Result result, string cacheString)
        {
            byte[] ba = Encoding.ASCII.GetBytes(cacheString);
            MemoryStream memoryStream = new MemoryStream(ba);
            result.Data = new DataSet();
            result.Data.ReadXml(memoryStream);
            memoryStream.Close();
            memoryStream.Dispose();
        }
    }
}
