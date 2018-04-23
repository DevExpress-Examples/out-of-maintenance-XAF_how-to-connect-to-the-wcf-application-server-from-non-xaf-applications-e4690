Imports System
Imports System.Collections.Generic
Imports System.ServiceProcess
Imports System.Text
Imports System.Configuration
Imports DevExpress.ExpressApp.Security.ClientServer
Imports DevExpress.ExpressApp.Security
Imports DevExpress.ExpressApp.MiddleTier
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Security.ClientServer.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels.Tcp
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Xpo
Imports DevExpress.ExpressApp.Security.Strategy
Imports DevExpress.Xpo
Imports DevExpress.Xpo.DB
Imports System.Data
Imports DevExpress.Xpo.Metadata
Imports PersistentClassesLibrary
Imports System.ServiceModel
Imports DevExpress.ExpressApp.Security.ClientServer.Wcf
Imports DevExpress.Data.Filtering

Namespace DataServer
    Friend NotInheritable Class Program

        Private Sub New()
        End Sub

        Shared Sub Main()
            Try
                Console.WriteLine("Starting...")
                Dim dataSet As New DataSet()
                ValueManager.ValueManagerType = GetType(MultiThreadValueManager(Of )).GetGenericTypeDefinition()

                XpoTypesInfoHelper.GetXpoTypeInfoSource()
                XafTypesInfo.Instance.RegisterEntity(GetType(Employee))
                XafTypesInfo.Instance.RegisterEntity(GetType(SecuritySystemUser))
                XafTypesInfo.Instance.RegisterEntity(GetType(SecuritySystemRole))

                Console.WriteLine("Creating default objects...")
                Dim uow As New UnitOfWork(New SimpleDataLayer(New DataSetDataStore(dataSet, AutoCreateOption.DatabaseAndSchema)))
                uow.UpdateSchema(GetType(Employee), GetType(SecuritySystemUser), GetType(SecuritySystemRole))

                CreateUsers(uow)
                CreateObjects(uow)

                uow.CommitChanges()

                Console.WriteLine("Starting server...")
                Dim securityProviderHandler As QueryRequestSecurityStrategyHandler = Function() New SecurityStrategyComplex(GetType(SecuritySystemUser), GetType(SecuritySystemRole), New AuthenticationStandard())
                Dim dataLayer As IDataLayer = New SimpleDataLayer(XpoTypesInfoHelper.GetXpoTypeInfoSource().XPDictionary, New DataSetDataStore(dataSet, AutoCreateOption.SchemaAlreadyExists))
                Dim dataServer As New SecuredDataServer(dataLayer, securityProviderHandler)

                Dim host As New ServiceHost(New WcfSecuredDataServer(dataServer))
                host.AddServiceEndpoint(GetType(IWcfSecuredDataServer), WcfDataServerHelper.CreateDefaultBinding(), "http://localhost:1424/DataServer")
                host.Open()

                Console.WriteLine("Server is started. Press Enter to stop.")
                Console.ReadLine()
                Console.WriteLine("Stopping...")
                host.Close()
                Console.WriteLine("Server is stopped.")
            Catch e As Exception
                Console.WriteLine("Exception occurs: " & e.Message)
                Console.WriteLine("Press Enter to close.")
                Console.ReadLine()
            End Try
        End Sub

        Private Shared Sub CreateObjects(ByVal uow As UnitOfWork)
            Dim obj1 As New Employee(uow)
            obj1.Name = "Sam"
            obj1.IsManager = True

            Dim obj2 As New Employee(uow)
            obj2.Name = "John"
            obj2.IsManager = True

            Dim obj3 As New Employee(uow)
            obj3.Name = "Michael"
            obj3.IsManager = False

            Dim obj4 As New Employee(uow)
            obj4.Name = "Peter"
            obj4.IsManager = False
        End Sub

        Private Shared Sub CreateUsers(ByVal uow As UnitOfWork)
            Dim admin As SecuritySystemUser = uow.FindObject(Of SecuritySystemUser)(New BinaryOperator("UserName", "Admin"))
                admin = New SecuritySystemUser(uow)
                admin.UserName = "Admin"
                admin.IsActive = True
                admin.SetPassword("")
                Dim role As New SecuritySystemRole(uow)
                role.IsAdministrative = True
                admin.Roles.Add(role)

                Dim user As New SecuritySystemUser(uow)
                user.UserName = "User"
                user.IsActive = True
                Dim userRole As New SecuritySystemRole(uow)
                userRole.Name = "Users"
                userRole.AddObjectAccessPermission(Of Employee)("[IsManager] != true", SecurityOperations.Read)
                user.Roles.Add(userRole)
                uow.CommitChanges()
        End Sub
    End Class
End Namespace