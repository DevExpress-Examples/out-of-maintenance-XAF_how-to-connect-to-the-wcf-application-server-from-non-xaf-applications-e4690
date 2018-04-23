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

namespace DataServer {
    static class Program {
        static void Main() {
            try {
                Console.WriteLine("Starting...");
                DataSet dataSet = new DataSet();
                ValueManager.ValueManagerType = typeof(MultiThreadValueManager<>).GetGenericTypeDefinition();

                XpoTypesInfoHelper.GetXpoTypeInfoSource(); 
                XafTypesInfo.Instance.RegisterEntity(typeof(Employee));
                XafTypesInfo.Instance.RegisterEntity(typeof(SecuritySystemUser));
                XafTypesInfo.Instance.RegisterEntity(typeof(SecuritySystemRole));

                Console.WriteLine("Creating default objects...");
                UnitOfWork uow = new UnitOfWork(new SimpleDataLayer(new DataSetDataStore(dataSet, AutoCreateOption.DatabaseAndSchema)));
                uow.UpdateSchema(typeof(Employee), typeof(SecuritySystemUser), typeof(SecuritySystemRole));

                CreateUsers(uow);
                CreateObjects(uow);
                
                uow.CommitChanges();

                Console.WriteLine("Starting server...");
                QueryRequestSecurityStrategyHandler securityProviderHandler = delegate() {
                    return new SecurityStrategyComplex(typeof(SecuritySystemUser), typeof(SecuritySystemRole), new AuthenticationStandard());
                };
                IDataLayer dataLayer = new SimpleDataLayer(XpoTypesInfoHelper.GetXpoTypeInfoSource().XPDictionary, new DataSetDataStore(dataSet, AutoCreateOption.SchemaAlreadyExists));
                SecuredDataServer dataServer = new SecuredDataServer(dataLayer, securityProviderHandler);

                ServiceHost host = new ServiceHost(new WcfSecuredDataServer(dataServer));
                host.AddServiceEndpoint(typeof(IWcfSecuredDataServer), WcfDataServerHelper.CreateDefaultBinding(), "http://localhost:1424/DataServer");
                host.Open();

                Console.WriteLine("Server is started. Press Enter to stop.");
                Console.ReadLine();
                Console.WriteLine("Stopping...");
                host.Close();
                Console.WriteLine("Server is stopped.");
            }
            catch(Exception e) {
                Console.WriteLine("Exception occurs: " + e.Message);
                Console.WriteLine("Press Enter to close.");
                Console.ReadLine();
            }
        }

        private static void CreateObjects(UnitOfWork uow) {
            Employee obj1 = new Employee(uow);
            obj1.Name = "Sam";
            obj1.IsManager = true;

            Employee obj2 = new Employee(uow);
            obj2.Name = "John";
            obj2.IsManager = true;

            Employee obj3 = new Employee(uow);
            obj3.Name = "Michael";
            obj3.IsManager = false;

            Employee obj4 = new Employee(uow);
            obj4.Name = "Peter";
            obj4.IsManager = false;
        }

        private static void CreateUsers(UnitOfWork uow) {
            SecuritySystemUser admin = uow.FindObject<SecuritySystemUser>(new BinaryOperator("UserName", "Admin"));
                admin = new SecuritySystemUser(uow);
                admin.UserName = "Admin";
                admin.IsActive = true;
                admin.SetPassword("");
                SecuritySystemRole role = new SecuritySystemRole(uow);
                role.IsAdministrative = true;
                admin.Roles.Add(role);

                SecuritySystemUser user = new SecuritySystemUser(uow);
                user.UserName = "User";
                user.IsActive = true;
                SecuritySystemRole userRole = new SecuritySystemRole(uow);
                userRole.Name = "Users";
                userRole.AddObjectAccessPermission<Employee>("[IsManager] != true", SecurityOperations.Read);
                user.Roles.Add(userRole);
                uow.CommitChanges();
        }
    }
}