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
namespace ConsoleClient {
    class Program {

       static void AccessEmployee(WcfSecuredDataServerClient clientDataServer, ClientInfo statelessClientInfo) {
            try {
                XPDictionary dictionary = new ReflectionDictionary();
                dictionary.CollectClassInfos(Assembly.GetAssembly(typeof(Employee)));
                bool isReadGranted = clientDataServer.IsGranted(statelessClientInfo, new SerializablePermissionRequest(typeof(Employee), null, null, SecurityOperations.Read));
                Console.WriteLine(string.Format("Is read granted for Employee: {0}", isReadGranted));

                bool isWriteGranted = clientDataServer.IsGranted(statelessClientInfo, new SerializablePermissionRequest(typeof(Employee), null, null, SecurityOperations.Write));
                Console.WriteLine(string.Format("Is modification granted for Employee: {0}", isWriteGranted));

                SecuredSerializableObjectLayerClient securedObjectLayerClient = new SecuredSerializableObjectLayerClient(statelessClientInfo, clientDataServer);
                SerializableObjectLayerClient client = new SerializableObjectLayerClient(securedObjectLayerClient, dictionary);
                UnitOfWork uow = new UnitOfWork(client);
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
            WcfSecuredDataServerClient clientDataServer = new WcfSecuredDataServerClient(
                WcfDataServerHelper.CreateDefaultBinding(), new EndpointAddress("http://localhost:1424/DataServer"));
            
            Console.WriteLine("Login as 'User' user");
            ClientInfo statelessClientInfo1 = new ClientInfo(Guid.Empty, Guid.Empty, new AuthenticationStandardLogonParameters("User", ""));
            AccessEmployee(clientDataServer, statelessClientInfo1);

            Console.WriteLine("\nLogin as 'Admin' user");
            ClientInfo statelessClientInfo2 = new ClientInfo(Guid.Empty, Guid.Empty, new AuthenticationStandardLogonParameters("Admin", ""));
            AccessEmployee(clientDataServer, statelessClientInfo2);

            Console.WriteLine("\nPress Enter to close...");
            Console.ReadLine();
        }
    }
}
