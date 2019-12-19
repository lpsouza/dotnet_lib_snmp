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
        public Task<IList<Variable>> Get()
        {
            Task<IList<Variable>> result = null;
            try
            {
                result = Messenger.GetAsync(SnmpVersion, Ip, Community, Oids);
                Oids = new List<Variable>();
            }
            catch (System.Exception err)
            {
                Console.WriteLine("#####");
                Console.WriteLine("METHOD: Get");
                Console.WriteLine("ERROR: {0}", err.Message);
                foreach (var oid in Oids)
                {
                    Console.WriteLine("OID: {0}", oid);
                }
                Console.WriteLine("#####");
                throw;
            }
            return result;
        }
        public IList<Variable> GetBulk(int maxRepetitions)
        {
            Task<ISnmpMessage> result = null;
            try
            {
                GetBulkRequestMessage message = new GetBulkRequestMessage(0, SnmpVersion, Community, 0, maxRepetitions, Oids);
                Oids = new List<Variable>();
                result = message.GetResponseAsync(Ip);
            }
            catch (System.Exception err)
            {
                Console.WriteLine("#####");
                Console.WriteLine("METHOD: GetBulk");
                Console.WriteLine("ERROR: {0}", err.Message);
                foreach (var oid in Oids)
                {
                    Console.WriteLine("OID: {0}", oid);
                }
                Console.WriteLine("#####");
                throw;
            }
            return result.Result.Variables();
        }
    }
}
