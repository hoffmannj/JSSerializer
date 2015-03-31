using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSerializer
{
    public interface IDeserializer
    {
        T Deserialize<T>(string json);
        dynamic DeserializeToDynamic(string json);
    }
}
