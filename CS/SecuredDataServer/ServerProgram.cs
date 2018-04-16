using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.MiddleTier;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security.ClientServer.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using System.Data;
using DevExpress.Xpo.Metadata;
using PersistentClassesLibrary;
using System.ServiceModel;
using DevExpress.ExpressApp.Security.ClientServer.Wcf;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;

namespace DataServer {
    static class Program {
        static void Main() {
            try {
                Console.WriteLine("Starting...");
                ValueManager.ValueManagerType = typeof(MultiThreadValueManager<>).GetGenericTypeDefinition();
                XpoTypesInfoHelper.GetXpoTypeInfoSource(); 
                XafTypesInfo.Instance.RegisterEntity(typeof(Employee));
                XafTypesInfo.Instance.RegisterEntity(typeof(PermissionPolicyUser));
                XafTypesInfo.Instance.RegisterEntity(typeof(PermissionPolicyRole));
                string connectionString = @"Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\mssqllocaldb;Initial Catalog=ClientServer";
                Console.WriteLine("Creating default objects...");
                XPObjectSpaceProvider objectSpaceProvider = new XPObjectSpaceProvider(connectionString);
                objectSpaceProvider.CheckCompatibilityType = CheckCompatibilityType.DatabaseSchema;
                objectSpaceProvider.SchemaUpdateMode = SchemaUpdateMode.DatabaseAndSchema;
                IObjectSpace objectSpace = objectSpaceProvider.CreateUpdatingObjectSpace(true);
                CreateUsers(objectSpace);
                CreateObjects(objectSpace);
                objectSpace.CommitChanges();
                
                Console.WriteLine("Starting server...");
                Func<IDataServerSecurity> securityProviderHandler = () => new SecurityStrategyComplex(typeof(PermissionPolicyUser), typeof(PermissionPolicyRole), new AuthenticationStandard());

                ServiceHost serviceHost = new WcfXafServiceHost(connectionString, securityProviderHandler);
                serviceHost.AddServiceEndpoint(typeof(IWcfXafDataServer), WcfDataServerHelper.CreateNetTcpBinding(), "net.tcp://localhost:1424/DataServer");
                serviceHost.Open();

                Console.WriteLine("Server is started. Press Enter to stop.");
                Console.ReadLine();
                Console.WriteLine("Stopping...");
                serviceHost.Close();
                Console.WriteLine("Server is stopped.");
            }
            catch(Exception e) {
                Console.WriteLine("Exception occurs: " + e.Message);
                Console.WriteLine("Press Enter to close.");
                Console.ReadLine();
            }
        }

        private static void CreateObjects(IObjectSpace objectSpace) {
            Employee obj1 = objectSpace.CreateObject<Employee>();
            obj1.Name = "Sam";
            obj1.IsManager = true;

            Employee obj2 = objectSpace.CreateObject<Employee>();
            obj2.Name = "John";
            obj2.IsManager = true;

            Employee obj3 = objectSpace.CreateObject<Employee>();
            obj3.Name = "Michael";
            obj3.IsManager = false;

            Employee obj4 = objectSpace.CreateObject<Employee>();
            obj4.Name = "Peter";
            obj4.IsManager = false;
        }

        private static void CreateUsers(IObjectSpace objectSpace) {
            PermissionPolicyUser admin = objectSpace.FindObject<PermissionPolicyUser>(new BinaryOperator("UserName", "Admin"));
                admin = objectSpace.CreateObject<PermissionPolicyUser>();
                admin.UserName = "Admin";
                admin.IsActive = true;
                admin.SetPassword("");
            PermissionPolicyRole role = objectSpace.CreateObject<PermissionPolicyRole>();
                role.IsAdministrative = true;
                admin.Roles.Add(role);

                PermissionPolicyUser user = objectSpace.CreateObject<PermissionPolicyUser>();
                user.UserName = "User";
                user.IsActive = true;
                PermissionPolicyRole userRole = objectSpace.CreateObject<PermissionPolicyRole>();
                userRole.Name = "Users";
                userRole.AddObjectPermission<Employee>(SecurityOperations.Read, "[IsManager] != true", SecurityPermissionState.Allow);
                user.Roles.Add(userRole);
                objectSpace.CommitChanges();
        }
    }
}