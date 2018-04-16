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
Imports DevExpress.Persistent.BaseImpl.PermissionPolicy

Namespace DataServer
    Friend NotInheritable Class Program

        Private Sub New()
        End Sub

        Shared Sub Main()
            Try
                Console.WriteLine("Starting...")
                ValueManager.ValueManagerType = GetType(MultiThreadValueManager(Of )).GetGenericTypeDefinition()
                XpoTypesInfoHelper.GetXpoTypeInfoSource()
                XafTypesInfo.Instance.RegisterEntity(GetType(Employee))
                XafTypesInfo.Instance.RegisterEntity(GetType(PermissionPolicyUser))
                XafTypesInfo.Instance.RegisterEntity(GetType(PermissionPolicyRole))
                Dim connectionString As String = "Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\mssqllocaldb;Initial Catalog=ClientServer"
                Console.WriteLine("Creating default objects...")
                Dim objectSpaceProvider As New XPObjectSpaceProvider(connectionString)
                objectSpaceProvider.CheckCompatibilityType = CheckCompatibilityType.DatabaseSchema
                objectSpaceProvider.SchemaUpdateMode = SchemaUpdateMode.DatabaseAndSchema
                Dim objectSpace As IObjectSpace = objectSpaceProvider.CreateUpdatingObjectSpace(True)
                CreateUsers(objectSpace)
                CreateObjects(objectSpace)
                objectSpace.CommitChanges()

                Console.WriteLine("Starting server...")
                Dim securityProviderHandler As Func(Of IDataServerSecurity) = Function() New SecurityStrategyComplex(GetType(PermissionPolicyUser), GetType(PermissionPolicyRole), New AuthenticationStandard())

                Dim serviceHost As ServiceHost = New WcfXafServiceHost(connectionString, securityProviderHandler)
                serviceHost.AddServiceEndpoint(GetType(IWcfXafDataServer), WcfDataServerHelper.CreateNetTcpBinding(), "net.tcp://localhost:1424/DataServer")
                serviceHost.Open()

                Console.WriteLine("Server is started. Press Enter to stop.")
                Console.ReadLine()
                Console.WriteLine("Stopping...")
                serviceHost.Close()
                Console.WriteLine("Server is stopped.")
            Catch e As Exception
                Console.WriteLine("Exception occurs: " & e.Message)
                Console.WriteLine("Press Enter to close.")
                Console.ReadLine()
            End Try
        End Sub

        Private Shared Sub CreateObjects(ByVal objectSpace As IObjectSpace)
            Dim obj1 As Employee = objectSpace.CreateObject(Of Employee)()
            obj1.Name = "Sam"
            obj1.IsManager = True

            Dim obj2 As Employee = objectSpace.CreateObject(Of Employee)()
            obj2.Name = "John"
            obj2.IsManager = True

            Dim obj3 As Employee = objectSpace.CreateObject(Of Employee)()
            obj3.Name = "Michael"
            obj3.IsManager = False

            Dim obj4 As Employee = objectSpace.CreateObject(Of Employee)()
            obj4.Name = "Peter"
            obj4.IsManager = False
        End Sub

        Private Shared Sub CreateUsers(ByVal objectSpace As IObjectSpace)
            Dim admin As PermissionPolicyUser = objectSpace.FindObject(Of PermissionPolicyUser)(New BinaryOperator("UserName", "Admin"))
                admin = objectSpace.CreateObject(Of PermissionPolicyUser)()
                admin.UserName = "Admin"
                admin.IsActive = True
                admin.SetPassword("")
            Dim role As PermissionPolicyRole = objectSpace.CreateObject(Of PermissionPolicyRole)()
                role.IsAdministrative = True
                admin.Roles.Add(role)

                Dim user As PermissionPolicyUser = objectSpace.CreateObject(Of PermissionPolicyUser)()
                user.UserName = "User"
                user.IsActive = True
                Dim userRole As PermissionPolicyRole = objectSpace.CreateObject(Of PermissionPolicyRole)()
                userRole.Name = "Users"
                userRole.AddObjectPermission(Of Employee)(SecurityOperations.Read, "[IsManager] != true", SecurityPermissionState.Allow)
                user.Roles.Add(userRole)
                objectSpace.CommitChanges()
        End Sub
    End Class
End Namespace