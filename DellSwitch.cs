using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

namespace dotnet_lib_snmp
{
    public class DellSwitch
    {
        private SNMP snmp { get; set; }
        public DellSwitch(string ip, string community)
        {
            snmp = new SNMP(ip, community);
        }
        public IList<Interface> GetInterfaceInfo()
        {
            IList<Interface> interfaces = new List<Interface>();

            snmp.AddOid("1.3.6.1.2.1.2.1.0"); // ifNumber
            snmp.AddOid("1.3.6.1.4.1.674.10895.3000.1.2.100.1.0"); // productIdentification
            Task<IList<Variable>> ifInfo = snmp.Get();
            int maxRepetitions = int.Parse(ifInfo.Result[0].Data.ToString());
            string productIdentification = ifInfo.Result[1].Data.ToString();

            bool isS4048T = (productIdentification.Contains("S4048T-ON")) ? true : false;

            maxRepetitions = (isS4048T) ? maxRepetitions++ : maxRepetitions;

            snmp.AddOid("1.3.6.1.2.1.2.2.1.1"); // ifIndex
            IList<Variable> idList = snmp.GetBulk(maxRepetitions);

            int iid = 1;
            foreach (var idItem in idList)
            {
                string id = idItem.Data.ToString();

                snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.2.2.1.3", id })); // ifType
                Task<IList<Variable>> type = snmp.Get();

                if (type.Result[0].Data.ToString() == "6") // ethernet-csmacd
                {
                    interfaces.Add(new Interface() { Id = iid });
                    Interface iface = interfaces.Where(a => a.Id == iid).FirstOrDefault();
                    iid++;

                    snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.31.1.1.1.1", id })); // ifName
                    snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.31.1.1.1.18", id })); // ifAlias
                    snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.2.2.1.7", id })); // ifAdminStatus
                    snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.2.2.1.8", id })); // ifOperStatus
                    snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.2.2.1.5", id })); // ifSpeed
                    snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.31.1.1.1.15", id })); // ifHighSpeed
                    snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.31.1.1.1.6", id })); // ifHCInOctets
                    snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.31.1.1.1.10", id })); // ifHCOutOctets

                    Task<IList<Variable>> info = snmp.Get();

                    iface.Name = info.Result[0].Data.ToString();
                    if (isS4048T)
                    {
                        iface.Description = (info.Result[1].Data.ToString() == "\0\0") ? "\"No description\"" : info.Result[1].Data.ToString();
                    }
                    else
                    {
                        iface.Description = (info.Result[1].Data.ToString() == string.Empty) ? "\"No description\"" : string.Format("\"{0}\"", info.Result[1].Data.ToString());
                    }
                    iface.AdminStatus = int.Parse(info.Result[2].Data.ToString());
                    iface.OperStatus = int.Parse(info.Result[3].Data.ToString());

                    UInt64 speed = UInt64.Parse(info.Result[4].Data.ToString());
                    iface.Speed = (speed >= 4294967295) ? UInt64.Parse(info.Result[5].Data.ToString()) : (speed / 1000000);

                    iface.InOctets = UInt64.Parse(info.Result[6].Data.ToString());
                    iface.OutOctets = UInt64.Parse(info.Result[7].Data.ToString());
                }

            }

            return interfaces;
        }
    }
}
