using Hydrology.Entity;
using Protocol.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Data.GY
{
    class DownParser : IDown
    {
        public String BuildQuery(string sid, IList<EDownParam> cmds, EChannelType ctype)
        {
            return "";
        }

        public String BuildQuery_Batch(string sid, ETrans trans, DateTime beginTime, EChannelType ctype)
        {
            return "";
        }

        public string BuildQuery_Flash(string sid, EStationType stationType, ETrans trans, DateTime beginTime, DateTime endTime, EChannelType ctype)
        {
            throw new NotImplementedException();
        }

        public string BuildQuery_SD(string sid, DateTime beginTime, EChannelType ctype)
        {
            throw new NotImplementedException();
        }

        public string BuildSet(string sid, IList<EDownParam> cmds, CDownConf down, EChannelType ctype)
        {
            throw new NotImplementedException();
        }

        public bool Parse(string resp, out CDownConf downConf)
        {
            throw new NotImplementedException();
        }

        public bool Parse_Batch(string msg, out CBatchStruct batch)
        {
            throw new NotImplementedException();
        }

        public bool Parse_Flash(string msg, EChannelType ctype, out CBatchStruct batch)
        {
            throw new NotImplementedException();
        }

        public bool Parse_SD(string msg, string id, out CSDStruct sd)
        {
            throw new NotImplementedException();
        }
    }
}
