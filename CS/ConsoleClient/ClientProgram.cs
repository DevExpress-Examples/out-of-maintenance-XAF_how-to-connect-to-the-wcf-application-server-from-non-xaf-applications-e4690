using DevExpress.ExpressApp.Security.ClientServer.Wcf;
using System.ServiceModel;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo.Metadata;
using System.Reflection;
using PersistentClassesLibrary;
using System;
using DevExpress.Xpo;
using System.Collections.Generic;
using DevExpress.Data.Filtering;
using System.Configuration;
using DevExpress.ExpressApp;
using System.ServiceModel.Channels;

namespace ConsoleClient {
    class Program {
       static void AccessEmployee(WcfSecuredClient wcfSecuredClient) {
            try {
                XPDictionary dictionary = new ReflectionDictionary();
                dictionary.CollectClassInfos(Assembly.GetAssembly(typeof(Employee)));
                bool isReadGranted = wcfSecuredClient.IsGranted(new SerializablePermissionRequest(typeof(Employee), null, null, SecurityOperations.Read));
                Console.WriteLine(string.Format("Is read granted for Employee: {0}", isReadGranted));

                bool isWriteGranted = wcfSecuredClient.IsGranted(new SerializablePermissionRequest(typeof(Employee), null, null, SecurityOperations.Write));
                Console.WriteLine(string.Format("Is modification granted for Employee: {0}", isWriteGranted));

                MiddleTierSerializableObjectLayerClient securedObjectLayerClient = new MiddleTierSerializableObjectLayerClient(wcfSecuredClient);
                SerializableObjectLayerClient objectLayerClient = new SerializableObjectLayerClient(securedObjectLayerClient, dictionary);
                UnitOfWork uow = new UnitOfWork(objectLayerClient);
                Console.WriteLine("\nRead all available Employee objects:");
                foreach (Employee obj in new XPCollection(uow, typeof(Employee))) {
                    Console.WriteLine(obj.Name);
                }
                Console.WriteLine("\nTry to modify object: ");
                try {
                    Employee employee = uow.FindObject<Employee>(new BinaryOperator("Name", "Peter"));
                    Console.WriteLine("Object name: " + employee.Name);
                    employee.Name += "_modified";
                    uow.CommitChanges();
                    Console.WriteLine("New object name: " + employee.Name);
                }
                catch (Exception e) {
                    Console.WriteLine("Error occured: " + e.Message);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
            Console.WriteLine();
        }

        static void Main(string[] args) {
            CustomWcfSecuredClient wcfSecuredClient = new CustomWcfSecuredClient(
                WcfDataServerHelper.CreateNetTcpBinding(), new EndpointAddress("net.tcp://localhost:1424/DataServer"));
                       
            Console.WriteLine("Login as 'User' user");
            wcfSecuredClient.GetSecurityStrategyInfo();
            wcfSecuredClient.Logon("User", "");
            AccessEmployee(wcfSecuredClient);
            wcfSecuredClient.Logoff();

            Console.WriteLine("\nLogin as 'Admin' user");
            wcfSecuredClient.GetSecurityStrategyInfo();
            wcfSecuredClient.Logon("Admin", "");
            AccessEmployee(wcfSecuredClient);
            wcfSecuredClient.Logoff();

            Console.WriteLine("\nPress Enter to close...");
            Console.ReadLine();
        }
    }
    public class CustomWcfSecuredClient : WcfSecuredClient {
        private object logonParameters;
        public CustomWcfSecuredClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) {
        }
        public void Logon(string userName, string password) {
            logonParameters = new AuthenticationStandardLogonParameters(userName, password);
            base.Logon();
        }
        protected override object GetLogonParameter() {
            return logonParameters;
        }
    }
}
