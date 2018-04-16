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
Imports DevExpress.ExpressApp
Imports System.ServiceModel.Channels

Namespace ConsoleClient
    Friend Class Program
       Private Shared Sub AccessEmployee(ByVal wcfSecuredClient As WcfSecuredClient)
            Try
                Dim dictionary As XPDictionary = New ReflectionDictionary()
                dictionary.CollectClassInfos(System.Reflection.Assembly.GetAssembly(GetType(Employee)))
                Dim isReadGranted As Boolean = wcfSecuredClient.IsGranted(New SerializablePermissionRequest(GetType(Employee), Nothing, Nothing, SecurityOperations.Read))
                Console.WriteLine(String.Format("Is read granted for Employee: {0}", isReadGranted))

                Dim isWriteGranted As Boolean = wcfSecuredClient.IsGranted(New SerializablePermissionRequest(GetType(Employee), Nothing, Nothing, SecurityOperations.Write))
                Console.WriteLine(String.Format("Is modification granted for Employee: {0}", isWriteGranted))

                Dim securedObjectLayerClient As New MiddleTierSerializableObjectLayerClient(wcfSecuredClient)
                Dim objectLayerClient As New SerializableObjectLayerClient(securedObjectLayerClient, dictionary)
                Dim uow As New UnitOfWork(objectLayerClient)
                Console.WriteLine(vbLf & "Read all available Employee objects:")
                For Each obj As Employee In New XPCollection(uow, GetType(Employee))
                    Console.WriteLine(obj.Name)
                Next obj
                Console.WriteLine(vbLf & "Try to modify object: ")
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
            Dim wcfSecuredClient As New CustomWcfSecuredClient(WcfDataServerHelper.CreateNetTcpBinding(), New EndpointAddress("net.tcp://localhost:1424/DataServer"))

            Console.WriteLine("Login as 'User' user")
            wcfSecuredClient.GetSecurityStrategyInfo()
            wcfSecuredClient.Logon("User", "")
            AccessEmployee(wcfSecuredClient)
            wcfSecuredClient.Logoff()

            Console.WriteLine(vbLf & "Login as 'Admin' user")
            wcfSecuredClient.GetSecurityStrategyInfo()
            wcfSecuredClient.Logon("Admin", "")
            AccessEmployee(wcfSecuredClient)
            wcfSecuredClient.Logoff()

            Console.WriteLine(vbLf & "Press Enter to close...")
            Console.ReadLine()
        End Sub
    End Class
    Public Class CustomWcfSecuredClient
        Inherits WcfSecuredClient

        Private logonParameters As Object
        Public Sub New(ByVal binding As Binding, ByVal remoteAddress As EndpointAddress)
            MyBase.New(binding, remoteAddress)
        End Sub
        Public Overloads Sub Logon(ByVal userName As String, ByVal password As String)
            logonParameters = New AuthenticationStandardLogonParameters(userName, password)
            MyBase.Logon()
        End Sub
        Protected Overrides Function GetLogonParameter() As Object
            Return logonParameters
        End Function
    End Class
End Namespace
