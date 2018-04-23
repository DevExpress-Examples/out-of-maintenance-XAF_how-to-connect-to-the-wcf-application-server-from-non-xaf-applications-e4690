Imports DevExpress.ExpressApp.Security.ClientServer.Wcf
Imports System.ServiceModel
Imports DevExpress.ExpressApp.Security.ClientServer
Imports DevExpress.ExpressApp.Security
Imports DevExpress.Xpo.Metadata
Imports System.Reflection
Imports PersistentClassesLibrary
Imports System
Imports DevExpress.Xpo
Imports System.Collections.Generic
Imports DevExpress.Data.Filtering
Imports System.Configuration
Namespace ConsoleClient
    Friend Class Program

       Private Shared Sub AccessEmployee(ByVal clientDataServer As WcfSecuredDataServerClient, ByVal statelessClientInfo As ClientInfo)
            Try
                Dim dictionary As XPDictionary = New ReflectionDictionary()
                dictionary.CollectClassInfos(System.Reflection.Assembly.GetAssembly(GetType(Employee)))
                Dim isReadGranted As Boolean = clientDataServer.IsGranted(statelessClientInfo, New SerializablePermissionRequest(GetType(Employee), Nothing, Nothing, SecurityOperations.Read))
                Console.WriteLine(String.Format("Is read granted for Employee: {0}", isReadGranted))

                Dim isWriteGranted As Boolean = clientDataServer.IsGranted(statelessClientInfo, New SerializablePermissionRequest(GetType(Employee), Nothing, Nothing, SecurityOperations.Write))
                Console.WriteLine(String.Format("Is modification granted for Employee: {0}", isWriteGranted))

                Dim securedObjectLayerClient As New SecuredSerializableObjectLayerClient(statelessClientInfo, clientDataServer)
                Dim client As New SerializableObjectLayerClient(securedObjectLayerClient, dictionary)
                Dim uow As New UnitOfWork(client)
                Console.WriteLine(ControlChars.Lf & "Read all available Employee objects:")
                For Each obj As Employee In New XPCollection(uow, GetType(Employee))
                    Console.WriteLine(obj.Name)
                Next obj
                Console.WriteLine(ControlChars.Lf & "Try to modify object: ")
                Try
                    Dim employee As Employee = uow.FindObject(Of Employee)(New BinaryOperator("Name", "Peter"))
                    Console.WriteLine("Object name: " & employee.Name)
                    employee.Name &= "_modified"
                    uow.CommitChanges()
                    Console.WriteLine("New object name: " & employee.Name)
                Catch e As Exception
                    Console.WriteLine("Error occured: " & e.Message)
                End Try
            Catch e As Exception
                Console.WriteLine(e.Message & Environment.NewLine & e.StackTrace)
            End Try
            Console.WriteLine()
       End Sub

        Shared Sub Main(ByVal args() As String)
            Dim clientDataServer As New WcfSecuredDataServerClient(WcfDataServerHelper.CreateDefaultBinding(), New EndpointAddress("http://localhost:1424/DataServer"))

            Console.WriteLine("Login as 'User' user")
            Dim statelessClientInfo1 As New ClientInfo(Guid.Empty, Guid.Empty, New AuthenticationStandardLogonParameters("User", ""))
            AccessEmployee(clientDataServer, statelessClientInfo1)

            Console.WriteLine(ControlChars.Lf & "Login as 'Admin' user")
            Dim statelessClientInfo2 As New ClientInfo(Guid.Empty, Guid.Empty, New AuthenticationStandardLogonParameters("Admin", ""))
            AccessEmployee(clientDataServer, statelessClientInfo2)

            Console.WriteLine(ControlChars.Lf & "Press Enter to close...")
            Console.ReadLine()
        End Sub
    End Class
End Namespace
