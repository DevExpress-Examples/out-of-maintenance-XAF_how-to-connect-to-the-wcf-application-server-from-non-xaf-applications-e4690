Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports DevExpress.Xpo

Namespace PersistentClassesLibrary
    Public Class Employee
        Inherits XPObject

        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub
        Public Property Name() As String
            Get
                Return GetPropertyValue(Of String)("Name")
            End Get
            Set(ByVal value As String)
                SetPropertyValue(Of String)("Name", value)
            End Set
        End Property
        Public Property IsManager() As Boolean
            Get
                Return GetPropertyValue(Of Boolean)("IsManager")
            End Get
            Set(ByVal value As Boolean)
                SetPropertyValue(Of Boolean)("IsManager", value)
            End Set
        End Property
    End Class
End Namespace
