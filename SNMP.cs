using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

namespace dotnet_lib_snmp
{
    class SNMP
    {
        private IPEndPoint Ip { get; set; }
        private OctetString Community { get; set; }
        public int Timeout { get; set; }
        public VersionCode SnmpVersion { get; set; }
        private IList<Variable> Oids { get; set; }
        public SNMP(string ip, string community)
        {
            Ip = new IPEndPoint(IPAddress.Parse(ip), 161);
            Community = new OctetString(community);
            Timeout = 2000;
            SnmpVersion = VersionCode.V2;
            Oids = new List<Variable>();
        }

        public IList<Variable> AddOid(string oid)
        {
            Oids.Add(new Variable(new ObjectIdentifier(oid)));
            return Oids;
        }
        public async Task<IList<Variable>> Get()
        {
            IList<Variable> result = null;
            result = await Messenger.GetAsync(SnmpVersion, Ip, Community, Oids);
            Oids = new List<Variable>();
            return result;
        }
        public async Task<IList<Variable>> GetBulk(int maxRepetitions)
        {
            ISnmpMessage result = null;
            GetBulkRequestMessage message = new GetBulkRequestMessage(0, SnmpVersion, Community, 0, maxRepetitions, Oids);
            Oids = new List<Variable>();
            result = await message.GetResponseAsync(Ip);
            return result.Variables();
        }
    }
}
