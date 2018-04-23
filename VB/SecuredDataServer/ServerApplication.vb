Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports DevExpress.ExpressApp.MiddleTier
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Xpo
Imports XafModule
Imports DevExpress.Xpo
Imports DevExpress.Xpo.DB
Imports DevExpress.Xpo.Metadata

Namespace DataServer
	Public Class ExampleServerApplication
		Inherits ServerApplication
		Public Sub New()
			ApplicationName = "ServerUsageExample"
			Me.Modules.Add(New ExampleModule())
		End Sub
		Protected Overrides Sub CreateDefaultObjectSpaceProvider(ByVal args As CreateCustomObjectSpaceProviderEventArgs)
			args.ObjectSpaceProvider = New XPObjectSpaceProvider(args.ConnectionString, args.Connection)
		End Sub
	End Class
End Namespace
