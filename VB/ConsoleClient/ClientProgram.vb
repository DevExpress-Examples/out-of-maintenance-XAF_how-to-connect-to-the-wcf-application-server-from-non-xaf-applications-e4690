Imports Microsoft.VisualBasic
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
		Private Const connectionString As String = "http://localhost:1424/DataServer"
		Private Shared dictionary As XPDictionary = New ReflectionDictionary()

		Shared Sub Main(ByVal args() As String)
			Try
				Dim dictionary As XPDictionary = New ReflectionDictionary()
				dictionary.CollectClassInfos(System.Reflection.Assembly.GetAssembly(GetType(Stuff)))

				Console.WriteLine("Login as 'User' user")
				Dim statelessClientInfo As New ClientInfo(Guid.Empty, Guid.Empty, New AuthenticationStandardLogonParameters("User", ""))
				Dim clientDataServer As New WcfSecuredDataServerClient(WcfDataServerHelper.CreateDefaultBinding(), New EndpointAddress("http://localhost:1424/DataServer"))

				Dim isReadGranted As Boolean = clientDataServer.IsGranted(statelessClientInfo, New ClientPermissionRequest(GetType(Stuff), Nothing, Nothing, SecurityOperations.Read))
				Console.WriteLine(String.Format("Is read granted for Stuff: {0}", isReadGranted))

				Dim isWriteGranted As Boolean = clientDataServer.IsGranted(statelessClientInfo, New ClientPermissionRequest(GetType(Stuff), Nothing, Nothing, SecurityOperations.Write))
				Console.WriteLine(String.Format("Is modification granted for Stuff: {0}", isWriteGranted))

				Dim securedObjectLayerClient As New SecuredSerializableObjectLayerClient(statelessClientInfo, clientDataServer)
				Dim client As New SerializableObjectLayerClient(securedObjectLayerClient, dictionary)
				Dim uow As New UnitOfWork(client)
				Console.WriteLine("Read all available Stuff objects:")
				For Each obj As Stuff In New XPCollection(uow, GetType(Stuff))
					Console.WriteLine(obj.Name)
				Next obj
				Console.WriteLine("Try to modify object: ")
				Try
					Dim stuff As Stuff = uow.FindObject(Of Stuff)(New BinaryOperator("Name", "Available object"))
					Console.WriteLine("Object name: " & stuff.Name)
					stuff.Name &= "_modified"
					uow.CommitChanges()
				Catch e As Exception
					Console.WriteLine("Error occured: " & e.Message)
				End Try
			Catch e As Exception
				Console.WriteLine(e.Message & Environment.NewLine & e.StackTrace)
			End Try
			Console.WriteLine("Press Enter to close...")
			Console.ReadLine()
		End Sub
	End Class
End Namespace
